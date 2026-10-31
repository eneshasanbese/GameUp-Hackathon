using System;
using UnityEditor;
using UnityEngine;

public static class EmberSuctionHeatAuraBuilder
{
    public const string SlimePath = "Assets/Prefabs/Enemies/SlimeFire_Patrol.prefab";

    [MenuItem("GameUp/Enemies/Rebuild Koz Ocagi Heat Aura")]
    public static void Rebuild()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(EmberSuctionPrefabBuilder.PrefabPath);
        try
        {
            Configure(root);
            PrefabUtility.SaveAsPrefabAsset(root, EmberSuctionPrefabBuilder.PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
    }

    public static void Configure(GameObject root)
    {
        GameObject slime = AssetDatabase.LoadAssetAtPath<GameObject>(SlimePath);
        Transform template = slime != null ? slime.transform.Find("HeatArea/HeatAura") : null;
        if (template == null) throw new InvalidOperationException("Missing SlimeFire heat aura template.");
        EmberSuctionEnemy source = root.GetComponentInChildren<EmberSuctionEnemy>();
        Transform area = source.transform;
        Transform existing = area.Find("HeatAura");
        GameObject aura = existing != null ? existing.gameObject : new GameObject("HeatAura");
        aura.transform.SetParent(area, false);
        SpriteRenderer renderer = aura.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = aura.AddComponent<SpriteRenderer>();
        SpriteRenderer templateRenderer = template.GetComponent<SpriteRenderer>();
        SpriteRenderer furnace = root.transform.Find("Visual").GetComponent<SpriteRenderer>();
        renderer.sprite = templateRenderer.sprite;
        renderer.sharedMaterial = templateRenderer.sharedMaterial;
        Color color = templateRenderer.color;
        // The furnace covers a much wider area; keep the wind and terrain readable beneath it.
        color.a *= 0.35f;
        renderer.color = color;
        renderer.sortingLayerID = furnace.sortingLayerID;
        // Keep both the furnace and the directional wind visible above the continuous heat.
        renderer.sortingOrder = furnace.sortingOrder - 2;
        Animator animator = aura.GetComponent<Animator>();
        if (animator == null) animator = aura.AddComponent<Animator>();
        animator.runtimeAnimatorController = template.GetComponent<Animator>().runtimeAnimatorController;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        EnemyHeatAuraVisual visual = aura.GetComponent<EnemyHeatAuraVisual>();
        if (visual == null) visual = aura.AddComponent<EnemyHeatAuraVisual>();
        var settings = new SerializedObject(visual);
        settings.FindProperty("heatCollider").objectReferenceValue = source.GetComponent<CircleCollider2D>();
        settings.FindProperty("heatArea").objectReferenceValue = null;
        settings.FindProperty("suctionSource").objectReferenceValue = source;
        settings.FindProperty("referenceSpriteDiameter").floatValue =
            new SerializedObject(template.GetComponent<EnemyHeatAuraVisual>()).FindProperty("referenceSpriteDiameter").floatValue;
        settings.ApplyModifiedPropertiesWithoutUndo();
    }
}
