using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using Object = UnityEngine.Object;

public static class EnemyPrefabBuilder
{
    private const string PrefabFolder = "Assets/Prefabs/Enemies";
    private const string AnimationFolder = "Assets/Animations/Enemies";
    private const string SpriteFolder = "Assets/platformerPack/Sprites/Enemies/";

    [MenuItem("GameUp/Enemies/Rebuild Enemy Prefabs")]
    public static void Rebuild()
    {
        EnsureFolder(PrefabFolder);
        EnsureFolder(AnimationFolder);
        RepairSawSlices("idle");
        RepairSawSlices("walk");
        // The walk sheet rotates the blade while keeping the face upright, even when stationary.
        AnimatorController saw = CreateAnimation("SawSpin", "saw", "walk", false, 24f);
        AnimatorController idle = CreateAnimation("SlimeFireIdle", "slime_fire", "idle", true);
        AnimatorController walk = CreateAnimation("SlimeFireWalk", "slime_fire", "walk", true);
        CreatePrefab("Saw_Stationary", true, false, saw);
        CreatePrefab("Saw_Patrol", true, true, saw);
        CreatePrefab("SlimeFire_Stationary", false, false, idle);
        CreatePrefab("SlimeFire_Patrol", false, true, walk);
        HapsirikKozPrefabBuilder.Rebuild();
        AssetDatabase.SaveAssets();
        Debug.Log("Created six enemy prefabs and timed HapsirikKoz smoke. Direct contact/knockback remains TODO.");
    }

    [MenuItem("GameUp/Enemies/Rebuild Saw Animation")]
    public static void RebuildSawAnimation()
    {
        EnsureFolder(AnimationFolder);
        CreateAnimation("SawSpin", "saw", "walk", false, 24f);
        AssetDatabase.SaveAssets();
    }

    private static AnimatorController CreateAnimation(string name, string enemy, string action, bool footAnchored, float frameRate = 10f)
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(SpriteFolder + enemy + "/Sheets/" + enemy + "_" + action + ".png")
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.rect.x)
            .ToArray();
        if (sprites.Length == 0)
            throw new InvalidOperationException("No sliced sprites found for " + enemy + "/" + action);

        string clipPath = AnimationFolder + "/" + name + ".anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip { name = name };
            AssetDatabase.CreateAsset(clip, clipPath);
        }
        clip.frameRate = frameRate;
        var frames = new ObjectReferenceKeyframe[sprites.Length];
        var heights = new Keyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            Sprite sprite = sprites[i];
            float time = i / clip.frameRate;
            frames[i] = new ObjectReferenceKeyframe { time = time, value = sprite };
            heights[i] = new Keyframe(time, -sprite.bounds.min.y, float.PositiveInfinity, float.PositiveInfinity);
        }
        AnimationUtility.SetObjectReferenceCurve(clip,
            EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), frames);
        if (footAnchored)
        {
            AnimationUtility.SetEditorCurve(clip,
                EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y"), new AnimationCurve(heights));
        }
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        // Unity holds the final sprite for one sample. A repeated first key would double its loop-boundary hold.
        settings.startTime = 0f;
        settings.stopTime = sprites.Length / clip.frameRate;
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
            // Reserved for a later knockback/contact response. Do not block the player now.
            contact.isTrigger = true;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.flipX = !saw;
            renderer.sortingLayerName = "Level";
            renderer.sortingOrder = 1;
            AnimationClip clip = (AnimationClip)controller.layers[0].stateMachine.defaultState.motion;
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            renderer.sprite = (Sprite)AnimationUtility.GetObjectReferenceCurve(clip, binding)[0].value;
            if (!saw)
                visual.transform.localPosition = Vector3.up * -renderer.sprite.bounds.min.y;
            visual.AddComponent<Animator>().runtimeAnimatorController = controller;

            if (patrol)
            {
                EnemyPatrol movement = root.AddComponent<EnemyPatrol>();
                var serialized = new SerializedObject(movement);
                serialized.FindProperty("bodyVisual").objectReferenceValue = renderer;
                serialized.FindProperty("faceMovement").boolValue = !saw;
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

    private static void RepairSawSlices(string action)
    {
        string path = SpriteFolder + "saw/Sheets/saw_" + action + ".png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        var factories = new SpriteDataProviderFactories();
        factories.Init();
        ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        List<SpriteRect> rects = provider.GetSpriteRects().ToList();
        // Automatic slicing merged the first two touching saw frames. Repair only that known layout.
        if (rects.Count != 7 || !rects.Any(rect => rect.rect.width > 200f))
            return;
        foreach (SpriteRect sprite in rects)
        {
            int column = Mathf.RoundToInt(sprite.rect.x / 128f);
            sprite.rect = new Rect(column * 128f, 0f, 128f, 128f);
        }
        rects.Add(new SpriteRect
        {
            name = "saw_" + action + "_01",
            rect = new Rect(128f, 0f, 128f, 128f),
            alignment = SpriteAlignment.Center,
            pivot = new Vector2(0.5f, 0.5f),
            spriteID = GUID.Generate()
        });
        provider.SetSpriteRects(rects.OrderBy(sprite => sprite.rect.x).ToArray());
        var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        names.SetNameFileIdPairs(rects.Select(sprite => new SpriteNameFileIdPair(sprite.name, sprite.spriteID)));
        provider.Apply();
        importer.SaveAndReimport();
    }
}
