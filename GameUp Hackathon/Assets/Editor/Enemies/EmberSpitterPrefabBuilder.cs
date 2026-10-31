using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class EmberSpitterPrefabBuilder
{
    public const string EnemyPath = "Assets/Prefabs/Enemies/EmberSpitter.prefab";
    public const string ProjectilePath = "Assets/Prefabs/Enemies/EmberFireball.prefab";
    public const string DemoPath = "Assets/Scenes/EmberSpitterDemo.unity";
    public const string IdlePath = "Assets/platformerPack/Sprites/Enemies/EmberPack/EmberSpitter/Unity_128/Idle.png";
    public const string FlyPath = "Assets/platformerPack/Sprites/Enemies/EmberPack/EmberFireball/Unity_128/Frames/Fly/Fly_00.png";
    public const string IdleClipPath = "Assets/Animations/Enemies/EmberSpitterIdle.anim";
    public const string IdleControllerPath = "Assets/Animations/Enemies/EmberSpitterIdle.controller";

    [MenuItem("GameUp/Enemies/Rebuild Ember Spitter Prefabs")]
    public static void RebuildPrefabs()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Enemies")) AssetDatabase.CreateFolder("Assets/Prefabs", "Enemies");
        Sprite idle = ImportSprites(IdlePath).First();
        Sprite fly = ImportSprite(FlyPath);
        int players = LayerMask.GetMask("Player");
        int obstacles = LayerMask.GetMask("Ground-Dry", "Ground-Wet", "Ground-Extra-Dry");

        var root = new GameObject("EmberFireball");
        try
        {
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            CircleCollider2D core = root.AddComponent<CircleCollider2D>();
            core.isTrigger = true;
            core.radius = 0.13f;
            Transform pivot = Child(root.transform, "Visual", Vector2.zero);
            Transform image = Child(pivot, "Sprite", new Vector2(-17.5f, 1.5f) * (0.65f / 128f));
            image.localScale = Vector3.one * 0.65f;
            Renderer(image, fly);
            EmberFireball projectile = root.AddComponent<EmberFireball>();
            var settings = new SerializedObject(projectile);
            settings.FindProperty("visual").objectReferenceValue = pivot;
            settings.FindProperty("playerLayerMask").intValue = players;
            settings.FindProperty("obstacleLayerMask").intValue = obstacles;
            settings.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, ProjectilePath);
        }
        finally { Object.DestroyImmediate(root); }

        root = new GameObject("EmberSpitter");
        try
        {
            root.layer = LayerMask.NameToLayer("GroundEnemy");
            Transform visual = Child(root.transform, "Visual", new Vector2(0f, 46f * 1.75f / 128f));
            visual.localScale = Vector3.one * 1.75f;
            SpriteRenderer renderer = Renderer(visual, idle);
            Transform origin = Child(root.transform, "RangeOrigin", new Vector2(0f, 0.85f));
            Transform right = Child(root.transform, "FirePointRight", new Vector2(0.64f, 0.95f));
            Transform left = Child(root.transform, "FirePointLeft", new Vector2(-0.64f, 0.95f));
            EmberSpitter enemy = root.AddComponent<EmberSpitter>();
            var settings = new SerializedObject(enemy);
            settings.FindProperty("visual").objectReferenceValue = renderer;
            settings.FindProperty("rangeOrigin").objectReferenceValue = origin;
            settings.FindProperty("firePointRight").objectReferenceValue = right;
            settings.FindProperty("firePointLeft").objectReferenceValue = left;
            settings.FindProperty("projectilePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<EmberFireball>(ProjectilePath);
            settings.FindProperty("playerLayerMask").intValue = players;
            settings.FindProperty("obstacleLayerMask").intValue = obstacles;
            settings.ApplyModifiedPropertiesWithoutUndo();
            ConfigureIdleAndGravity(root);
            PrefabUtility.SaveAsPrefabAsset(root, EnemyPath);
        }
        finally { Object.DestroyImmediate(root); }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("GameUp/Enemies/Configure Ember Spitter Idle and Gravity")]
    public static void ConfigureIdleAndGravity()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(EnemyPath);
        try
        {
            ConfigureIdleAndGravity(root);
            PrefabUtility.SaveAsPrefabAsset(root, EnemyPath);
            AssetDatabase.SaveAssets();
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void ConfigureIdleAndGravity(GameObject root)
    {
        Sprite[] frames = ImportSprites(IdlePath);
        if (frames.Length != 12 || frames.Where((sprite, i) => sprite.name != "Idle_" + i.ToString("D2")).Any())
            throw new InvalidOperationException("Expected twelve ordered EmberSpitter Idle sheet sprites.");
        if (!AssetDatabase.IsValidFolder("Assets/Animations/Enemies"))
            AssetDatabase.CreateFolder("Assets/Animations", "Enemies");
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(IdleClipPath);
        if (clip == null)
        {
            clip = new AnimationClip { name = "EmberSpitterIdle" };
            AssetDatabase.CreateAsset(clip, IdleClipPath);
        }
        clip.frameRate = 12f;
        var keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / clip.frameRate, value = frames[i] };
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
        AnimationClipSettings loop = AnimationUtility.GetAnimationClipSettings(clip);
        loop.startTime = 0f;
        loop.stopTime = 1f;
        loop.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, loop);
        EditorUtility.SetDirty(clip);
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(IdleControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(IdleControllerPath);
            AnimatorState state = controller.layers[0].stateMachine.AddState("Idle");
            controller.layers[0].stateMachine.defaultState = state;
        }
        controller.layers[0].stateMachine.defaultState.motion = clip;
        EditorUtility.SetDirty(controller);
        Transform visual = root.transform.Find("Visual");
        visual.GetComponent<SpriteRenderer>().sprite = frames[0];
        // Full 128px cells use a common foot pivot. The visible feet end at y=17px in every frame.
        // Account for the 20.48px pivot without animating position, scale or the physics body.
        visual.localPosition = Vector3.up * ((frames[0].pivot.y - 17f) / frames[0].pixelsPerUnit * visual.localScale.y);
        Animator animator = visual.GetComponent<Animator>();
        if (animator == null) animator = visual.gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        Rigidbody2D body = root.GetComponent<Rigidbody2D>();
        if (body == null) body = root.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 1f;
        body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        Transform terrain = root.transform.Find("TerrainBody");
        if (terrain == null) terrain = Child(root.transform, "TerrainBody", Vector2.zero);
        terrain.gameObject.layer = LayerMask.NameToLayer("GroundEnemy");
        BoxCollider2D solid = terrain.GetComponent<BoxCollider2D>();
        if (solid == null) solid = terrain.gameObject.AddComponent<BoxCollider2D>();
        solid.size = new Vector2(0.9f, 1.2f);
        solid.offset = new Vector2(0f, 0.6f);
        solid.isTrigger = false;
        int terrainMask = LayerMask.GetMask("Ground-Dry", "Ground-Wet", "Ground-Extra-Dry", "EnemyBoundary");
        solid.includeLayers = terrainMask;
        solid.excludeLayers = ~terrainMask;
        solid.layerOverridePriority = 1;
        solid.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/platformerPack/Material/Slip.physicsMaterial2D");
        if (solid.sharedMaterial == null) throw new InvalidOperationException("Missing Slip physics material.");
    }

    private static Sprite ImportSprite(string path) => ImportSprites(path).Single();

    private static Sprite[] ImportSprites(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        // Preserve the supplied cells, sprite IDs, fileIDs, pivots and GUIDs.
        importer.spritePixelsPerUnit = 128f;
        importer.filterMode = FilterMode.Bilinear;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        foreach (string platform in new[] { "Standalone", "Android", "iPhone" })
        {
            TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
            if (!settings.overridden) continue;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(settings);
        }
        importer.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(sprite => sprite.name, StringComparer.Ordinal).ToArray();
    }

    [MenuItem("GameUp/Enemies/Rebuild Ember Spitter Demo")]
    public static void RebuildDemo()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Scene previous = SceneManager.GetActiveScene();
        bool additive = !string.IsNullOrEmpty(previous.path);
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, additive ? NewSceneMode.Additive : NewSceneMode.Single);
        SceneManager.SetActiveScene(scene);
        try
        {
            var cameraRoot = new GameObject("Main Camera");
            cameraRoot.tag = "MainCamera";
            Camera camera = cameraRoot.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(0f, 2.8f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.09f, 0.13f);
            cameraRoot.AddComponent<AudioListener>();

            Block("Dry Ground", new Vector2(0f, -0.5f), new Vector2(24f, 1f), new Color(0.42f, 0.36f, 0.3f));
            Block("Sight Wall - jump over", new Vector2(-1.5f, 0.85f), new Vector2(0.25f, 1.7f), new Color(0.55f, 0.55f, 0.6f));
            Block("Thin Platform - swept hit", new Vector2(4.6f, 1.8f), new Vector2(2f, 0.05f), new Color(0.85f, 0.65f, 0.3f));
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab"), scene);
            player.transform.position = new Vector2(-6.7f, 0.51f);
            player.transform.Find("Visual").GetComponent<SpriteRenderer>().sharedMaterial = Material;
            var physics = new SerializedObject(player.GetComponent<PhysicsControl>());
            physics.FindProperty("extraDryGroundLayer").intValue = LayerMask.GetMask("Ground-Extra-Dry");
            physics.ApplyModifiedPropertiesWithoutUndo();
            GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPath), scene);
            enemy.transform.position = new Vector2(1.5f, 0f);
            GameObject hud = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Canvas.prefab"), scene);
            var ui = new SerializedObject(hud.GetComponentInChildren<WaterBarUI>());
            ui.FindProperty("waterMeter").objectReferenceValue = player.GetComponent<WaterMeter>();
            ui.ApplyModifiedPropertiesWithoutUndo();
            var controls = new SerializedObject(new GameObject("Demo Controls").AddComponent<EmberSpitterDemoControls>());
            controls.FindProperty("waterMeter").objectReferenceValue = player.GetComponent<WaterMeter>();
            controls.ApplyModifiedPropertiesWithoutUndo();
            if (!EditorSceneManager.SaveScene(scene, DemoPath)) throw new InvalidOperationException("Could not save " + DemoPath);
        }
        finally
        {
            if (additive && previous.IsValid() && previous.isLoaded)
            {
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }

    public static void RebuildAll() { RebuildPrefabs(); RebuildDemo(); }

    private static Material Material => AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

    private static SpriteRenderer Renderer(Transform target, Sprite sprite)
    {
        SpriteRenderer renderer = target.gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = Material;
        renderer.sortingLayerName = "Level";
        renderer.sortingOrder = 2;
        return renderer;
    }

    private static void Block(string name, Vector2 position, Vector2 size, Color color)
    {
        var root = new GameObject(name);
        root.layer = LayerMask.NameToLayer("Ground-Dry");
        root.transform.position = position;
        root.AddComponent<BoxCollider2D>().size = size;
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/platformerPack/Sprites/Tiles/Double/block_planks.png").OfType<Sprite>().First();
        SpriteRenderer renderer = Renderer(root.transform, sprite);
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = size;
        renderer.color = color;
        renderer.sortingOrder = 0;
    }

    private static Transform Child(Transform parent, string name, Vector2 position)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = position;
        return child.transform;
    }
}
