using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class DeathRespawnSetup
{
    [MenuItem("GameUp/Player/Configure Death and Respawn")]
    public static void Configure()
    {
        var frames = new Sprite[8];
        for (int i = 0; i < frames.Length; i++)
        {
            string path = $"Assets/platformerPack/Sprites/Player/Frames/death/player_death_{i:00}.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var factories = new SpriteDataProviderFactories();
            factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            SpriteRect[] slices = provider.GetSpriteRects();
            string frameName = $"player_death_{i:00}_0";
            SpriteRect frame = slices.SingleOrDefault(slice => slice.name == frameName);
            if (frame == null)
                throw new InvalidOperationException("Missing existing death sprite: " + frameName);
            // Preserve the existing sprite IDs while matching Idle/Move cell size and foot pivot.
            frame.rect = new Rect(0f, 0f, 128f, 128f);
            frame.alignment = SpriteAlignment.Custom;
            frame.pivot = new Vector2(0.5f, 0.47265625f);
            provider.SetSpriteRects(slices);
            provider.Apply();
            importer.spritePixelsPerUnit = 128f;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            frames[i] = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single(sprite => sprite.name == frameName);
        }

        const string clipPath = "Assets/Animations/Death.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip { name = "Death" };
            AssetDatabase.CreateAsset(clip, clipPath);
        }
        clip.frameRate = 10f;
        var keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / clip.frameRate, value = frames[i] };
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.startTime = 0f;
        settings.stopTime = frames.Length / clip.frameRate;
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);

        foreach (string path in new[] { "Assets/Animations/Visual.controller", "Assets/Animations/Player.controller" })
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState death = machine.states.Select(child => child.state).FirstOrDefault(state => state.name == "Death")
                ?? machine.AddState("Death", new Vector3(650f, 360f));
            death.motion = clip;
            death.writeDefaultValues = false;
            AnimatorStateTransition enter = machine.anyStateTransitions.FirstOrDefault(transition => transition.destinationState == death)
                ?? machine.AddAnyStateTransition(death);
            enter.conditions = new[] { new AnimatorCondition { mode = AnimatorConditionMode.Equals, parameter = "State", threshold = (int)PlayerStates.State.Death } };
            enter.hasExitTime = false;
            enter.duration = 0f;
            enter.canTransitionToSelf = false;
            foreach (AnimatorStateTransition heal in machine.anyStateTransitions.Where(transition => transition.destinationState != null && transition.destinationState.name == "Heal"))
            {
                if (!heal.conditions.Any(condition => condition.parameter == "State" && condition.mode == AnimatorConditionMode.NotEqual && condition.threshold == (int)PlayerStates.State.Death))
                    heal.AddCondition(AnimatorConditionMode.NotEqual, (int)PlayerStates.State.Death, "State");
            }
            EditorUtility.SetDirty(controller);
        }

        const string prefabPath = "Assets/Prefabs/Player.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            if (root.GetComponent<DeathAbility>() == null)
                root.AddComponent<DeathAbility>();
            var serialized = new SerializedObject(root.GetComponent<Player>());
            SerializedProperty abilities = serialized.FindProperty("stateMachine.arrayOfAbilities");
            BaseAbility[] components = root.GetComponents<BaseAbility>();
            abilities.arraySize = components.Length;
            for (int i = 0; i < components.Length; i++)
                abilities.GetArrayElementAtIndex(i).objectReferenceValue = components[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
    }
}
