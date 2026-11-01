using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

public static class KnockbackLoopVerification
{
    private static readonly List<string> checks = new List<string>();
    private static readonly List<string> errors = new List<string>();
    private static bool expectRepeatedSound;
    private static Keyboard keyboard;

    public static void Reproduce() { expectRepeatedSound = true; Start(); }
    public static void Verify() { expectRepeatedSound = false; Start(); }

    private static void Start()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use an isolated project.");
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorApplication.playModeStateChanged += Entered;
        EditorApplication.EnterPlaymode();
    }

    private static async void Entered(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        try
        {
            AudioListener.volume = 0f;
            Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            Application.logMessageReceived += Log;
            keyboard = InputSystem.AddDevice<Keyboard>();
            var camera = new GameObject("Test Camera");
            camera.tag = "MainCamera";
            camera.AddComponent<Camera>();
            camera.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0f, 0f, -5f);
            var floor = new GameObject("Ground");
            floor.layer = LayerMask.NameToLayer("Ground-Dry");
            floor.transform.position = new Vector2(0f, -1.25f);
            floor.AddComponent<BoxCollider2D>().size = new Vector2(100f, 1f);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Knockback.anim");
            var soundEvent = clip.events.Single(e => e.functionName == "PlaySound");
            KnockbackSoundProbe.ObservedClip = soundEvent.objectReferenceParameter as AudioClip;
            Check(KnockbackSoundProbe.ObservedClip != null, "original hurt AudioClip still assigned to the animation event");
            Check(clip.isLooping == expectRepeatedSound, "expected clip loop setting is imported by Unity");
            foreach (string controller in new[] { "Visual", "Player" })
                await CheckController(controller);
            Check(errors.Count == 0, "no gameplay exceptions");
            Finish(null);
        }
        catch (Exception error) { Finish(error); }
    }

    private static async Task CheckController(string controller)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        InputSystem.Update();
        var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab"));
        root.transform.position = Vector3.zero;
        var player = root.GetComponent<Player>();
        var water = root.GetComponent<WaterMeter>();
        foreach (string field in new[] { "dryDrainPerSecond", "wetDrainPerSecond", "extraDryDrainPerSecond", "dryIdleDrainPerSecond", "wetIdleDrainPerSecond", "extraDryIdleDrainPerSecond" }) Set(water, field, 0f);
        Set(water, "fallLimit", -10000f);
        var hit = root.GetComponent<KnockbackAbility>();
        Animator animator = root.GetComponentInChildren<Animator>();
        var controllerAsset = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/" + controller + ".controller");
        Check(controllerAsset != null, controller + ": controller asset is present in the isolated fixture");
        animator.runtimeAnimatorController = controllerAsset;
        var probe = animator.gameObject.AddComponent<KnockbackSoundProbe>();
        await Task.Delay(120);
        int hits = 0;
        hit.HitAccepted += () => hits++;
        Check(hit.TryKnockback(Vector2.left), controller + ": first hit accepted");
        Check(player.CurrentState == PlayerStates.State.Knockback, controller + ": ability enters knockback once");
        await Until(() => Get<float>(hit, "elapsed") >= 0.55f || player.CurrentState != PlayerStates.State.Knockback);
        Check(player.CurrentState != PlayerStates.State.Knockback, controller + ": ability exits after lock plus recovery");
        Check(hit.IsFlashing, controller + ": flash protection remains after control recovery");
        Check(hits == 1, controller + ": only one physical hit and damage event");
        Check(water.CurrentWater == 90f, controller + ": one ten-point water loss");
        Check(!hit.TryKnockback(Vector2.left), controller + ": repeated contact during flash is rejected");
        Check(animator.GetInteger("State") == 3, controller + ": flash holds the hurt visual independently of the ability");
        float position = player.transform.position.x;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
        InputSystem.Update();
        var move = Get<InputAction>(player.gatherInput, "moveAction");
        checks.Add("INPUT immediate: key=" + keyboard.dKey.isPressed + ", enabled=" + move.enabled + ", value=" + move.ReadValue<Vector2>());
        await Task.Delay(160);
        checks.Add("INPUT later: key=" + keyboard.dKey.isPressed + ", enabled=" + move.enabled + ", value=" + move.ReadValue<Vector2>() +
            ", startX=" + position + ", endX=" + player.transform.position.x + ", velocity=" + player.physicsControl.rb.linearVelocity +
            ", state=" + player.CurrentState + ", ability=" + player.stateMachine.CurrentAbility.GetType().Name + ", constraints=" + player.physicsControl.rb.constraints);
        Check(player.gatherInput.horizontalInput > 0.5f && player.transform.position.x > position,
            controller + ": normal right input moves the player during flash");
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        InputSystem.Update();
        await Until(() => !hit.IsFlashing);
        await Task.Delay(100);
        int firstHitEvents = probe.Events;
        checks.Add("MEASURE " + controller + ": physical hits=" + hits + ", animation sound events=" + firstHitEvents + ", physical state=" + player.CurrentState);
        Check(expectRepeatedSound ? firstHitEvents > 5 : firstHitEvents == 1,
            controller + (expectRepeatedSound ? ": reproduces repeated sound without repeated ability entry" : ": sound event fires once for the whole flash"));
        Check(animator.GetInteger("State") != 3, controller + ": normal animation returns after flash");
        await Task.Delay(150);
        Check(probe.Events == firstHitEvents, controller + ": no extra hurt events after recovery");
        if (!expectRepeatedSound)
        {
            Check(hit.TryKnockback(Vector2.right), controller + ": a later new hit remains possible");
            await Task.Delay(450);
            Check(probe.Events == firstHitEvents + 1 && hits == 2, controller + ": second hit adds exactly one sound");
            root.SetActive(false);
            root.SetActive(true);
            await Task.Delay(100);
            Check(!hit.IsFlashing && player.CurrentState != PlayerStates.State.Knockback,
                controller + ": disable/enable does not resume old knockback");
        }
        Object.Destroy(root);
        await Task.Delay(120);
    }

    private static async Task Until(Func<bool> condition)
    {
        for (int i = 0; i < 160; i++) { if (condition()) return; await Task.Delay(25); }
        throw new Exception("Timed out waiting for knockback progress");
    }
    private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
    private static T Get<T>(object target, string field) => (T)target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
    private static void Check(bool success, string label) { if (!success) throw new Exception(label); checks.Add("PASS " + label); }
    private static void Log(string message, string stack, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception) return;
        if (message.StartsWith("ArgumentOutOfRangeException") && stack.Contains("UnityEditor.Search.")) return;
        errors.Add(message + "\n" + stack);
    }
    private static void Finish(Exception error)
    {
        checks.AddRange(errors.Select(message => "LOG_ERROR " + message));
        checks.Add(error == null ? "RESULT PASS " + checks.Count(s => s.StartsWith("PASS ")) : "RESULT FAIL " + error);
        File.WriteAllLines(expectRepeatedSound ? "knockback-loop-before.txt" : "knockback-loop-after.txt", checks);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}
