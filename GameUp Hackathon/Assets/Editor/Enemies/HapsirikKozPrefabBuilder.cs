using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

public static class HapsirikKozPrefabBuilder
{
    private const string AnimationFolder = "Assets/Animations/Enemies";
    private const string PrefabFolder = "Assets/Prefabs/Enemies";
    private const string SpriteFolder = "Assets/platformerPack/Sprites/Enemies/HapsirikKoz/";

    [MenuItem("GameUp/Enemies/Rebuild HapsirikKoz Prefabs")]
    public static void Rebuild()
    {
        EnsureFolder(AnimationFolder);
        EnsureFolder(PrefabFolder);
        AnimationClip idle = CreateClip("Idle", 8, 8f, true);
        AnimationClip sneeze = CreateClip("Sneeze", 12, 12f, false);
        AnimatorController controller = CreateController(idle, sneeze);
        EnemySmokeCloud smoke = CreateSmokePrefab();
        CreatePrefab(false, controller, smoke);
        CreatePrefab(true, controller, smoke);
        AssetDatabase.SaveAssets();
        Debug.Log("Created two HapsirikKoz prefabs and timed smoke. Contact/knockback remains TODO.");
    }

    private static AnimationClip CreateClip(string action, int frameCount, float frameRate, bool loop)
    {
        // These sheets span multiple rows; names encode the original left-to-right, top-to-bottom order.
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(SpriteFolder + action + ".png")
            .OfType<Sprite>().OrderBy(sprite => sprite.name, StringComparer.Ordinal).ToArray();
        if (sprites.Length != frameCount || sprites.Where((sprite, i) => sprite.name != action + "_" + i.ToString("D2")).Any())
            throw new InvalidOperationException("Expected " + frameCount + " numbered HapsirikKoz " + action + " sprites.");

        string name = "HapsirikKoz" + action;
        string path = AnimationFolder + "/" + name + ".anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip { name = name };
            AssetDatabase.CreateAsset(clip, path);
        }
        clip.frameRate = frameRate;
        var frames = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            frames[i] = new ObjectReferenceKeyframe { time = i / frameRate, value = sprites[i] };
        AnimationUtility.SetObjectReferenceCurve(clip,
            EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), frames);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.startTime = 0f;
        settings.stopTime = sprites.Length / frameRate;
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static AnimatorController CreateController(AnimationClip idleClip, AnimationClip sneezeClip)
    {
        string path = AnimationFolder + "/HapsirikKoz.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.parameters = new[] { new AnimatorControllerParameter { name = "Sneeze", type = AnimatorControllerParameterType.Trigger } };
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idle = machine.states.Select(child => child.state).FirstOrDefault(state => state.name == "Idle")
            ?? machine.AddState("Idle", new Vector3(250f, 50f, 0f));
        AnimatorState sneeze = machine.states.Select(child => child.state).FirstOrDefault(state => state.name == "Sneeze")
            ?? machine.AddState("Sneeze", new Vector3(500f, 50f, 0f));
        machine.defaultState = idle;
        idle.motion = idleClip;
        sneeze.motion = sneezeClip;
        foreach (AnimatorStateTransition transition in idle.transitions)
            idle.RemoveTransition(transition);
        foreach (AnimatorStateTransition transition in sneeze.transitions)
            sneeze.RemoveTransition(transition);

        AnimatorStateTransition start = idle.AddTransition(sneeze);
        start.hasExitTime = false;
        start.duration = 0f;
        start.AddCondition(AnimatorConditionMode.If, 0f, "Sneeze");
        AnimatorStateTransition finish = sneeze.AddTransition(idle);
        finish.hasExitTime = true;
        finish.exitTime = 1f;
        finish.duration = 0f;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static EnemySmokeCloud CreateSmokePrefab()
    {
        var root = new GameObject("HapsirikKoz_Smoke");
        try
        {
            // An independent body leaves the cloud at the emission point when the enemy moves on.
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.useFullKinematicContacts = true;
            CircleCollider2D area = root.AddComponent<CircleCollider2D>();
            area.isTrigger = true;
            area.radius = 1.75f;
            area.offset = new Vector2(0f, 0.4f);
            root.AddComponent<EnemyHeatArea>();
            root.AddComponent<EnemySmokeCloud>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + "/HapsirikKoz_Smoke.prefab");
            return prefab.GetComponent<EnemySmokeCloud>();
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    private static void CreatePrefab(bool patrol, AnimatorController controller, EnemySmokeCloud smoke)
    {
        string name = "HapsirikKoz_" + (patrol ? "Patrol" : "Stationary");
        var root = new GameObject(name);
        try
        {
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.useFullKinematicContacts = true;
            // Keep the shared foot pivot at the root; facial/squash frames must not animate physics.
            BoxCollider2D contact = root.AddComponent<BoxCollider2D>();
            contact.size = new Vector2(0.64f, 0.7f);
            contact.offset = new Vector2(0f, 0.35f);
            contact.isTrigger = true;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sortingLayerName = "Level";
            renderer.sortingOrder = 1;
            AnimationClip idle = (AnimationClip)controller.layers[0].stateMachine.defaultState.motion;
            renderer.sprite = (Sprite)AnimationUtility.GetObjectReferenceCurve(idle,
                EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"))[0].value;
            Animator animator = visual.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            HapsirikKozSneeze behaviour = root.AddComponent<HapsirikKozSneeze>();
            var sneezeSettings = new SerializedObject(behaviour);
            sneezeSettings.FindProperty("visualAnimator").objectReferenceValue = animator;
            sneezeSettings.FindProperty("smokeCloudPrefab").objectReferenceValue = smoke;
            sneezeSettings.ApplyModifiedPropertiesWithoutUndo();
            if (patrol)
            {
                EnemyPatrol movement = root.AddComponent<EnemyPatrol>();
                var movementSettings = new SerializedObject(movement);
                movementSettings.FindProperty("bodyVisual").objectReferenceValue = renderer;
                movementSettings.FindProperty("faceMovement").boolValue = true;
                movementSettings.ApplyModifiedPropertiesWithoutUndo();
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
        EnsureFolder(path.Substring(0, separator));
        AssetDatabase.CreateFolder(path.Substring(0, separator), path.Substring(separator + 1));
    }
}
