using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Runs only in an isolated batch project, never in the user's open Unity session.
public static class DeathRespawnVerification
{
    private const string ScenePath = "Assets/Scenes/DeathRespawnVerification.unity";
    private const string OtherScenePath = "Assets/Scenes/DeathRespawnOther.unity";
    private const string EditorOnlyScenePath = "Assets/Scenes/DeathRespawnEditorOnly.unity";
    private static readonly List<string> passed = new List<string>();
    private static int assetChecks;
    private static Keyboard keyboard;

    public static void ConfigureAndRun()
    {
        if (!Application.isBatchMode)
            throw new InvalidOperationException("Use an isolated batch project.");
        try
        {
            DeathRespawnSetup.Configure();
            string prefabGuid = AssetDatabase.AssetPathToGUID("Assets/Prefabs/Player.prefab");
            string clipGuid = AssetDatabase.AssetPathToGUID("Assets/Animations/Death.anim");
            DeathRespawnSetup.Configure();
            Check(prefabGuid == AssetDatabase.AssetPathToGUID("Assets/Prefabs/Player.prefab") &&
                clipGuid == AssetDatabase.AssetPathToGUID("Assets/Animations/Death.anim"), "Configuration is repeatable and preserves asset identity");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            Check(prefab.GetComponent<DeathAbility>() != null && prefab.GetComponentsInChildren<Component>(true).All(c => c != null),
                "Player prefab imports with DeathAbility and no missing scripts");
            Check((int)PlayerStates.State.Knockback == 3 && (int)PlayerStates.State.Death == 4, "Existing serialized state numbers are preserved");
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Death.anim");
            var keys = AnimationUtility.GetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"));
            Check(keys.Length == 8 && keys.Select(k => k.value).Distinct().Count() == 8 && keys.All(k => k.value != null),
                "Death clip references all eight existing frames with equal sample durations");
            Check(Mathf.Abs(clip.length - 0.8f) < 0.001f && !AnimationUtility.GetAnimationClipSettings(clip).loopTime,
                "Death plays once for 0.8 seconds");
            foreach (string path in new[] { "Assets/Animations/Visual.controller", "Assets/Animations/Player.controller" })
            {
                var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                Check(controller.layers[0].stateMachine.states.Any(s => s.state.name == "Death" && s.state.motion == clip),
                    path + " contains the death clip");
            }
            assetChecks = passed.Count;
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            CreateScene(ScenePath);
            CreateScene(OtherScenePath);
            CreateScene(EditorOnlyScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true), new EditorBuildSettingsScene(OtherScenePath, true) };
            EditorSceneManager.OpenScene(ScenePath);
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            EditorApplication.playModeStateChanged += EnteredPlay;
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Finish(error); }
    }

    private static void CreateScene(string path)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var floor = new GameObject("Floor");
        floor.layer = LayerMask.NameToLayer("Ground-Dry");
        floor.transform.position = new Vector2(0f, -0.5f);
        floor.AddComponent<BoxCollider2D>().size = new Vector2(100f, 1f);
        var player = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab"));
        player.transform.position = new Vector2(0f, 1f);
        if (path == OtherScenePath)
        {
            var settings = new SerializedObject(player.GetComponent<WaterMeter>());
            settings.FindProperty("currentWater").floatValue = 0f;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
        var hud = new GameObject("WaterHUD", typeof(RectTransform), typeof(WaterBarUI));
        var fill = new GameObject("WaterFill", typeof(RectTransform));
        fill.transform.SetParent(hud.transform, false);
        var hudSettings = new SerializedObject(hud.GetComponent<WaterBarUI>());
        hudSettings.FindProperty("fill").objectReferenceValue = fill.GetComponent<RectTransform>();
        hudSettings.FindProperty("waterMeter").objectReferenceValue = player.GetComponent<WaterMeter>();
        hudSettings.ApplyModifiedPropertiesWithoutUndo();
        var source = new GameObject("CheckpointSource");
        source.layer = LayerMask.NameToLayer("Water");
        source.transform.position = new Vector2(10f, 0.5f);
        var trigger = source.AddComponent<BoxCollider2D>();
        trigger.size = new Vector2(2f, 1f);
        trigger.isTrigger = true;
        var point = new GameObject("RespawnPoint");
        point.transform.SetParent(source.transform, false);
        point.transform.localPosition = Vector2.up;
        new GameObject("MapMarker").transform.position = new Vector2(20f, 2f);
        var trap = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Traps/Trap_stoneSpike.prefab"));
        trap.name = "ResettableStoneTrap";
        trap.transform.position = new Vector2(30f, 4f);
        if (!EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path))
            throw new InvalidOperationException("Could not save fixture scene: " + path);
    }

    private static async void EnteredPlay(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        try
        {
            Debug.unityLogger.filterLogType = LogType.Warning;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>();
            await Delay(0.5f);
            Player player = FindPlayer();
            WaterMeter water = player.GetComponent<WaterMeter>();
            Check(Vector2.Distance(water.Checkpoint, new Vector2(0f, 1f)) < 0.01f, "Initial checkpoint is the scene start position");
            Keys(Key.D); await Delay(0.2f);
            Check(player.CurrentState == PlayerStates.State.Move && player.GetComponent<Rigidbody2D>().linearVelocity.x > 1f,
                "Normal movement still responds to real Input System events");
            Keys();
            Player previousPlayer = player;
            int deaths = 0;
            int stopped = 0;
            water.Depleted += () => { deaths++; water.RespawnAtCheckpoint(); };
            player.GameplayStopped += () => stopped++;
            Object.Destroy(GameObject.Find("ResettableStoneTrap"));
            GameObject.Find("MapMarker").transform.position = new Vector2(42f, 4f);
            new GameObject("RuntimeHazard");
            water.SetDrainMultiplier(player, 4f);
            water.Consume(200f);
            Check(water.IsEmpty && water.IsRespawning && player.CurrentState == PlayerStates.State.Death && player.isActiveAndEnabled,
                "Lethal depletion immediately enters active DeathAbility once");
            Check(!player.GetComponent<Rigidbody2D>().simulated && player.GetComponent<Rigidbody2D>().linearVelocity == Vector2.zero && stopped == 1,
                "Death stops physics and publishes gameplay cleanup once before reloading");
            water.Refill(); water.SetCheckpoint(new Vector2(99f, 99f)); water.Consume(5f); water.RespawnAtCheckpoint();
            Keys(Key.A, Key.Space);
            await Delay(0.3f);
            var animator = player.GetComponentInChildren<Animator>();
            Check(deaths == 1 && ReferenceEquals(player, previousPlayer) && water.IsEmpty && water.IsRespawning,
                "Repeated damage/refill/respawn and a reentrant Depleted subscriber cannot duplicate or cancel death");
            Check(player.gatherInput.horizontalInput == 0f && player.CurrentState == PlayerStates.State.Death &&
                player.GetComponent<Rigidbody2D>().linearVelocity == Vector2.zero, "Movement and jump cannot interrupt death");
            Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Death") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f,
                "Real Animator plays Death before the map can reload");
            Check(GameObject.Find("WaterFill").GetComponent<RectTransform>().anchorMax.x == 0f, "Existing water UI shows depletion during death");
            Check(player.GetComponentInChildren<SpriteRenderer>().enabled && !player.GetComponent<KnockbackAbility>().IsFlashing &&
                !player.GetComponentInChildren<TrailRenderer>().emitting, "Death remains visible and clears flash and trail");
            Keys();
            await Reloaded(previousPlayer);
            player = FindPlayer(); water = player.GetComponent<WaterMeter>();
            Check(Vector2.Distance(water.Checkpoint, new Vector2(0f, 1f)) < 0.01f && Mathf.Abs(player.transform.position.x) < 0.01f,
                "Before the first source, scene reload respawns at the original start");
            Check(water.CurrentWater > 99f && !water.IsRespawning && player.GetComponent<Rigidbody2D>().simulated && player.isActiveAndEnabled,
                "Reload restores full water, simulation, controls and a fresh player");
            Check(GameObject.Find("WaterFill").GetComponent<RectTransform>().anchorMax.x > 0.99f, "Reloaded UI binds to the new full WaterMeter");
            Check(GameObject.Find("ResettableStoneTrap") != null && GameObject.Find("RuntimeHazard") == null &&
                Vector2.Distance(GameObject.Find("MapMarker").transform.position, new Vector2(20f, 2f)) < 0.01f,
                "Map reload restores destroyed stone traps and map transforms and removes runtime hazards");
            var trail = player.GetComponentInChildren<TrailRenderer>();
            var points = new Vector3[trail.positionCount];
            trail.GetPositions(points);
            Check(water.DrainMultiplier == 1f && player.GetComponent<PhysicsControl>().BodyScale > 0.99f &&
                points.All(point => Mathf.Abs(point.x) < 0.1f), "Respawn clears heat, size penalty and old trail points");

            // Reach a real trigger with a RespawnPoint, rather than assigning the checkpoint directly.
            Teleport(player, new Vector2(10f, 0.6f));
            await Delay(0.1f);
            Check(Vector2.Distance(water.Checkpoint, new Vector2(10f, 1.5f)) < 0.01f && water.CurrentWater > 99f,
                "Water source contact refills and records its safe RespawnPoint");
            Teleport(player, new Vector2(15f, 1f));
            await Delay(0.1f);
            // Kill during flash to ensure the hurt sprite does not override the death clip.
            var knockback = player.GetComponent<KnockbackAbility>();
            Check(knockback.TryWaterHit(20f) && knockback.IsFlashing, "Nonlethal hit still starts shared flash immunity");
            previousPlayer = player;
            water.Consume(200f);
            await Delay(0.2f);
            Check(!knockback.IsFlashing && player.GetComponentInChildren<SpriteRenderer>().enabled &&
                player.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Death"),
                "Death overrides a live hurt flash and keeps its animation visible");
            await Reloaded(previousPlayer);
            player = FindPlayer(); water = player.GetComponent<WaterMeter>();
            Check(Vector2.Distance(water.Checkpoint, new Vector2(10f, 1.5f)) < 0.01f && Mathf.Abs(player.transform.position.x - 10f) < 0.01f,
                "Last source checkpoint survives map reload");
            previousPlayer = player;
            water.Consume(200f);
            await Reloaded(previousPlayer);
            player = FindPlayer(); water = player.GetComponent<WaterMeter>();
            Check(Mathf.Abs(player.transform.position.x - 10f) < 0.01f, "Repeated deaths retain the last checkpoint instead of reverting to the level start");

            // Death from falling follows the same animation and reload path with water remaining.
            previousPlayer = player;
            Teleport(player, new Vector2(15f, -20f));
            await Delay(0.1f);
            Check(water.IsRespawning && !water.IsEmpty && player.CurrentState == PlayerStates.State.Death,
                "Falling below the limit uses the same DeathAbility even with water left");
            await Reloaded(previousPlayer);
            player = FindPlayer(); water = player.GetComponent<WaterMeter>();
            Check(Mathf.Abs(player.transform.position.x - 10f) < 0.01f && water.CurrentWater > 99f, "Fall death also reloads safely at the checkpoint");

            // Missing animation must never leave a player stuck in death.
            previousPlayer = player;
            player.GetComponentInChildren<Animator>().enabled = false;
            water.Consume(200f);
            await Reloaded(previousPlayer);
            Check(FindPlayer().GetComponent<WaterMeter>().CurrentWater > 99f, "Disabled Animator allows an immediate safe reload");

            player = FindPlayer(); previousPlayer = player;
            player.GetComponent<WaterMeter>().Consume(200f);
            player.enabled = false;
            await Reloaded(previousPlayer);
            Check(FindPlayer().isActiveAndEnabled, "Disabling Player during death cannot strand the respawn coroutine");

            SceneManager.LoadScene(OtherScenePath);
            await Delay(0.1f);
            player = FindPlayer();
            Check(player.CurrentState == PlayerStates.State.Death && player.GetComponent<WaterMeter>().IsRespawning,
                "A scene that starts with zero water enters death safely during component initialization");
            await Reloaded(player);
            await Delay(0.5f);
            player = FindPlayer(); water = player.GetComponent<WaterMeter>();
            Check(Mathf.Abs(player.transform.position.x) < 0.01f && Mathf.Abs(water.Checkpoint.x) < 0.01f,
                "Changing levels does not carry over a previous scene's checkpoint");
            Keys(Key.Space); await Delay(0.15f);
            Check(player.CurrentState == PlayerStates.State.Jump && player.GetComponent<Rigidbody2D>().linearVelocity.y > 0f,
                "Normal jump still works after death and level transitions");
            Keys();
            await Delay(0.1f);
            Check(player.GetComponent<Rigidbody2D>().linearVelocity.y <= 0f, "Early jump release still ends ascent");

            Check(!Application.CanStreamedLevelBeLoaded(EditorOnlyScenePath), "Editor-only fixture is intentionally outside the build list");
            var editorLoad = EditorSceneManager.LoadSceneAsyncInPlayMode(EditorOnlyScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            while (!editorLoad.isDone) await Task.Delay(2);
            await Delay(0.1f);
            player = FindPlayer(); previousPlayer = player; water = player.GetComponent<WaterMeter>();
            water.SetCheckpoint(new Vector2(7f, 1f));
            Object.Destroy(GameObject.Find("ResettableStoneTrap"));
            new GameObject("RuntimeHazard");
            water.Consume(200f);
            await Reloaded(previousPlayer);
            player = FindPlayer();
            Check(player.gameObject.scene.path == EditorOnlyScenePath && Mathf.Abs(player.transform.position.x - 7f) < 0.01f &&
                GameObject.Find("ResettableStoneTrap") != null && GameObject.Find("RuntimeHazard") == null,
                "Saved Editor scene outside the build list also resets the map and respawns at its last checkpoint");
            Finish(null);
        }
        catch (Exception error) { Finish(error); }
    }

    private static Player FindPlayer() => Object.FindAnyObjectByType<Player>();
    private static void Teleport(Player player, Vector2 position)
    {
        player.GetComponent<Rigidbody2D>().position = position;
        player.transform.position = position;
        player.physicsControl.ResetMotion();
        player.GetComponent<WaterMeter>().ResetPositionTracking();
        Physics2D.SyncTransforms();
    }
    private static void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
    private static async Task Reloaded(Player previousPlayer)
    {
        double timeout = EditorApplication.timeSinceStartup + 20f;
        while (FindPlayer() == null || ReferenceEquals(FindPlayer(), previousPlayer))
        {
            if (EditorApplication.timeSinceStartup > timeout) throw new TimeoutException("Scene did not respawn");
            await Task.Delay(2);
        }
        await Delay(0.025f);
    }
    private static async Task Delay(float seconds)
    {
        float until = Time.time + seconds;
        double timeout = EditorApplication.timeSinceStartup + 20f;
        while (Time.time < until)
        {
            if (EditorApplication.timeSinceStartup > timeout) throw new TimeoutException("Play Mode did not advance");
            await Task.Delay(2);
        }
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        passed.Add(message);
    }
    private static void Finish(Exception error)
    {
        string result = error == null
            ? $"PASS: Unity {Application.unityVersion}; {passed.Count} checks ({assetChecks} asset, {passed.Count - assetChecks} Play Mode). Real Player with Slip physics material, death frames, controllers, Input System, UI and stone prefab in three saved fixture scenes, including one outside the build list; main levels and exported build not tested."
            : "FAIL after " + passed.Count + " checks: " + error;
        File.WriteAllText("death-respawn-result.txt", result);
        File.WriteAllLines("death-respawn-checks.txt", passed);
        Debug.unityLogger.filterLogType = LogType.Log;
        Debug.Log(result);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}
