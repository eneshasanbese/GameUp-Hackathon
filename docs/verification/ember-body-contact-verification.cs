using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class EmberBodyContactVerification
{
    private static readonly List<string> passed = new List<string>();
    private static readonly List<string> errors = new List<string>();
    private static readonly List<string> editorIssues = new List<string>();
    private static readonly List<GameObject> objects = new List<GameObject>();
    private static readonly string[] Enemies = { "EmberSpitter", "KozOcagi_Stationary" };

    public static void Run()
    {
        if (!Application.isBatchMode) throw new Exception("Use an isolated verification project.");
        try
        {
            foreach (string name in Enemies)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/" + name + ".prefab");
                Check(prefab != null && prefab.GetComponentsInChildren<Component>(true).All(c => c != null), name + ": prefab imports without missing scripts");
                BoxCollider2D box = prefab.GetComponent<BoxCollider2D>();
                Check(box != null && box.isTrigger && prefab.GetComponent<EnemyContactKnockback>() != null, name + ": body sensor uses the existing contact component");
                Check(box.includeLayers.value == LayerMask.GetMask("Player") && box.excludeLayers.value == ~LayerMask.GetMask("Player"), name + ": contact filter includes only Player");
            }
            var spitter = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/EmberSpitter.prefab");
            Check(spitter.transform.Find("TerrainBody").GetComponent<BoxCollider2D>().isTrigger == false && spitter.GetComponent<Rigidbody2D>().gravityScale == 1f, "Ember gravity and terrain support are preserved");
            var furnace = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/KozOcagi_Stationary.prefab");
            Check(furnace.GetComponent<Rigidbody2D>() == null && furnace.transform.Find("EffectArea").GetComponent<CircleCollider2D>().radius == 6f, "furnace remains stationary and keeps its separate six-unit effect area");
            Check(furnace.GetComponent<AudioSource>().clip != null && furnace.GetComponent<AudioSource>().loop && Get<AudioClip>(spitter.GetComponent<EmberSpitter>(), "fireSound") != null, "chosen breathing and shot sounds remain assigned");
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.playModeStateChanged += OnPlay;
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Finish(error); }
    }

    private static async void OnPlay(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        try
        {
            Application.logMessageReceived += Log;
            Application.runInBackground = true;
            AudioListener.volume = 0f;
            var camera = new GameObject("Test camera");
            camera.tag = "MainCamera";
            camera.AddComponent<Camera>();
            camera.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0, 0, -5);
            foreach (string name in Enemies)
            {
                await Contact(name, -1, true);
                await Contact(name, 1, false);
                await RejectedAndLethalContacts(name);
            }
            await Suction();
            await Shot();
            Check(errors.Count == 0, "no gameplay exceptions during body-contact verification");
            Finish(null);
        }
        catch (Exception error) { Finish(error); }
    }

    private static async Task Contact(string name, int side, bool prolonged)
    {
        var enemy = Enemy(name);
        var player = await PlayerAt(new Vector2(side * 3f, 0.6f));
        var water = player.GetComponent<WaterMeter>();
        var knockback = player.GetComponent<KnockbackAbility>();
        var body = player.GetComponent<Rigidbody2D>();
        int hits = 0;
        knockback.HitAccepted += () => hits++;
        Move(player, new Vector2(side * 0.65f, 0.6f));
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        await Until(() => hits == 1);
        await Task.Delay(35);
        string label = name + (side < 0 ? " left" : " right");
        Check(Mathf.Approximately(water.CurrentWater, 90f), label + ": real body contact consumes exactly ten water points");
        Check(player.CurrentState == PlayerStates.State.Knockback && knockback.IsFlashing, label + ": enters existing Knockback and flash");
        Check(body.linearVelocity.x * side > 0f && body.linearVelocity.y > 0f, label + ": impulse moves away from the enemy and upward");
        Check(!knockback.TryWaterHit(7f) && water.CurrentWater == 90f, label + ": projectile and body hits share flash immunity");
        if (prolonged)
        {
            body.constraints = RigidbodyConstraints2D.FreezeAll;
            Move(player, new Vector2(side * 0.4f, 0.6f));
            await Task.Delay(700);
            Check(hits == 1 && water.CurrentWater == 90f, label + ": continuous overlap does not repeat damage during flash");
            Check(player.CurrentState != PlayerStates.State.Knockback, label + ": ability recovers instead of looping during flash");
            await Until(() => hits == 2, 3200);
            Check(water.CurrentWater == 80f && knockback.IsFlashing, label + ": staying in contact can hit again only after immunity expires");
            enemy.GetComponent<EnemyContactKnockback>().enabled = false;
            Set(knockback, "flashRemaining", 0f);
            await Task.Delay(650);
            Check(hits == 2 && water.CurrentWater == 80f, label + ": disabling the body-contact component stops damage");
        }
        await Clean();
    }

    private static async Task RejectedAndLethalContacts(string name)
    {
        var enemy = Enemy(name);
        var player = await PlayerAt(new Vector2(3f, 0.6f));
        var water = player.GetComponent<WaterMeter>();
        player.enabled = false;
        Move(player, new Vector2(0.4f, 0.6f));
        await Task.Delay(100);
        Check(water.CurrentWater == 100f, name + ": disabled Player cannot take a body hit");
        Move(player, new Vector2(3f, 0.6f));
        player.enabled = true;
        await Task.Delay(80);
        enemy.GetComponent<BoxCollider2D>().enabled = false;
        Move(player, new Vector2(0.4f, 0.6f));
        await Task.Delay(100);
        Check(water.CurrentWater == 100f, name + ": disabled body collider cannot hit");
        Move(player, new Vector2(3f, 0.6f));
        await Task.Delay(80);
        enemy.GetComponent<BoxCollider2D>().enabled = true;
        var sensor = new GameObject("Player trigger sensor");
        sensor.transform.SetParent(player.transform);
        sensor.transform.position = new Vector2(0f, 0.6f);
        sensor.layer = LayerMask.NameToLayer("Player");
        sensor.AddComponent<CircleCollider2D>().isTrigger = true;
        await Task.Delay(100);
        Check(water.CurrentWater == 100f, name + ": player trigger sensors cannot take body hits");
        Object.Destroy(sensor);
        water.Consume(95f);
        Move(player, new Vector2(0.3f, 0.6f));
        await Until(() => water.IsRespawning);
        Check(water.IsEmpty && player.CurrentState == PlayerStates.State.Death, name + ": lethal contact enters existing death flow");
        await Clean();
    }

    private static async Task Suction()
    {
        var enemy = Enemy("KozOcagi_Stationary", false);
        var player = await PlayerAt(new Vector2(2f, 0.6f));
        var water = player.GetComponent<WaterMeter>();
        var knockback = player.GetComponent<KnockbackAbility>();
        int hits = 0;
        knockback.HitAccepted += () => hits++;
        await Task.Delay(250);
        Check(hits == 0 && !knockback.IsFlashing && water.CurrentWater < 100f, "furnace outer effect area drains water without body knockback");
        Check(Get<Dictionary<Object, float>>(player.physicsControl, "externalHorizontalSpeeds").Count == 1, "furnace still publishes its horizontal suction contribution");
        Move(player, new Vector2(0.4f, 0.6f));
        await Until(() => hits == 1);
        float afterHit = water.CurrentWater;
        await Task.Delay(200);
        Check(hits == 1 && knockback.IsFlashing && water.CurrentWater < afterHit, "furnace continuous water drain continues during body-hit immunity");
        await Clean();
    }

    private static async Task Shot()
    {
        var enemy = Enemy("EmberSpitter", false);
        var player = await PlayerAt(new Vector2(3f, 0.6f));
        await Until(() => Object.FindObjectsByType<EmberFireball>(FindObjectsSortMode.None).Length == 1, 2500);
        Check(enemy.GetComponent<EmberSpitter>().CurrentState == EmberSpitter.ShotState.Cooldown, "Ember still prepares and launches projectiles with body contact enabled");
        Check(!player.GetComponent<KnockbackAbility>().IsFlashing, "Ember detection range alone does not cause body knockback");
        await Clean();
    }

    private static GameObject Enemy(string name, bool isolateContact = true)
    {
        var root = Spawn("Enemies/" + name + ".prefab", Vector2.zero);
        if (root.TryGetComponent(out Rigidbody2D body))
        {
            body.constraints = RigidbodyConstraints2D.FreezeAll;
            body.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }
        if (isolateContact)
        {
            var spitter = root.GetComponent<EmberSpitter>();
            if (spitter != null) spitter.enabled = false;
            foreach (var suction in root.GetComponentsInChildren<EmberSuctionEnemy>()) suction.enabled = false;
        }
        return root;
    }

    private static async Task<Player> PlayerAt(Vector2 position)
    {
        var root = Spawn("Player.prefab", position);
        var water = root.GetComponent<WaterMeter>();
        foreach (string field in new[] { "dryDrainPerSecond", "wetDrainPerSecond", "extraDryDrainPerSecond", "dryIdleDrainPerSecond", "wetIdleDrainPerSecond", "extraDryIdleDrainPerSecond" }) Set(water, field, 0f);
        Set(water, "fallLimit", -10000f);
        root.GetComponent<Rigidbody2D>().gravityScale = 0f;
        root.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        root.GetComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.NeverSleep;
        foreach (var receiver in root.GetComponentsInChildren<SoundOnAnimationEvent>()) receiver.enabled = false;
        await Task.Delay(100);
        return root.GetComponent<Player>();
    }

    private static GameObject Spawn(string path, Vector2 position) { var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + path), position, Quaternion.identity); objects.Add(root); return root; }
    private static void Move(Player player, Vector2 position) { player.GetComponent<Rigidbody2D>().position = position; Physics2D.SyncTransforms(); }
    private static async Task Clean() { foreach (var root in objects) if (root != null) Object.Destroy(root); objects.Clear(); await Task.Delay(100); }
    private static async Task Until(Func<bool> condition, int timeout = 1600) { for (int i = 0; i < timeout / 10; i++) { if (condition()) return; await Task.Delay(10); } throw new Exception("Timed out waiting for contact/shot event"); }
    private static void Set(object target, string name, object value) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    private static T Get<T>(object target, string name) => (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    private static void Check(bool condition, string label) { if (!condition) throw new Exception(label); passed.Add(label); }
    private static void Log(string message, string stack, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception) return;
        if (message.StartsWith("ArgumentOutOfRangeException") && stack.Contains("UnityEditor.Search.")) editorIssues.Add(message + "\n" + stack);
        else errors.Add(message + "\n" + stack);
    }
    private static void Finish(Exception error)
    {
        var result = passed.Select(line => "PASS " + line).ToList();
        result.AddRange(errors.Select(line => "LOG_ERROR " + line));
        result.AddRange(editorIssues.Select(line => "EDITOR_INDEXER_ISSUE " + line));
        result.Add(error == null ? "RESULT PASS " + passed.Count : "RESULT FAIL " + error);
        File.WriteAllLines("ember-body-contact-result.txt", result);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}
