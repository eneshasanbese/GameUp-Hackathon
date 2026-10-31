using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class EmberSuctionWindBuilder
{
    public const string SheetFolder = "Assets/platformerPack/Sprites/Enemies/KozOcagi/SuctionWind/Unity_256/";
    public const string AnimationFolder = "Assets/Animations/Enemies/";
    public const string ControllerPath = AnimationFolder + "KozOcagiSuctionWind.controller";

    [MenuItem("GameUp/Enemies/Rebuild Koz Ocagi Suction Wind")]
    public static void Rebuild()
    {
        RebuildAnimations();
        GameObject root = PrefabUtility.LoadPrefabContents(EmberSuctionPrefabBuilder.PrefabPath);
        try
        {
            Configure(root);
            PrefabUtility.SaveAsPrefabAsset(root, EmberSuctionPrefabBuilder.PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
    }

    public static void RebuildAnimations()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        foreach (var definition in new[] { (name: "IdleWind", fps: 12f), (name: "PullWind", fps: 16f), (name: "PullWindStrong", fps: 24f) })
        {
            string sheet = SheetFolder + definition.name + ".png";
            var importer = AssetImporter.GetAtPath(sheet) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing suction wind sheet: " + sheet);
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(textureSettings);
            foreach (string platform in new[] { "Standalone", "Android", "iPhone", "WebGL" })
            {
                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
                if (!settings.overridden) continue;
                settings.textureCompression = TextureImporterCompression.Uncompressed;
                settings.format = TextureImporterFormat.RGBA32;
                importer.SetPlatformTextureSettings(settings);
            }
            importer.SaveAndReimport();
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(sheet).OfType<Sprite>().OrderBy(sprite => sprite.name, StringComparer.Ordinal).ToArray();
            if (sprites.Length != 24 || sprites.Where((sprite, i) => sprite.name != definition.name + "_" + i.ToString("D2") ||
                sprite.rect.size != new Vector2(256f, 256f) || sprite.pixelsPerUnit != 256f ||
                Vector2.Distance(sprite.pivot / 256f, new Vector2(0.14f, 0.5f)) > 0.001f).Any())
                throw new InvalidOperationException("Expected 24 ordered 256px wind frames with the shared mouth pivot: " + sheet);

            string clipPath = AnimationFolder + "KozOcagi" + definition.name + ".anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip { name = "KozOcagi" + definition.name };
                AssetDatabase.CreateAsset(clip, clipPath);
            }
            clip.frameRate = definition.fps;
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"),
                sprites.Select((sprite, i) => new ObjectReferenceKeyframe { time = i / definition.fps, value = sprite }).ToArray());
            AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
            clipSettings.startTime = 0f;
            clipSettings.stopTime = sprites.Length / definition.fps;
            clipSettings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
            EditorUtility.SetDirty(clip);
            AnimatorState state = machine.states.Select(child => child.state).FirstOrDefault(existing => existing.name == definition.name);
            if (state == null) state = machine.AddState(definition.name);
            state.motion = clip;
            state.writeDefaultValues = false;
            if (definition.name == "IdleWind") machine.defaultState = state;
        }
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    public static void Configure(GameObject root)
    {
        Transform existing = root.transform.Find("SuctionWind");
        GameObject wind = existing != null ? existing.gameObject : new GameObject("SuctionWind");
        wind.transform.SetParent(root.transform, false);
        wind.transform.position = root.transform.Find("PullPoint").position;
        wind.transform.localScale = Vector3.one * 1.5f;
        SpriteRenderer renderer = wind.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = wind.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAllAssetsAtPath(SheetFolder + "IdleWind.png").OfType<Sprite>().Single(sprite => sprite.name == "IdleWind_00");
        SpriteRenderer furnace = root.transform.Find("Visual").GetComponent<SpriteRenderer>();
        renderer.sharedMaterial = furnace.sharedMaterial;
        renderer.sortingLayerID = furnace.sortingLayerID;
        renderer.sortingOrder = furnace.sortingOrder - 1;
        renderer.color = new Color(1f, 1f, 1f, 0.35f);
        Animator animator = wind.GetComponent<Animator>();
        if (animator == null) animator = wind.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        EmberSuctionWindVisual visual = wind.GetComponent<EmberSuctionWindVisual>();
        if (visual == null) visual = wind.AddComponent<EmberSuctionWindVisual>();
        var settings = new SerializedObject(visual);
        settings.FindProperty("suction").objectReferenceValue = root.GetComponentInChildren<EmberSuctionEnemy>();
        settings.ApplyModifiedPropertiesWithoutUndo();
    }
}
