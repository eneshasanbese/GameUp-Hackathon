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

public static class SoundSlotsVerification
{
    private static readonly List<string> passed = new List<string>();
    private static readonly List<GameObject> objects = new List<GameObject>();
    private static readonly List<string> errors = new List<string>();
    private static readonly List<string> editorIssues = new List<string>();
    private static AudioClip tone;
    private static AudioSource Source => (AudioSource)typeof(Sfx).GetField("source", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use a separate batch project.");
        try
        {
            CheckAsset<PlayerSounds>("Player.prefab", "hurt", "refill", "death", "land", "lowWaterWarning");
            CheckAsset<EmberSpitter>("Enemies/EmberSpitter.prefab", "windupSound", "fireSound");
            CheckAsset<EmberFireball>("Enemies/EmberFireball.prefab", "impactSound");
            CheckAsset<FallingGroundTrap>("Traps/FallingGround_Sand.prefab", "warningSound", "fallSound");
            CheckAsset<FallingGroundTrap>("Traps/FallingGround_Rock.prefab", "warningSound", "fallSound");
            CheckAsset<FallingStoneTrap>("Traps/Trap_stoneSpike.prefab", "fallSound", "hitSound");
            CheckAsset<SoundOnEnable>("Enemies/HapsirikKoz_Smoke.prefab", "clip");
            foreach (string path in new[] { "WaterSource.prefab", "Enemies/SlimeFire_Stationary.prefab", "Enemies/SlimeFire_Patrol.prefab" })
            {
                AudioSource source = PrefabUtility.LoadPrefabContents("Assets/Prefabs/" + path).GetComponent<AudioSource>();
                Check(source != null && source.clip == null && source.playOnAwake && source.loop && source.spatialBlend == 1f,
                    path + ": ambient source ready for only an audio clip");
                PrefabUtility.UnloadPrefabContents(source.gameObject);
            }
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.playModeStateChanged += OnPlay;
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Finish(error); }
    }

    private static void CheckAsset<T>(string path, params string[] fields) where T : Component
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + path);
        Check(prefab != null && prefab.GetComponentsInChildren<Component>(true).All(c => c != null), path + ": no missing components");
        T component = prefab.GetComponent<T>();
        Check(component != null, path + ": sound component attached");
        SerializedObject settings = new SerializedObject(component);
        Check(fields.All(name => settings.FindProperty(name) != null && settings.FindProperty(name).objectReferenceValue == null),
            path + ": all requested audio slots are available and unassigned");
    }

    private static async void OnPlay(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        try
        {
            Application.logMessageReceived += Log;
            var camera = new GameObject("Audio Test Camera");
            camera.tag = "MainCamera";
            camera.AddComponent<Camera>();
            camera.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0, 0, -5);
            AudioListener.volume = 0f; // Silent verification, while the audio engine still advances sources.
            tone = AudioClip.Create("Verification tone", 44100 * 4, 1, 44100, false);
            tone.SetData(Enumerable.Range(0, 44100 * 4).Select(i => 0.1f * Mathf.Sin(i * 440f * 2f * Mathf.PI / 44100f)).ToArray(), 0);
            await Expect(() => Sfx.Play(tone), true, "native one-shot playback works");

            Player player = await PlayerAt(Vector2.zero);
            PlayerSounds sounds = player.GetComponent<PlayerSounds>();
            WaterMeter water = player.GetComponent<WaterMeter>();
            KnockbackAbility knockback = player.GetComponent<KnockbackAbility>();
            await Expect(() => knockback.TryWaterHit(1f), false, "unassigned hurt clip safely stays silent");
            Set(knockback, "flashRemaining", 0f);
            Set(sounds, "hurt", tone);
            await Expect(() => Check(knockback.TryWaterHit(5f), "projectile damage is accepted"), true, "projectile damage plays hurt");
            await Expect(() => Check(!knockback.TryWaterHit(5f), "flash rejects another projectile"), false, "immune hit stays silent");
            Set(knockback, "flashRemaining", 0f);
            await Expect(() => Check(knockback.TryKnockback(Vector2.left), "body damage is accepted"), true, "body/trap damage plays hurt");
            sounds.enabled = false;
            Set(knockback, "flashRemaining", 0f);
            await Expect(() => water.Refill(), false, "disabled PlayerSounds unsubscribes safely");
            await Clean();

            player = await PlayerAt(Vector2.zero);
            Set(player.GetComponent<PlayerSounds>(), "hurt", tone);
            await Expect(() => player.GetComponent<KnockbackAbility>().TryWaterHit(100f), false,
                "lethal damage suppresses the hurt voice when death is the selected cue");
            await Clean();

            player = await PlayerAt(Vector2.zero);
            water = player.GetComponent<WaterMeter>();
            Set(player.GetComponent<PlayerSounds>(), "refill", tone);
            await Expect(() => Spawn("WaterSource.prefab", Vector2.zero), false, "full-water source entry stays silent");
            await Clean();

            player = await PlayerAt(Vector2.zero);
            Set(player.GetComponent<PlayerSounds>(), "death", tone);
            await Expect(() => player.GetComponent<WaterMeter>().RespawnAtCheckpoint(), true,
                "fall death can play while the water meter is still positive");
            await Clean();

            player = await PlayerAt(Vector2.zero);
            sounds = player.GetComponent<PlayerSounds>();
            water = player.GetComponent<WaterMeter>();
            Set(sounds, "refill", tone);
            water.Consume(10f);
            await Expect(() => Spawn("WaterSource.prefab", Vector2.zero), true, "entering a real water source plays refill");
            Check(water.NormalizedWater == 1f, "source still refills the single water meter");
            await Expect(() => { }, false, "staying in the source does not repeat refill");
            await Clean();

            player = await PlayerAt(Vector2.zero);
            sounds = player.GetComponent<PlayerSounds>();
            water = player.GetComponent<WaterMeter>();
            Set(sounds, "lowWaterWarning", tone);
            await Expect(() => water.Consume(76f), true, "low water threshold plays warning");
            await Expect(() => water.Consume(1f), false, "continued drain does not repeat low-water warning");
            water.Refill();
            await Expect(() => water.Consume(76f), true, "refill rearms the low-water warning");
            Set(sounds, "lowWaterThreshold", 0.9f);
            water.Refill();
            await Expect(() => water.Consume(11f), true, "a 90 percent warning threshold also arms correctly");
            water.Refill();
            await Expect(() => water.Consume(11f), true, "full refill rearms the maximum configurable threshold");
            Set(sounds, "death", tone);
            await Expect(() => water.Consume(100f), true, "depletion starts death sound synchronously");
            await Expect(() => water.RespawnAtCheckpoint(), false, "repeated death request stays silent");
            await Clean();

            await Spitter();
            await Projectile();
            await Landing();
            await Ground();
            await Stone();
            await Final();
            Check(errors.Count == 0, "no gameplay errors during the sound scenarios");
            Finish(null);
        }
        catch (Exception error) { Finish(error); }
    }

    private static async Task Spitter()
    {
        await PlayerAt(new Vector2(3f, 0f));
        GameObject enemyObject = Spawn("Enemies/EmberSpitter.prefab", Vector2.zero);
        enemyObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        EmberSpitter enemy = enemyObject.GetComponent<EmberSpitter>();
        Set(enemy, "windupDuration", 10f);
        Set(enemy, "windupSound", tone);
        await Expect(() => { }, true, "spitter windup has its own cue");
        Check(enemy.CurrentState == EmberSpitter.ShotState.Windup, "spitter remains in windup");
        await Expect(() => { }, false, "windup cue does not repeat each physics step");
        Set(enemy, "windupSound", null);
        Set(enemy, "fireSound", tone);
        await Expect(() => Set(enemy, "timer", 0f), true, "real shot plays fire cue");
        Check(Object.FindObjectsByType<EmberFireball>(FindObjectsSortMode.None).Length == 1, "shot still creates one projectile");
        await Clean();
    }

    private static async Task Projectile()
    {
        Player player = await PlayerAt(new Vector2(3f, 0f));
        var owner = new GameObject("Test projectile owner");
        objects.Add(owner);
        var wall = new GameObject("Test wall");
        objects.Add(wall);
        wall.layer = LayerMask.NameToLayer("Ground-Dry");
        wall.transform.position = Vector2.right;
        wall.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 3f);
        EmberFireball ball = Spawn("Enemies/EmberFireball.prefab", Vector2.zero).GetComponent<EmberFireball>();
        Set(ball, "impactSound", tone);
        StopSound();
        ball.Launch(player.transform.position, player, owner.transform, LayerMask.GetMask("Player"), LayerMask.GetMask("Ground-Dry"));
        await Task.Delay(400);
        Check(ball == null && Source != null && Source.isPlaying, "projectile wall impact plays before destruction");
        await Clean();
    }

    private static async Task Ground()
    {
        GameObject platform = Spawn("Traps/FallingGround_Sand.prefab", Vector2.zero);
        FallingGroundTrap trap = platform.GetComponent<FallingGroundTrap>();
        Set(trap, "fallDelay", 0.3f);
        Set(trap, "warningSound", tone);
        Player player = await PlayerAt(new Vector2(0f, 1.3f));
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        body.gravityScale = 1f;
        StopSound();
        await Until(() => Get<bool>(trap, "triggered"));
        Check(Source != null && Source.isPlaying, "first platform contact plays warning");
        Set(trap, "fallSound", tone);
        StopSound();
        await Until(() => Get<bool>(trap, "falling"));
        Check(Source != null && Source.isPlaying, "platform release plays fall cue");
        Check(!platform.GetComponent<BoxCollider2D>().enabled, "fall still releases the supporting collider");
        await Expect(() => { }, false, "falling platform does not repeat the start cue");
        await Clean();
    }

    private static async Task Landing()
    {
        var floor = new GameObject("Landing floor");
        objects.Add(floor);
        floor.layer = LayerMask.NameToLayer("Ground-Dry");
        floor.transform.position = new Vector2(0f, -1f);
        floor.AddComponent<BoxCollider2D>().size = new Vector2(8f, 1f);
        Player player = await PlayerAt(new Vector2(0f, 2f));
        Set(player.GetComponent<PlayerSounds>(), "land", tone);
        var body = player.GetComponent<Rigidbody2D>();
        body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        body.gravityScale = 1f;
        StopSound();
        await Until(() => player.IsGrounded);
        Check(Source != null && Source.isPlaying, "a real fall and landing plays the landing cue");
        await Expect(() => { }, false, "standing on the ground does not repeat landing");
        await Clean();
    }

    private static async Task Stone()
    {
        await PlayerAt(new Vector2(0f, -2f));
        FallingStoneTrap stone = Spawn("Traps/Trap_stoneSpike.prefab", Vector2.zero).GetComponent<FallingStoneTrap>();
        Set(stone, "fallSound", tone);
        Set(stone, "hitSound", tone);
        StopSound();
        await Until(() => Get<bool>(stone, "falling"));
        Check(Source != null && Source.isPlaying, "stalactite detection plays release cue");
        StopSound();
        await Until(() => Get<bool>(stone, "hitPlayer"));
        Check(Source != null && Source.isPlaying, "stalactite player contact plays hit cue");
        await Expect(() => { }, false, "stalactite contact is latched once");
        await Clean();
    }

    private static async Task Final()
    {
        Player player = await PlayerAt(Vector2.zero);
        var root = new GameObject("Test final sapling");
        root.SetActive(false);
        objects.Add(root);
        var renderer = root.AddComponent<SpriteRenderer>();
        var texture = new Texture2D(2, 2);
        var sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.one * 0.5f, 2f);
        renderer.sprite = sprite;
        root.AddComponent<BoxCollider2D>().isTrigger = true;
        var final = root.AddComponent<FinalSapling>();
        Set(final, "greenSprite", sprite);
        foreach (string field in new[] { "absorbSound", "bloomSound", "victorySound" }) Set(final, field, tone);
        foreach (string field in new[] { "uiFadeDuration", "zoomDuration", "shrinkDuration", "bloomDuration" }) Set(final, field, 0.2f);
        Set(final, "holdDuration", 0.1f);
        StopSound();
        root.SetActive(true);
        await Until(() => Get<bool>(final, "started"));
        Check(Source != null && Source.isPlaying, "valid final contact plays absorb cue");
        StopSound();
        await Task.Delay(260);
        Check(Source != null && Source.isPlaying, "bloom phase plays its cue");
        StopSound();
        await Task.Delay(350);
        Check(Source != null && Source.isPlaying, "cutscene completion plays victory cue");
        Check(!player.enabled && !player.GetComponent<WaterMeter>().enabled, "final still stops gameplay and water drain");
        await Expect(() => { }, false, "final cues do not repeat after completion");
        await Clean();
    }

    private static GameObject Spawn(string path, Vector2 position)
    {
        var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + path), position, Quaternion.identity);
        objects.Add(instance);
        return instance;
    }

    private static async Task<Player> PlayerAt(Vector2 position)
    {
        GameObject root = Spawn("Player.prefab", position);
        var water = root.GetComponent<WaterMeter>();
        foreach (string field in new[] { "dryDrainPerSecond", "wetDrainPerSecond", "extraDryDrainPerSecond", "dryIdleDrainPerSecond", "wetIdleDrainPerSecond", "extraDryIdleDrainPerSecond" }) Set(water, field, 0f);
        Set(water, "fallLimit", -10000f);
        root.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        root.GetComponent<Rigidbody2D>().gravityScale = 0f;
        foreach (SoundOnAnimationEvent receiver in root.GetComponentsInChildren<SoundOnAnimationEvent>()) receiver.enabled = false;
        await Task.Delay(80);
        return root.GetComponent<Player>();
    }

    private static async Task Expect(Action action, bool audible, string label)
    {
        StopSound();
        action();
        await Task.Delay(90);
        Check((Source != null && Source.isPlaying) == audible, label);
    }

    private static void StopSound() { if (Source != null) Source.Stop(); }

    private static async Task Clean()
    {
        foreach (GameObject instance in objects) if (instance != null) Object.Destroy(instance);
        objects.Clear();
        await Task.Delay(100);
        StopSound();
    }

    private static async Task Until(Func<bool> condition)
    {
        for (int i = 0; i < 80; i++) { if (condition()) { await Task.Delay(30); return; } await Task.Delay(25); }
        throw new Exception("Timed out waiting for a gameplay event");
    }

    private static void Set(object target, string name, object value) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    private static T Get<T>(object target, string name) => (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    private static void Check(bool condition, string label) { if (!condition) throw new Exception(label); passed.Add(label); }
    private static void Log(string message, string stack, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception) return;
        if (message.StartsWith("ArgumentOutOfRangeException") && stack.Contains("UnityEditor.Search."))
            editorIssues.Add(message + "\n" + stack);
        else
            errors.Add(message + "\n" + stack);
    }

    private static void Finish(Exception error)
    {
        var report = passed.Select(label => "PASS " + label).ToList();
        report.AddRange(errors.Select(message => "LOG_ERROR " + message));
        report.AddRange(editorIssues.Select(message => "EDITOR_INDEXER_ISSUE " + message));
        report.Add(error == null ? "RESULT PASS " + passed.Count : "RESULT FAIL " + error);
        File.WriteAllLines("sound-slots-result.txt", report);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}
