using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

// Run with -executeMethod WaterDrainVerification.Run in an isolated project copy.
public static class WaterDrainVerification
{
    private static readonly List<GameObject> objects = new List<GameObject>();
    private static readonly List<string> passed = new List<string>();
    private static readonly string[] layers = { "Ground-Wet", "Ground-Dry", "Ground-Extra-Dry" };
    private static readonly float[] idleRates = { 0.25f, 0.5f, 2f };
    private static readonly float[] moveRates = { 1f, 3f, 6f };
    private static Player player;
    private static PhysicsControl physics;
    private static WaterMeter water;
    private static GameObject ground;
    private static GroundSurface surface;
    private static Keyboard keyboard;

    public static void Run()
    {
        try
        {
            if (!Application.isBatchMode)
                throw new InvalidOperationException("Run in a separate batch project; this test replaces the scene.");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            var settings = new SerializedObject(prefab.GetComponent<WaterMeter>());
            foreach (string name in new[] { "sandDrainMultiplier", "grassDrainMultiplier", "blackStoneDrainMultiplier" })
                Check(settings.FindProperty(name).floatValue == 1f, name + " is saved as neutral 1x");
            var contact = new SerializedObject(prefab.GetComponent<PhysicsControl>());
            Check(contact.FindProperty("extraDryGroundLayer").intValue == LayerMask.GetMask("Ground-Extra-Dry"),
                "Prefab recognizes extra dry ground without scene overrides");
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            EditorApplication.playModeStateChanged += EnteredPlay;
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Finish(error); }
    }

    private static async void EnteredPlay(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        EditorApplication.playModeStateChanged -= EnteredPlay;
        try
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>();
            await DefaultsAndOrder();
            await IndependentMaterials();
            await AirAndSupport();
            await WallAndSlope();
            await HeatAndLifecycle();
            await TimingRefillAndRespawn();
            Finish(null);
        }
        catch (Exception error) { Finish(error); }
    }

    private static async Task DefaultsAndOrder()
    {
        var idle = new float[3];
        var moving = new float[3];
        for (int i = 0; i < layers.Length; i++)
        {
            Setup(layers[i]);
            await Delay(0.2f);
            Check(physics.grounded && (i != 2 || physics.extraDryGrounded), layers[i] + " supports the real prefab");
            int changes = 0;
            water.WaterChanged += value => changes++;
            idle[i] = await Rate();
            Near(idle[i], idleRates[i], layers[i] + " idle");
            Check(changes > 0 && physics.rb.linearVelocity.magnitude < 0.01f, "Stationary loss updates the existing water event");
            Keys(Key.D);
            await Delay(0.15f);
            moving[i] = await Rate();
            Near(moving[i], moveRates[i], layers[i] + " actual walking");
            Check(physics.rb.linearVelocity.x > 1f, "Walking sample measures real movement");
            await Clear();
        }
        Check(idle[0] > 0f && idle[0] < idle[1] && idle[1] < moving[0] && moving[0] < idle[2] &&
            idle[2] < moving[1] && moving[1] < moving[2], "All six measured rates follow the requested strict order");
    }

    private static async Task IndependentMaterials()
    {
        var materials = new[] { GroundSurface.Material.Sand, GroundSurface.Material.Grass, GroundSurface.Material.BlackStone };
        var multipliers = new[] { 1.25f, 0.8f, 1.5f };
        foreach (int i in new[] { 0, 1, 2 })
        {
            Setup(layers[i]);
            Set(water, "sandDrainMultiplier", multipliers[0]);
            Set(water, "grassDrainMultiplier", multipliers[1]);
            Set(water, "blackStoneDrainMultiplier", multipliers[2]);
            await Delay(0.2f);
            for (int m = 0; m < materials.Length; m++)
            {
                Set(surface, "surfaceMaterial", materials[m]);
                await Delay(0.08f);
                Check(physics.GroundMaterial == materials[m] && ground.layer == LayerMask.NameToLayer(layers[i]),
                    materials[m] + " is independent of " + layers[i]);
                Near(await Rate(), idleRates[i] * multipliers[m], layers[i] + "/" + materials[m] + " idle material factor");
                Keys(Key.D);
                await Delay(0.15f);
                Near(await Rate(), moveRates[i] * multipliers[m], layers[i] + "/" + materials[m] + " walking material factor");
                Keys();
                await Delay(0.15f);
            }
            surface.enabled = false;
            await Delay(0.08f);
            Near(await Rate(), idleRates[i], "Disabled material marker uses neutral fallback");
            Object.DestroyImmediate(surface);
            await Delay(0.08f);
            Near(await Rate(), idleRates[i], "Missing material marker uses neutral fallback");
            await Clear();
        }
    }

    private static async Task AirAndSupport()
    {
        Setup("Ground-Wet");
        Set(surface, "surfaceMaterial", GroundSurface.Material.Grass);
        Set(water, "grassDrainMultiplier", 0.5f);
        await Delay(0.2f);
        Keys(Key.Space);
        await Delay(0.12f);
        Check(!physics.grounded && physics.GroundMaterial == GroundSurface.Material.Unspecified && physics.rb.linearVelocity.y > 0f,
            "Actual vertical jump clears the previous ground material");
        Near(await Rate(), 3f, "Vertical jump without horizontal input uses normal air rate");
        Keys();
        Teleport(new Vector2(0f, 4f));
        await Delay(0.06f);
        Near(await Rate(), 3f, "Falling does not inherit wet/material discounts");
        var parent = new GameObject("Material parent"); objects.Add(parent);
        var marker = parent.AddComponent<GroundSurface>();
        Set(marker, "surfaceMaterial", GroundSurface.Material.BlackStone);
        Object.DestroyImmediate(surface);
        ground.transform.SetParent(parent.transform, true);
        Teleport(Vector2.zero);
        await Delay(0.2f);
        Check(physics.grounded && physics.GroundMaterial == GroundSurface.Material.BlackStone, "Collider can read a material marker from its parent");
        var distant = new GameObject("Lower sand support"); objects.Add(distant);
        distant.layer = LayerMask.NameToLayer("Ground-Dry");
        distant.transform.position = new Vector2(0f, -1.2f);
        distant.AddComponent<BoxCollider2D>().size = new Vector2(1000f, 1f);
        Set(distant.AddComponent<GroundSurface>(), "surfaceMaterial", GroundSurface.Material.Sand);
        await Delay(0.1f);
        Check(physics.wetGrounded && physics.GroundMaterial == GroundSurface.Material.BlackStone,
            "Material follows the actual supporting collider rather than the lower overlapping ground");
        physics.ResetMotion();
        Check(physics.GroundMaterial == GroundSurface.Material.Unspecified, "Motion reset clears stale material");
        await Clear();
    }

    private static async Task WallAndSlope()
    {
        Setup("Ground-Dry");
        var wall = new GameObject("Wall"); objects.Add(wall);
        wall.layer = ground.layer;
        wall.transform.position = new Vector2(1f, 1f);
        wall.AddComponent<BoxCollider2D>().size = new Vector2(1f, 4f);
        Keys(Key.D);
        await Delay(0.4f);
        Near(await Rate(), 0.5f, "Pressing into a wall uses idle loss rather than walking loss");
        await Clear();
        foreach (float angle in new[] { -30f, 30f })
        {
            Setup("Ground-Dry");
            ground.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            ground.transform.position = Vector3.down;
            Teleport(new Vector2(0f, 0.1f));
            await Delay(0.5f);
            Check(physics.grounded, "Slope support at " + angle);
            Near(await Rate(), 0.5f, "Idle slope drift stays below the walking threshold at " + angle);
            await Clear();
        }
    }

    private static async Task HeatAndLifecycle()
    {
        Setup("Ground-Extra-Dry");
        Set(surface, "surfaceMaterial", GroundSurface.Material.Sand);
        Set(water, "sandDrainMultiplier", 1.25f);
        var heatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/SlimeFire_Stationary.prefab");
        // Keep its body away so this fixture measures heat without contact knockback.
        var source = Object.Instantiate(heatPrefab, new Vector3(10f, 0f, 0f), Quaternion.identity); objects.Add(source);
        var heat = source.GetComponentInChildren<EnemyHeatArea>();
        heat.GetComponent<CircleCollider2D>().radius = 100f;
        await Delay(0.2f);
        Check(water.DrainMultiplier == 2f, "Real Slime heat trigger still registers");
        Near(await Rate(), 5f, "Idle rate combines dryness, material and heat");
        Keys(Key.D);
        await Delay(0.15f);
        Near(await Rate(), 15f, "Walking rate combines dryness, material and heat");
        Keys();
        await Delay(0.15f);
        var stronger = new GameObject("Stronger heat source"); objects.Add(stronger);
        water.SetDrainMultiplier(stronger, 3f);
        Near(await Rate(), 7.5f, "Overlapping heat keeps the strongest multiplier");
        water.RemoveDrainMultiplier(stronger);
        heat.enabled = false;
        await Delay(0.08f);
        Near(await Rate(), 2.5f, "Removing heat restores positive ordinary idle loss");
        player.enabled = false;
        Near(await Rate(), 0f, "Disabled controls stop ordinary idle loss");
        player.enabled = true;
        await Delay(0.08f);
        physics.rb.simulated = false;
        Near(await Rate(), 0f, "Unsimulated body stops ordinary idle loss");
        physics.rb.simulated = true;
        await Delay(0.08f);
        water.enabled = false;
        Near(await Rate(), 0f, "Disabled WaterMeter stops loss");
        await Clear();
    }

    private static async Task TimingRefillAndRespawn()
    {
        Setup("Ground-Dry");
        await Delay(0.2f);
        float original = Time.fixedDeltaTime;
        foreach (float step in new[] { 0.01f, 0.04f })
        {
            Time.fixedDeltaTime = step;
            await Delay(0.1f);
            Near(await Rate(), 0.5f, "Idle loss is time based at fixed step " + step);
        }
        Time.fixedDeltaTime = original;
        var source = new GameObject("Water source"); objects.Add(source);
        source.layer = LayerMask.NameToLayer("Water");
        source.AddComponent<BoxCollider2D>().isTrigger = true;
        int healed = 0;
        water.RefilledFromSource += () => healed++;
        water.Consume(20f);
        await Delay(0.15f);
        Check(water.CurrentWater > 99.9f && healed == 1, "Water source refills and heals once despite positive idle loss");
        await Delay(0.2f);
        Check(water.CurrentWater > 99.9f && healed == 1, "Staying in water keeps refilling without repeated heal events");
        source.SetActive(false);
        water.SetCheckpoint(new Vector2(20f, 0f));
        int deaths = 0;
        water.Depleted += () => deaths++;
        water.Consume(water.CurrentWater - 0.005f);
        await Delay(0.08f);
        Check(deaths == 1 && water.IsEmpty && water.IsRespawning, "Ordinary idle alone starts exactly one depletion/respawn");
        Near(await Rate(0.06f), 0f, "Death animation does not continue consuming water");
        await Delay(1f);
        Check(deaths == 1 && !water.IsRespawning && player.enabled && water.CurrentWater > 99f &&
            Mathf.Abs(physics.rb.position.x - 20f) < 0.1f, "Respawn restores water, control and checkpoint after idle depletion");
        await Clear();
    }

    private static void Setup(string layer)
    {
        ground = new GameObject("Ground"); objects.Add(ground);
        ground.layer = LayerMask.NameToLayer(layer);
        ground.transform.position = new Vector2(0f, -1f);
        ground.AddComponent<BoxCollider2D>().size = new Vector2(1000f, 1f);
        surface = ground.AddComponent<GroundSurface>();
        var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab"), Vector3.zero, Quaternion.identity);
        objects.Add(root);
        player = root.GetComponent<Player>(); physics = root.GetComponent<PhysicsControl>(); water = root.GetComponent<WaterMeter>();
        root.transform.Find("TrailAnchor").GetComponent<TrailRenderer>().enabled = false;
        Physics2D.SyncTransforms();
    }

    private static async Task<float> Rate(float seconds = 0.32f)
    {
        float before = water.CurrentWater, start = Time.fixedTime;
        await Delay(seconds);
        return (before - water.CurrentWater) / (Time.fixedTime - start);
    }

    private static void Teleport(Vector2 position)
    {
        physics.rb.position = position; player.transform.position = position;
        physics.ResetMotion(); water.ResetPositionTracking(); Physics2D.SyncTransforms();
    }

    private static void Set(object obj, string name, object value) => obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);
    private static void Keys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); InputSystem.Update(); }

    private static async Task Clear()
    {
        Keys();
        for (int i = objects.Count - 1; i >= 0; i--) if (objects[i] != null) Object.DestroyImmediate(objects[i]);
        objects.Clear();
        await Delay(0.06f);
    }

    private static void Near(float actual, float expected, string description) =>
        Check(Mathf.Abs(actual - expected) < 0.04f, description + ": measured=" + actual + ", expected=" + expected);

    private static void Check(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
        passed.Add(description);
        File.WriteAllLines("water-drain-checks.txt", passed);
    }

    private static async Task Delay(float seconds)
    {
        float end = Time.time + seconds;
        double timeout = EditorApplication.timeSinceStartup + 30;
        while (Time.time < end)
        {
            if (EditorApplication.timeSinceStartup > timeout) throw new TimeoutException("Play Mode did not advance");
            var completion = new TaskCompletionSource<bool>();
            EditorApplication.CallbackFunction callback = null;
            callback = () => { EditorApplication.update -= callback; completion.SetResult(true); };
            EditorApplication.update += callback;
            await completion.Task;
        }
    }

    private static void Finish(Exception error)
    {
        string result = (error == null ? "PASS" : "FAIL") + ": Unity " + Application.unityVersion + "; " + passed.Count +
            " checks using current copied Player prefab and scripts in an isolated project. Main scene gameplay/build not tested.";
        if (error != null) { result += "\n" + error; Debug.LogException(error); }
        File.WriteAllText("water-drain-result.txt", result);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}
