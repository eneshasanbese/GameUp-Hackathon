using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class EmberSuctionPrefabBuilder
{
    public const string PrefabPath = "Assets/Prefabs/Enemies/KozOcagi_Stationary.prefab";
    public const string DemoPath = "Assets/Scenes/KozOcagiDemo.unity";
    private const string SpritePath = "Assets/platformerPack/Sprites/Enemies/KozOcagi/Unity_128/Idle.png";

    [MenuItem("GameUp/Enemies/Rebuild Koz Ocagi Prefab")]
    public static void RebuildPrefab()
    {
        Sprite idle = AssetDatabase.LoadAllAssetsAtPath(SpritePath).OfType<Sprite>().Single(sprite => sprite.name == "Idle_00");
        var root = new GameObject("KozOcagi_Stationary");
        try
        {
            Transform visual = Child(root.transform, "Visual", Vector2.zero);
            // Sheet PPU is 128: visible width ~0.83 units, comparable to the 1-unit player.
            visual.localScale = Vector3.one * 1.5f;
            SpriteRenderer renderer = visual.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = idle;
            renderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            renderer.sortingLayerName = "Level";
            renderer.sortingOrder = 1;
            Transform origin = Child(root.transform, "RangeOrigin", new Vector2(0f, 0.5f));
            Transform mouth = Child(root.transform, "PullPoint", new Vector2(0f, 0.38f));
            Transform effect = Child(root.transform, "EffectArea", new Vector2(0f, 0.5f));
            CircleCollider2D area = effect.gameObject.AddComponent<CircleCollider2D>();
            area.isTrigger = true;
            area.radius = 3f;
            EmberSuctionEnemy enemy = effect.gameObject.AddComponent<EmberSuctionEnemy>();
            var settings = new SerializedObject(enemy);
            settings.FindProperty("rangeOrigin").objectReferenceValue = origin;
            settings.FindProperty("pullPoint").objectReferenceValue = mouth;
            settings.FindProperty("playerLayerMask").intValue = LayerMask.GetMask("Player");
            settings.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            AssetDatabase.SaveAssets();
        }
        finally { Object.DestroyImmediate(root); }
        Debug.Log("Created KozOcagi_Stationary: single idle sprite, horizontal suction and independent water-point loss. Existing prefabs unchanged.");
    }

    [MenuItem("GameUp/Enemies/Rebuild Koz Ocagi Demo")]
    public static void RebuildDemo()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;
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
            camera.transform.position = new Vector3(0f, 2f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.09f, 0.13f);
            cameraRoot.AddComponent<AudioListener>();

            var ground = new GameObject("Dry Ground");
            ground.layer = LayerMask.NameToLayer("Ground-Dry");
            ground.transform.position = new Vector2(0f, -0.5f);
            ground.AddComponent<BoxCollider2D>().size = new Vector2(32f, 1f);
            SpriteRenderer surface = ground.AddComponent<SpriteRenderer>();
            surface.sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/platformerPack/Sprites/Tiles/Double/block_planks.png").OfType<Sprite>().First();
            surface.drawMode = SpriteDrawMode.Tiled;
            surface.size = new Vector2(32f, 1f);
            surface.sortingLayerName = "Level";
            surface.color = new Color(0.48f, 0.42f, 0.38f);
            surface.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab"), scene);
            player.transform.position = new Vector2(-6f, 0.51f);
            // This isolated demo has no 2D lights; keep its Player visible without changing the Player prefab or render pipeline.
            player.transform.Find("Visual").GetComponent<SpriteRenderer>().sharedMaterial = surface.sharedMaterial;
            var physicsSettings = new SerializedObject(player.GetComponent<PhysicsControl>());
            physicsSettings.FindProperty("extraDryGroundLayer").intValue = LayerMask.GetMask("Ground-Extra-Dry");
            physicsSettings.ApplyModifiedPropertiesWithoutUndo();
            GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene);
            enemy.transform.position = new Vector2(1f, 0f);
            GameObject canvas = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Canvas.prefab"), scene);
            var hudSettings = new SerializedObject(canvas.GetComponentInChildren<WaterBarUI>());
            hudSettings.FindProperty("waterMeter").objectReferenceValue = player.GetComponent<WaterMeter>();
            hudSettings.ApplyModifiedPropertiesWithoutUndo();
            EmberSuctionDemoControls controls = new GameObject("Demo Controls").AddComponent<EmberSuctionDemoControls>();
            var controlSettings = new SerializedObject(controls);
            controlSettings.FindProperty("waterMeter").objectReferenceValue = player.GetComponent<WaterMeter>();
            controlSettings.ApplyModifiedPropertiesWithoutUndo();
            if (!EditorSceneManager.SaveScene(scene, DemoPath))
                throw new InvalidOperationException("Could not save " + DemoPath);
        }
        finally
        {
            if (additive && previous.IsValid() && previous.isLoaded)
            {
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }
        Debug.Log("Created " + DemoPath + " with a safe start, flat ground, Player, furnace, existing water HUD and R restart. Build scene list unchanged.");
    }

    // For an isolated batch editor. Does not run automatically in the main editor.
    public static void RebuildAll()
    {
        RebuildPrefab();
        RebuildDemo();
    }

    private static Transform Child(Transform parent, string name, Vector2 position)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = position;
        return child.transform;
    }
}
