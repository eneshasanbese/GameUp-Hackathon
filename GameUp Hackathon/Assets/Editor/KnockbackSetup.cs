using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class KnockbackSetup
{
    private const string HurtSpritePath = "Assets/platformerPack/Sprites/Player/Frames/death/player_death_01.png";

    [MenuItem("GameUp/Player/Configure Knockback")]
    public static void Configure()
    {
        ConfigureHurtSprite();
        Sprite hurt = AssetDatabase.LoadAllAssetsAtPath(HurtSpritePath).OfType<Sprite>().Single();
        const string clipPath = "Assets/Animations/Knockback.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip { name = "Knockback" };
            AssetDatabase.CreateAsset(clip, clipPath);
        }
        clip.frameRate = 10f;
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"),
            new[] { new ObjectReferenceKeyframe { time = 0f, value = hurt } });
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.startTime = 0f;
        settings.stopTime = 0.1f;
        // The last sprite stays visible during flash; a loop would repeat sound events.
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        ConfigureController("Assets/Animations/Visual.controller", clip);
        ConfigureController("Assets/Animations/Player.controller", clip);
        ConfigurePlayer();
        foreach (string name in new[] { "Saw_Stationary", "Saw_Patrol", "SlimeFire_Stationary", "SlimeFire_Patrol", "HapsirikKoz_Stationary", "HapsirikKoz_Patrol" })
        {
            string path = "Assets/Prefabs/Enemies/" + name + ".prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (root.GetComponent<EnemyContactKnockback>() == null)
                    root.AddComponent<EnemyContactKnockback>();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Configured player knockback/flash and body contact on six enemy prefabs, preserving existing prefab settings.");
    }

    private static void ConfigureHurtSprite()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(HurtSpritePath);
        var factories = new SpriteDataProviderFactories();
        factories.Init();
        ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        SpriteRect[] slices = provider.GetSpriteRects();
        if (slices.Length != 1)
            throw new InvalidOperationException("Expected one existing player_death_01 sprite slice.");
        // Match the normal player animation cells and foot pivot while retaining spriteID/fileID/GUID.
        slices[0].rect = new Rect(0f, 0f, 128f, 128f);
        slices[0].alignment = SpriteAlignment.Custom;
        slices[0].pivot = new Vector2(0.5f, 0.47265625f);
        provider.SetSpriteRects(slices);
        provider.Apply();
        importer.spritePixelsPerUnit = 128f;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static void ConfigureController(string path, AnimationClip clip)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null)
            throw new InvalidOperationException("Missing existing player controller: " + path);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState hurt = machine.states.Select(child => child.state).FirstOrDefault(state => state.name == "Knockback")
            ?? machine.AddState("Knockback", new Vector3(650f, 240f));
        hurt.motion = clip;
        hurt.writeDefaultValues = false;
        AnimatorStateTransition enter = machine.anyStateTransitions.FirstOrDefault(transition => transition.destinationState == hurt)
            ?? machine.AddAnyStateTransition(hurt);
        enter.conditions = new[] { new AnimatorCondition { mode = AnimatorConditionMode.Equals, parameter = "State", threshold = (int)PlayerStates.State.Knockback } };
        enter.hasExitTime = false;
        enter.duration = 0f;
        enter.canTransitionToSelf = false;
        foreach (PlayerStates.State state in new[] { PlayerStates.State.Idle, PlayerStates.State.Move, PlayerStates.State.Jump })
        {
            AnimatorState target = machine.states.Select(child => child.state).Single(candidate => candidate.name == state.ToString());
            AnimatorStateTransition exit = hurt.transitions.FirstOrDefault(transition => transition.destinationState == target)
                ?? hurt.AddTransition(target);
            exit.conditions = new[] { new AnimatorCondition { mode = AnimatorConditionMode.Equals, parameter = "State", threshold = (int)state } };
            exit.hasExitTime = false;
            exit.duration = 0f;
        }
        foreach (AnimatorStateTransition heal in machine.anyStateTransitions.Where(transition => transition.destinationState != null && transition.destinationState.name == "Heal"))
        {
            if (!heal.conditions.Any(condition => condition.parameter == "State" && condition.mode == AnimatorConditionMode.NotEqual && condition.threshold == (int)PlayerStates.State.Knockback))
                heal.AddCondition(AnimatorConditionMode.NotEqual, (int)PlayerStates.State.Knockback, "State");
        }
        EditorUtility.SetDirty(hurt);
        EditorUtility.SetDirty(machine);
        EditorUtility.SetDirty(controller);
    }

    private static void ConfigurePlayer()
    {
        const string path = "Assets/Prefabs/Player.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            KnockbackAbility knockback = root.GetComponent<KnockbackAbility>();
            if (knockback == null)
                knockback = root.AddComponent<KnockbackAbility>();
            var serialized = new SerializedObject(knockback);
            serialized.FindProperty("bodyVisual").objectReferenceValue = root.transform.Find("Visual").GetComponent<SpriteRenderer>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
