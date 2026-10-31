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
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Run only in a separate batch project: -executeMethod EmberSuctionVerification.Run.
// Tests real prefabs, physics steps and input, then exits that batch editor with a report.
public static class EmberSuctionVerification
{
    private static readonly List<GameObject> objects = new List<GameObject>();
    private static Player player;
    private static PhysicsControl physics;
    private static WaterMeter water;
    private static EmberSuctionEnemy enemy;
    private static Keyboard keyboard;
    private static int checks, assetChecks;

    public static void GenerateAndRun()
    {
        if (!Application.isBatchMode)
            throw new InvalidOperationException("Use a separate batch project for this verification.");
        EmberSuctionPrefabBuilder.RebuildAll();
        Run();
    }

    public static void Run()
    {
        if (!Application.isBatchMode)
            throw new InvalidOperationException("Use a separate batch project for this verification.");
        try
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CheckAssets();
            assetChecks = checks;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            EditorApplication.playModeStateChanged += EnteredPlay;
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Finish(error); }
    }

    private static void CheckAssets()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EmberSuctionPrefabBuilder.PrefabPath);
        Check(prefab != null && prefab.GetComponentsInChildren<EmberSuctionEnemy>().Length == 1, "One stationary suction component in the saved prefab");
        Check(prefab.GetComponentsInChildren<Rigidbody2D>().Length == 0 && prefab.GetComponentsInChildren<EnemyContactKnockback>().Length == 0 &&
            prefab.GetComponentsInChildren<EnemyPatrol>().Length == 0 && prefab.GetComponentsInChildren<HapsirikKozSneeze>().Length == 0,
            "No added body, patrol, projectiles, body-hit damage or knockback");
        Check(prefab.transform.Find("Visual") != null && prefab.transform.Find("RangeOrigin") != null && prefab.transform.Find("PullPoint") != null && prefab.transform.Find("EffectArea") != null,
            "Visual, range origin, mouth and trigger have separate transforms");
        EmberSuctionEnemy behaviour = prefab.GetComponentInChildren<EmberSuctionEnemy>();
        CircleCollider2D area = behaviour.GetComponent<CircleCollider2D>();
        var settings = new SerializedObject(behaviour);
        Check(area.isTrigger && Mathf.Approximately(area.radius, 3f) && prefab.GetComponentsInChildren<Collider2D>().Length == 1 &&
            settings.FindProperty("playerLayerMask").intValue == LayerMask.GetMask("Player"), "Only the explicit 3-unit Player-filtered trigger is present");
        Check(settings.FindProperty("rangeOrigin").objectReferenceValue == prefab.transform.Find("RangeOrigin") &&
            settings.FindProperty("pullPoint").objectReferenceValue == prefab.transform.Find("PullPoint"), "Both origin references are wired");
        Sprite sprite = prefab.transform.Find("Visual").GetComponent<SpriteRenderer>().sprite;
        Check(sprite != null && sprite.name == "Idle_00" && sprite.rect.size == new Vector2(128f, 128f) && sprite.pixelsPerUnit == 128f &&
            Mathf.Abs(sprite.pivot.y - 20.266666f) < 0.01f, "Real idle sheet sprite has its existing 128 PPU, cell and foot pivot");
        Scene demo = EditorSceneManager.OpenScene(EmberSuctionPrefabBuilder.DemoPath, OpenSceneMode.Single);
        try
        {
            var roots = demo.GetRootGameObjects();
            Player demoPlayer = roots.SelectMany(root => root.GetComponentsInChildren<Player>()).Single();
            WaterBarUI hud = roots.SelectMany(root => root.GetComponentsInChildren<WaterBarUI>()).Single();
            Check(new SerializedObject(hud).FindProperty("waterMeter").objectReferenceValue == demoPlayer.GetComponent<WaterMeter>(), "Demo uses the existing water HUD wired to its actual Player");
            Check(demoPlayer.transform.Find("Visual").GetComponent<SpriteRenderer>().sharedMaterial.shader.name == "Sprites/Default" &&
                prefab.transform.Find("Visual").GetComponent<SpriteRenderer>().sharedMaterial.shader.name == "Sprites/Default",
                "Demo Player and furnace use unlit sprite materials without needing scene lights");
            EmberSuctionEnemy demoEnemy = roots.SelectMany(root => root.GetComponentsInChildren<EmberSuctionEnemy>()).Single();
            Check(Vector2.Distance(demoPlayer.transform.position, demoEnemy.transform.position) > 3.5f &&
                roots.Any(root => root.GetComponent<EmberSuctionDemoControls>() != null) && roots.Any(root => root.name == "Dry Ground"),
                "Saved demo has a safe starting area, dry floor, one furnace and restart controls");
        }
        finally { EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); }
    }

    private static async void EnteredPlay(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        try
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.targetFrameRate = 120;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>();
            await RatesAndRange();
            await PullAndEscape();
            await MultipleContactsAndSources();
            await Lifecycle();
            await JumpAndNormalDrain();
            await TimingAndValidation();
            await DemoRestart();
            Finish(null);
        }
        catch (Exception error) { Finish(error); }
    }

    private static async Task RatesAndRange()
    {
        Setup(-4f);
        await Delay(0.15f);
        Check(await Rate() < 0.001f && Mathf.Abs(physics.rb.linearVelocity.x) < 0.001f, "Outside grounded idle has zero pull and zero extra/normal loss");
        Set(enemy, "minPullStrength", 0f); Set(enemy, "maxPullStrength", 0f);
        physics.rb.constraints = RigidbodyConstraints2D.FreezeAll;
        Teleport(-3.1f);
        Check(await Rate() < 0.001f, "A collider overlapping the trigger with its body centre outside produces no effect");
        Teleport(-2.9f); float edge = await Rate();
        Teleport(-1.8f); float middle = await Rate();
        Teleport(-0.4f); float inner = await Rate();
        Check(edge > 1f && edge < 1.4f && middle > 3.4f && middle < 3.9f && Mathf.Abs(inner - 6f) < 0.02f,
            "Distance increases stationary additional points/sec: edge=" + edge + ", middle=" + middle + ", inner=" + inner);
        water.Refill(); float full = await Rate(0.5f);
        water.Consume(40f); float low = await Rate(0.5f);
        Check(Mathf.Abs(full - 6f) < 0.02f && Mathf.Abs(low - full) < 0.02f, "Full and lower starting water both lose 6 points/sec (about 3 points in 0.5s)");
        KnockbackAbility knockback = player.GetComponent<KnockbackAbility>();
        Set(knockback, "force", Vector2.zero); Set(knockback, "contactWaterLoss", 0f);
        Check(knockback.TryKnockback(Vector2.left), "Fixture enters actual body-hit flash");
        Check(Mathf.Abs(await Rate() - 6f) < 0.02f && knockback.IsFlashing, "Continuous suction water loss remains active during body-hit immunity");
        water.SetDrainMultiplier(enemy, 2f);
        Check(Mathf.Abs(await Rate() - 12f) < 0.03f, "Heat-scaled dry loss (6) and independent suction loss (6) add to 12, with no multiplier on suction");
        await Clear();
    }

    private static async Task PullAndEscape()
    {
        Setup(-2f); ZeroNormalDrain(); ZeroEnemyDrain();
        Set(enemy, "innerFullEffectRadius", 2.9f); Set(enemy, "minPullStrength", 2.5f);
        await Delay(0.08f); float initial = physics.rb.linearVelocity.x;
        await Delay(0.32f);
        Check(initial > 0f && initial < 1.2f && Mathf.Abs(physics.rb.linearVelocity.x - 2.5f) < 0.02f,
            "Entry ramps the bounded motor offset from a gentle start to 2.5 world units/sec");
        Check(Mathf.Abs(physics.rb.linearVelocity.y) < 0.02f && player.CurrentState == PlayerStates.State.Idle, "Idle suction moves horizontally without directly adding vertical speed");
        enemy.enabled = false;
        physics.ApproachHorizontalSpeed(0f, 10000f);
        Check(Mathf.Abs(physics.rb.linearVelocity.x) < 0.001f, "Disabling the source immediately removes its motor contribution");
        await Clear();
        foreach (bool rightSide in new[] { false, true })
        {
            Setup(rightSide ? 0.4f : -0.4f); Set(enemy, "activationRampDuration", 0f);
            await Delay(0.02f);
            Keys(rightSide ? Key.D : Key.A);
            await Delay(1.1f);
            Check(rightSide ? physics.rb.position.x > 3.1f : physics.rb.position.x < -3.1f,
                "Actual full opposite input escapes the strongest zone on the " + (rightSide ? "right" : "left"));
            await Clear();
        }
        Setup(0.02f); ZeroNormalDrain(); ZeroEnemyDrain(); Set(enemy, "activationRampDuration", 0f);
        float min = physics.rb.position.x, max = min;
        await Delay(0.5f, () => { min = Mathf.Min(min, physics.rb.position.x); max = Mathf.Max(max, physics.rb.position.x); });
        Check(max - min < 0.001f, "The horizontal mouth dead zone has no alternating pull or idle jitter");
        await Clear();
    }

    private static async Task MultipleContactsAndSources()
    {
        Setup(-0.4f); ZeroNormalDrain();
        physics.rb.constraints = RigidbodyConstraints2D.FreezeAll;
        var extra = new GameObject("Second player collider"); extra.layer = player.gameObject.layer;
        extra.transform.SetParent(player.transform, false); extra.AddComponent<CircleCollider2D>().radius = 0.25f;
        await Delay(0.08f);
        Check(Mathf.Abs(await Rate() - 6f) < 0.02f, "Multiple player colliders do not multiply the water loss");
        player.GetComponent<CircleCollider2D>().enabled = false;
        Check(Mathf.Abs(await Rate() - 6f) < 0.02f, "Losing one collider does not end the remaining player's contact");
        extra.GetComponent<Collider2D>().enabled = false;
        Check(await Rate() < 0.001f, "Disabling the last player collider clears the effect without Exit callbacks");
        extra.GetComponent<Collider2D>().enabled = true;
        EmberSuctionEnemy second = SpawnEnemy(); Set(second, "minWaterLossPerSecond", 3f); Set(second, "maxWaterLossPerSecond", 3f);
        Check(Mathf.Abs(await Rate() - 9f) < 0.03f, "Two independent sources add their 6 and 3 points/sec once each");
        second.enabled = false;
        Check(Mathf.Abs(await Rate() - 6f) < 0.02f, "One source exiting leaves the other water source active");
        await Clear();
        Setup(-1.5f); ZeroNormalDrain(); ZeroEnemyDrain();
        Set(enemy, "minPullStrength", 0.5f); Set(enemy, "maxPullStrength", 0.5f); Set(enemy, "activationRampDuration", 0f);
        second = SpawnEnemy(); Set(second, "minPullStrength", 0.7f); Set(second, "maxPullStrength", 0.7f);
        Set(second, "activationRampDuration", 0f); Set(second, "minWaterLossPerSecond", 0f); Set(second, "maxWaterLossPerSecond", 0f);
        await Delay(0.12f);
        Check(Mathf.Abs(physics.rb.linearVelocity.x - 1.2f) < 0.02f, "Horizontal motor contributions are composed per source");
        second.enabled = false; await Delay(0.08f);
        Check(Mathf.Abs(physics.rb.linearVelocity.x - 0.5f) < 0.02f, "Removing one pull source preserves the other pull");
        await Clear();
    }

    private static async Task Lifecycle()
    {
        Setup(-0.4f); ZeroNormalDrain(); physics.rb.constraints = RigidbodyConstraints2D.FreezeAll;
        await Delay(0.08f);
        enemy.GetComponent<Collider2D>().enabled = false;
        Check(await Rate() < 0.001f, "Disabling the trigger stops environmental loss");
        enemy.GetComponent<Collider2D>().enabled = true;
        Check(Mathf.Abs(await Rate() - 6f) < 0.02f, "Re-enabled trigger reacquires the player");
        Teleport(-5f); Check(await Rate() < 0.001f, "Teleport out clears loss without relying on a trigger exit");
        Teleport(-0.4f); await Delay(0.06f);
        player.enabled = false; Check(await Rate() < 0.001f, "Disabled control / non-playing state takes no environmental loss");
        player.enabled = true; water.enabled = false; Check(await Rate() < 0.001f, "Disabled WaterMeter takes no environmental loss");
        water.enabled = true; physics.rb.simulated = false; Check(await Rate() < 0.001f, "Non-simulated player takes no environmental loss");
        physics.rb.simulated = true;
        await Delay(0.06f); Time.timeScale = 0f;
        float pausedWater = water.CurrentWater;
        await Task.Delay(100);
        Check(Mathf.Approximately(water.CurrentWater, pausedWater), "Pause stops fixed-step environmental loss");
        Time.timeScale = 1f;
        water.Refill(); water.SetCheckpoint(new Vector2(-5f, 0f));
        Set(enemy, "minWaterLossPerSecond", 1000f); Set(enemy, "maxWaterLossPerSecond", 1000f);
        int deaths = 0; water.Depleted += () => deaths++;
        await Delay(0.18f);
        Check(water.IsRespawning && water.CurrentWater == 0f && deaths == 1, "Environmental depletion clamps at zero and starts exactly one existing respawn");
        await Delay(0.45f);
        Check(!water.IsRespawning && player.enabled && water.CurrentWater == 100f && deaths == 1 && physics.rb.position.x < -4.9f,
            "Respawn restores water and control at the safe checkpoint without old effects");
        Set(enemy, "minWaterLossPerSecond", 6f); Set(enemy, "maxWaterLossPerSecond", 6f);
        Teleport(-0.4f); await Delay(0.06f);
        Object.DestroyImmediate(enemy.transform.parent.gameObject);
        Check(await Rate() < 0.001f, "Destroying the source clears its loss and pull");
        await Clear();
        Setup(-0.4f); ZeroNormalDrain();
        Scene temporary = SceneManager.CreateScene("Suction source lifecycle");
        SceneManager.MoveGameObjectToScene(enemy.transform.parent.gameObject, temporary);
        await Delay(0.06f);
        AsyncOperation unloading = SceneManager.UnloadSceneAsync(temporary);
        while (!unloading.isDone) await Task.Delay(2);
        await Delay(0.08f);
        Check(await Rate() < 0.001f && Mathf.Abs(physics.rb.linearVelocity.x) < 0.001f, "Unloading the source's scene leaves no motor or water contribution");
        await Clear();
    }

    private static async Task JumpAndNormalDrain()
    {
        Setup(-5f); enemy.enabled = false; await Delay(0.12f);
        Keys(Key.D); await Delay(0.15f);
        float dry = await Rate();
        Check(dry > 2.98f && dry < 3.02f && player.CurrentState == PlayerStates.State.Move, "Original dry movement still consumes 3 points/sec");
        Keys(); Teleport(-5f); await Delay(0.12f);
        objects[0].layer = LayerMask.NameToLayer("Ground-Wet");
        Keys(Key.D); await Delay(0.12f);
        Check(Mathf.Abs(await Rate() - 1f) < 0.02f, "Original wet-ground movement still consumes 1 point/sec");
        await Clear();
        float[] apex = new float[2];
        for (int i = 0; i < 2; i++)
        {
            Setup(-0.5f); ZeroNormalDrain(); ZeroEnemyDrain();
            if (i == 0) enemy.enabled = false;
            await Delay(0.12f);
            Keys(Key.Space);
            await Delay(1.2f, () => apex[i] = Mathf.Max(apex[i], physics.rb.position.y));
            Keys();
            Check(player.CurrentState == PlayerStates.State.Jump, "Jump remains airborne near the apex, run " + i);
            await Clear();
        }
        Check(Mathf.Abs(apex[0] - apex[1]) < 0.03f && apex[0] > 4f, "Suction does not change actual jump height: " + apex[0] + " vs " + apex[1]);
    }

    private static async Task TimingAndValidation()
    {
        foreach (float step in new[] { 0.01f, 0.04f })
        {
            Time.fixedDeltaTime = step;
            foreach (int fps in new[] { 30, 120 })
            {
                Application.targetFrameRate = fps;
                Setup(-0.4f); ZeroNormalDrain(); physics.rb.constraints = RigidbodyConstraints2D.FreezeAll;
                await Delay(0.1f);
                Check(Mathf.Abs(await Rate(0.5f) - 6f) < 0.03f, "6 points/sec at target render FPS " + fps + " and fixed step " + step);
                await Clear();
            }
        }
        Time.fixedDeltaTime = 0.02f; Application.targetFrameRate = 120;
        Setup(-0.4f); ZeroNormalDrain();
        enemy.transform.parent.localScale = new Vector3(2f, 2f, 1f);
        await Delay(0.06f);
        Check(Mathf.Abs(enemy.GetComponent<CircleCollider2D>().bounds.extents.x - 3f) < 0.02f, "Trigger stays at the configured world radius after root scaling");
        Set(enemy, "effectRadius", -1f); Set(enemy, "innerFullEffectRadius", 4f);
        Set(enemy, "minWaterLossPerSecond", -3f); Set(enemy, "maxPullStrength", float.NaN);
        enemy.SendMessage("OnValidate");
        var settings = new SerializedObject(enemy);
        Check(settings.FindProperty("effectRadius").floatValue > settings.FindProperty("innerFullEffectRadius").floatValue &&
            settings.FindProperty("innerFullEffectRadius").floatValue >= 0f && settings.FindProperty("minWaterLossPerSecond").floatValue == 0f &&
            !float.IsNaN(settings.FindProperty("maxPullStrength").floatValue), "Invalid radius, negative and non-finite settings are sanitized");
        await Clear();
    }

    private static async Task DemoRestart()
    {
        Scene demo = EditorSceneManager.LoadSceneInPlayMode(EmberSuctionPrefabBuilder.DemoPath, new LoadSceneParameters(LoadSceneMode.Additive));
        await Delay(0.15f);
        var demoPlayer = demo.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Player>()).Single();
        var demoWater = demoPlayer.GetComponent<WaterMeter>();
        var demoPhysics = demoPlayer.GetComponent<PhysicsControl>();
        float before = demoWater.CurrentWater;
        await Delay(0.2f);
        Check(demoWater.CurrentWater == before && demoPhysics.grounded, "Actual saved demo starts grounded in the safe zone with stable water");
        demoPhysics.rb.position = new Vector2(0.6f, 0.51f); Physics2D.SyncTransforms();
        await Delay(0.2f);
        Check(demoWater.CurrentWater < before, "Actual saved demo applies furnace loss when approached");
        // Let the player loop consume this edge; a manual InputSystem.Update here would
        // consume wasPressedThisFrame before the demo's MonoBehaviour.Update can read it.
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
        await Delay(0.06f); Keys(); await Delay(0.45f);
        Check(demoPlayer.isActiveAndEnabled && !demoWater.IsRespawning && demoWater.CurrentWater == 100f && demoPhysics.rb.position.x < -5.9f,
            "Actual demo R shortcut returns to the start with full water and no retained suction; water=" + demoWater.CurrentWater + ", position=" + demoPhysics.rb.position + ", respawning=" + demoWater.IsRespawning);
        Camera camera = demo.GetRootGameObjects().Select(root => root.GetComponent<Camera>()).First(candidate => candidate != null);
        var texture = new RenderTexture(960, 540, 24);
        var capture = new Texture2D(960, 540, TextureFormat.RGB24, false);
        RenderTexture previous = RenderTexture.active;
        try
        {
            camera.targetTexture = texture; camera.Render(); RenderTexture.active = texture;
            capture.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); capture.Apply();
            File.WriteAllBytes("koz-ocagi-demo.png", capture.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = null; RenderTexture.active = previous;
            Object.DestroyImmediate(texture); Object.DestroyImmediate(capture);
        }
        SceneManager.UnloadSceneAsync(demo);
    }

    private static void Setup(float x)
    {
        var ground = new GameObject("Dry fixture"); objects.Add(ground);
        ground.layer = LayerMask.NameToLayer("Ground-Dry"); ground.transform.position = new Vector2(0f, -1f);
        ground.AddComponent<BoxCollider2D>().size = new Vector2(100f, 1f);
        var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab"), new Vector3(x, 0f, 0f), Quaternion.identity);
        objects.Add(root);
        player = root.GetComponent<Player>(); physics = root.GetComponent<PhysicsControl>(); water = root.GetComponent<WaterMeter>();
        Set(physics, "extraDryGroundLayer", (LayerMask)LayerMask.GetMask("Ground-Extra-Dry"));
        root.transform.Find("TrailAnchor").GetComponent<TrailRenderer>().enabled = false;
        enemy = SpawnEnemy(); Physics2D.SyncTransforms();
    }

    private static EmberSuctionEnemy SpawnEnemy()
    {
        var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(EmberSuctionPrefabBuilder.PrefabPath), new Vector3(0f, -0.5f, 0f), Quaternion.identity);
        objects.Add(root); Physics2D.SyncTransforms(); return root.GetComponentInChildren<EmberSuctionEnemy>();
    }

    private static void Teleport(float x)
    {
        physics.rb.position = new Vector2(x, 0f); physics.ResetMotion(); water.ResetPositionTracking(); Physics2D.SyncTransforms();
    }
    private static void ZeroNormalDrain() { Set(water, "dryDrainPerSecond", 0f); Set(water, "wetDrainPerSecond", 0f); }
    private static void ZeroEnemyDrain() { Set(enemy, "minWaterLossPerSecond", 0f); Set(enemy, "maxWaterLossPerSecond", 0f); }
    private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    private static void Keys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); InputSystem.Update(); }
    private static async Task<float> Rate(float duration = 0.24f)
    {
        await Delay(Time.fixedDeltaTime * 2f);
        float before = water.CurrentWater, start = Time.fixedTime;
        await Delay(duration);
        return (before - water.CurrentWater) / (Time.fixedTime - start);
    }
    private static async Task Delay(float seconds, Action sample = null)
    {
        float end = Time.time + seconds;
        double limit = EditorApplication.timeSinceStartup + 20f;
        while (Time.time < end)
        {
            if (EditorApplication.timeSinceStartup > limit) throw new TimeoutException("Play Mode stopped advancing");
            sample?.Invoke(); await Task.Delay(2);
        }
        sample?.Invoke();
    }
    private static async Task Clear()
    {
        Keys();
        foreach (GameObject root in objects) if (root != null) Object.DestroyImmediate(root);
        objects.Clear(); await Delay(0.04f);
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++; Debug.Log("EMBER_PASS " + message);
    }
    private static void Finish(Exception error)
    {
        Time.timeScale = 1f; Time.fixedDeltaTime = 0.02f;
        string report = error == null ? "PASS: Unity " + Application.unityVersion + "; " + checks + " checks (" + assetChecks + " asset, " + (checks - assetChecks) + " Play Mode). Isolated project; main level rendering and exported build not tested."
            : "FAIL after " + checks + " checks: " + error;
        File.WriteAllText("ember-suction-result.txt", report); Debug.Log("EMBER_RESULT " + report);
        if (error != null) Debug.LogException(error);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}
