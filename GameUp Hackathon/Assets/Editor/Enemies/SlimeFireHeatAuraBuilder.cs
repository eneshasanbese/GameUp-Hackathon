using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class SlimeFireHeatAuraBuilder
{
    public const string SheetPath = "Assets/platformerPack/Sprites/Enemies/FireSlime/HeatAura/256/Heat_Sheet.png";
    public const string ClipPath = "Assets/Animations/Enemies/SlimeFireHeatAura.anim";
    public const string ControllerPath = "Assets/Animations/Enemies/SlimeFireHeatAura.controller";

    [MenuItem("GameUp/Enemies/Rebuild Slime Fire Heat Aura")]
    public static void Rebuild()
    {
        RebuildAnimation();
        foreach (string variant in new[] { "Stationary", "Patrol" })
        {
            string path = "Assets/Prefabs/Enemies/SlimeFire_" + variant + ".prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Configure(root.transform.Find("HeatArea").gameObject);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
    }

    public static void RebuildAnimation()
    {
        ConfigureSheet();
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(SheetPath).OfType<Sprite>()
            .OrderBy(sprite => sprite.name, StringComparer.Ordinal).ToArray();
        if (sprites.Length != 24 || sprites.Where((sprite, i) => sprite.name != "Heat_" + i.ToString("D2")).Any())
            throw new InvalidOperationException("Heat aura requires 24 ordered Heat_00 through Heat_23 sprites.");

        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
        if (clip == null)
        {
            clip = new AnimationClip { name = "SlimeFireHeatAura" };
            AssetDatabase.CreateAsset(clip, ClipPath);
        }
        clip.frameRate = 12f;
        var frames = new ObjectReferenceKeyframe[24];
        for (int i = 0; i < frames.Length; i++)
            frames[i] = new ObjectReferenceKeyframe { time = i / 12f, value = sprites[i] };
        AnimationUtility.SetObjectReferenceCurve(clip,
            EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), frames);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.startTime = 0f;
        settings.stopTime = 2f;
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            AnimatorState state = controller.layers[0].stateMachine.AddState("HeatAura");
            controller.layers[0].stateMachine.defaultState = state;
        }
        controller.layers[0].stateMachine.defaultState.motion = clip;
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    public static void Configure(GameObject heat)
    {
        Transform existing = heat.transform.Find("HeatAura");
        GameObject aura = existing != null ? existing.gameObject : new GameObject("HeatAura");
        aura.transform.SetParent(heat.transform, false);
        var renderer = aura.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = aura.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAllAssetsAtPath(SheetPath).OfType<Sprite>()
            .Single(sprite => sprite.name == "Heat_00");
        renderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        renderer.sortingLayerName = "Level";
        renderer.sortingOrder = 0;
        renderer.color = Color.white;
        var animator = aura.GetComponent<Animator>();
        if (animator == null) animator = aura.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (aura.GetComponent<EnemyHeatAuraVisual>() == null)
            aura.AddComponent<EnemyHeatAuraVisual>();
    }

    private static void ConfigureSheet()
    {
        var importer = AssetImporter.GetAtPath(SheetPath) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing HeatAura 256 sheet.");
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 256f;
        var textureSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(textureSettings);
        importer.filterMode = FilterMode.Bilinear;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        foreach (string platform in new[] { "Standalone", "Android", "iPhone", "WebGL" })
        {
            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings(platform);
            if (!platformSettings.overridden) continue;
            platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
            platformSettings.format = TextureImporterFormat.RGBA32;
            importer.SetPlatformTextureSettings(platformSettings);
        }

        var factories = new SpriteDataProviderFactories();
        factories.Init();
        ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        SpriteRect[] previous = provider.GetSpriteRects();
        var rects = new SpriteRect[24];
        for (int i = 0; i < rects.Length; i++)
        {
            var rect = new Rect(i % 6 * 256, (3 - i / 6) * 256, 256, 256);
            // Preserve the imported sprite IDs when replacing automatic trimmed slices with a fixed grid.
            SpriteRect old = previous.FirstOrDefault(sprite => rect.Contains(sprite.rect.center));
            rects[i] = new SpriteRect
            {
                name = "Heat_" + i.ToString("D2"), rect = rect,
                alignment = SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f),
                spriteID = old != null ? old.spriteID : GUID.Generate()
            };
        }
        provider.SetSpriteRects(rects);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(
            rects.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
        provider.Apply();
        importer.SaveAndReimport();
    }
}
