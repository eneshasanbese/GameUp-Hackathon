using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

public static class EnemyPrefabBuilder
{
    private const string PrefabFolder = "Assets/Prefabs/Enemies";
    private const string AnimationFolder = "Assets/Animations/Enemies";
    private const string SpriteFolder = "Assets/platformerPack/Sprites/Enemies/";

    [MenuItem("GameUp/Enemies/Configure Terrain Physics")]
    public static void ConfigureTerrainPhysics()
    {
        foreach (string name in new[] { "Saw", "SlimeFire", "HapsirikKoz" })
        {
            foreach (string variant in new[] { "Stationary", "Patrol" })
            {
                string path = PrefabFolder + "/" + name + "_" + variant + ".prefab";
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    ConfigureBody(root, name == "Saw", variant == "Stationary");
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }
        AssetDatabase.SaveAssets();
    }

    public static void ConfigureBody(GameObject root, bool flying, bool stationary)
    {
        Rigidbody2D body = root.GetComponent<Rigidbody2D>();
        body.bodyType = flying && stationary ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        body.gravityScale = flying ? 0f : 1f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation |
            (!flying && stationary ? RigidbodyConstraints2D.FreezePositionX : RigidbodyConstraints2D.None);
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        EnemyPatrol patrol = root.GetComponent<EnemyPatrol>();
        if (patrol != null)
        {
            var settings = new SerializedObject(patrol);
            settings.FindProperty("flying").boolValue = flying;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }

        // Keep damage on the original root trigger. Only this separate shape blocks terrain;
        // it must never push the player or another enemy, or block a heat/smoke trigger.
        Transform existing = root.transform.Find("TerrainBody");
        GameObject terrain = existing != null ? existing.gameObject : new GameObject("TerrainBody");
        terrain.transform.SetParent(root.transform, false);
        Collider2D contact = root.GetComponent<Collider2D>();
        Collider2D solid;
        Vector2 offset = contact.offset;
        if (contact is CircleCollider2D circle)
        {
            var shape = terrain.GetComponent<CircleCollider2D>();
            if (shape == null) shape = terrain.AddComponent<CircleCollider2D>();
            shape.radius = circle.radius;
            solid = shape;
        }
        else
        {
            var shape = terrain.GetComponent<BoxCollider2D>();
            if (shape == null) shape = terrain.AddComponent<BoxCollider2D>();
            Vector2 size = ((BoxCollider2D)contact).size;
            // Ground sprite roots are at the feet. A deliberately shortened damage trigger
            // must not make the visible feet sink into the floor.
            float bottom = Mathf.Min(0f, offset.y - size.y * 0.5f);
            float top = offset.y + size.y * 0.5f;
            shape.size = new Vector2(size.x, top - bottom);
            offset.y = (top + bottom) * 0.5f;
            solid = shape;
        }
        solid.offset = offset;
        solid.isTrigger = false;
        int groundLayers = LayerMask.GetMask("Ground-Dry", "Ground-Wet", "Ground-Extra-Dry");
        if (groundLayers == 0) throw new InvalidOperationException("Enemy physics requires the ground layers.");
        solid.includeLayers = groundLayers;
        solid.excludeLayers = ~groundLayers;
        solid.layerOverridePriority = 1;
        solid.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/platformerPack/Material/Slip.physicsMaterial2D");
        if (solid.sharedMaterial == null) throw new InvalidOperationException("Missing Slip physics material.");
    }

    [MenuItem("GameUp/Enemies/Rebuild Enemy Prefabs")]
    public static void Rebuild()
    {
        EnsureFolder(PrefabFolder);
        EnsureFolder(AnimationFolder);
        // Both saw variants keep spinning; use the same timing as the dedicated Saw rebuild.
        AnimatorController saw = CreateSawAnimation();
        AnimatorController idle = CreateAnimation("SlimeFireIdle", "FireSlime/Unity_256/Idle.png", 12, 0.9f);
        AnimatorController walk = CreateAnimation("SlimeFireWalk", "FireSlime/Unity_256/Patrol.png", 12, 0.7f);
        CreatePrefab("Saw_Stationary", true, false, saw);
        CreatePrefab("Saw_Patrol", true, true, saw);
        CreatePrefab("SlimeFire_Stationary", false, false, idle);
        CreatePrefab("SlimeFire_Patrol", false, true, walk);
        HapsirikKozPrefabBuilder.Rebuild();
        AssetDatabase.SaveAssets();
        Debug.Log("Created six enemy prefabs with contact knockback and timed HapsirikKoz smoke.");
    }

    [MenuItem("GameUp/Enemies/Rebuild Saw Animation")]
    public static void RebuildSawAnimation()
    {
        EnsureFolder(AnimationFolder);
        CreateSawAnimation();
        AssetDatabase.SaveAssets();
    }

    private static AnimatorController CreateSawAnimation()
    {
        // Preserve the authored 12 sprite changes/sec independently of the user's 90-sample editor grid.
        const float duration = 16f / 12f;
        AnimatorController controller = CreateAnimation("SawSpin", "Saw 1/Unity_256/Patrol.png", 16, duration);
        AnimationClip clip = (AnimationClip)controller.layers[0].stateMachine.defaultState.motion;
        clip.frameRate = 90f;
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        ObjectReferenceKeyframe[] frames = AnimationUtility.GetObjectReferenceCurve(clip, binding);
        Object lastSprite = frames[frames.Length - 1].value;
        Array.Resize(ref frames, frames.Length + 1);
        // Unity derives the native end from the last key + one Samples tick. Anchor the hold
        // with the same final sprite so 90 Samples cannot shorten its visible 1/12-second hold.
        frames[frames.Length - 1] = new ObjectReferenceKeyframe { time = duration - 1f / clip.frameRate, value = lastSprite };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        // The final sprite must hold for the same 1/12 second as every other sprite.
        settings.stopTime = duration;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return controller;
    }

    private static AnimatorController CreateAnimation(string name, string sheet, int frameCount, float duration)
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(SpriteFolder + sheet)
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name, StringComparer.Ordinal)
            .ToArray();
        string prefix = System.IO.Path.GetFileNameWithoutExtension(sheet);
        if (sprites.Length != frameCount || sprites.Where((sprite, i) => sprite.name != prefix + "_" + i.ToString("D2")).Any())
            throw new InvalidOperationException("Expected " + frameCount + " numerically named sprites in " + sheet);

        string clipPath = AnimationFolder + "/" + name + ".anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip { name = name };
            AssetDatabase.CreateAsset(clip, clipPath);
        }
        clip.frameRate = sprites.Length / duration;
        var frames = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            Sprite sprite = sprites[i];
            float time = i / clip.frameRate;
            frames[i] = new ObjectReferenceKeyframe { time = time, value = sprite };
        }
        AnimationUtility.SetObjectReferenceCurve(clip,
            EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), frames);
        // The new sheets have a common foot pivot; old cropped-sprite offsets would lift the slime.
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
        {
            if (binding.type == typeof(Transform))
                AnimationUtility.SetEditorCurve(clip, binding, null);
        }
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        // Unity holds the final sprite for one sample. A repeated first key would double its loop-boundary hold.
        settings.startTime = 0f;
        settings.stopTime = duration;
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);

        string controllerPath = AnimationFolder + "/" + name + ".controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            AnimatorState state = controller.layers[0].stateMachine.AddState(name);
            controller.layers[0].stateMachine.defaultState = state;
        }
        controller.layers[0].stateMachine.defaultState.motion = clip;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static void CreatePrefab(string name, bool saw, bool patrol, AnimatorController controller)
    {
        var root = new GameObject(name);
        try
        {
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.useFullKinematicContacts = true;
            Collider2D contact;
            if (saw)
            {
                var circle = root.AddComponent<CircleCollider2D>();
                circle.radius = 0.46f;
                contact = circle;
            }
            else
            {
                var box = root.AddComponent<BoxCollider2D>();
                box.size = new Vector2(0.8f, 0.64f);
                box.offset = new Vector2(0f, 0.32f);
                contact = box;
            }
            contact.isTrigger = true;
            root.AddComponent<EnemyContactKnockback>();

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.flipX = !saw;
            renderer.sortingLayerName = "Level";
            renderer.sortingOrder = 1;
            AnimationClip clip = (AnimationClip)controller.layers[0].stateMachine.defaultState.motion;
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            renderer.sprite = (Sprite)AnimationUtility.GetObjectReferenceCurve(clip, binding)[0].value;
            // Saw roots are centred on the blade; slime roots stay at the prepared foot pivot.
            visual.transform.localPosition = saw ? Vector3.down * 0.425f : Vector3.zero;
            visual.AddComponent<Animator>().runtimeAnimatorController = controller;

            if (patrol)
            {
                EnemyPatrol movement = root.AddComponent<EnemyPatrol>();
                var serialized = new SerializedObject(movement);
                serialized.FindProperty("bodyVisual").objectReferenceValue = renderer;
                serialized.FindProperty("faceMovement").boolValue = !saw;
                serialized.FindProperty("horizontalChaseOnly").boolValue = !saw;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            if (!saw)
            {
                var heat = new GameObject("HeatArea");
                heat.transform.SetParent(root.transform, false);
                CircleCollider2D area = heat.AddComponent<CircleCollider2D>();
                area.isTrigger = true;
                area.radius = 1.75f;
                area.offset = new Vector2(0f, 0.4f);
                heat.AddComponent<EnemyHeatArea>();
            }
            ConfigureBody(root, saw, !patrol);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + "/" + name + ".prefab");
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;
        int separator = path.LastIndexOf('/');
        string parent = path.Substring(0, separator);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, path.Substring(separator + 1));
    }

}
