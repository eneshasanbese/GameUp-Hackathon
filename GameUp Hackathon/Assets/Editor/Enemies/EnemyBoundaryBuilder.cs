using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class EnemyBoundaryBuilder
{
    public const string BoundaryLayer = "EnemyBoundary";
    public const string GroundEnemyLayer = "GroundEnemy";
    public const string PrefabPath = "Assets/Prefabs/Enemies/GroundEnemyBoundary.prefab";

    public static void EnsureLayers()
    {
        if (LayerMask.NameToLayer(BoundaryLayer) >= 0 && LayerMask.NameToLayer(GroundEnemyLayer) >= 0)
            return;
        var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = settings.FindProperty("layers");
        foreach (string name in new[] { BoundaryLayer, GroundEnemyLayer })
        {
            bool found = false;
            for (int i = 8; i < layers.arraySize; i++)
                if (layers.GetArrayElementAtIndex(i).stringValue == name) found = true;
            if (found) continue;
            int empty = -1;
            for (int i = 8; i < layers.arraySize; i++)
                if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue)) { empty = i; break; }
            if (empty < 0) throw new InvalidOperationException("No free user layer for " + name);
            layers.GetArrayElementAtIndex(empty).stringValue = name;
        }
        settings.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("GameUp/Enemies/Configure Ground Enemy Boundary")]
    public static void Configure()
    {
        EnsureLayers();
        bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null;
        GameObject root = exists ? PrefabUtility.LoadPrefabContents(PrefabPath) : new GameObject("GroundEnemyBoundary");
        try
        {
            root.layer = LayerMask.NameToLayer(BoundaryLayer);
            EdgeCollider2D edge = root.GetComponent<EdgeCollider2D>();
            if (edge == null) edge = root.AddComponent<EdgeCollider2D>();
            if (!exists) edge.points = new[] { new Vector2(0f, -1f), new Vector2(0f, 2f) };
            edge.isTrigger = false;
            edge.edgeRadius = 0.02f;
            int groundEnemy = LayerMask.GetMask(GroundEnemyLayer);
            edge.includeLayers = groundEnemy;
            edge.excludeLayers = ~groundEnemy;
            edge.layerOverridePriority = 2;
            edge.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/platformerPack/Material/Slip.physicsMaterial2D");
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            if (exists) PrefabUtility.UnloadPrefabContents(root);
            else Object.DestroyImmediate(root);
        }
        EnemyPrefabBuilder.ConfigureTerrainPhysics();
    }

    [MenuItem("GameUp/Enemies/Create Ground Enemy Boundary")]
    public static void CreateInScene()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Configure();
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }
        var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(root, "Create ground enemy boundary");
        SceneView view = SceneView.lastActiveSceneView;
        if (view != null) root.transform.position = new Vector3(view.pivot.x, view.pivot.y, 0f);
        Selection.activeGameObject = root;
    }
}
