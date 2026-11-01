using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class WaterEmberSoundVerification
{
    private static readonly List<string> passed = new List<string>();
    private static readonly List<string> errors = new List<string>();
    private static readonly List<string> indexerIssues = new List<string>();
    private static readonly List<GameObject> objects = new List<GameObject>();
    private static AudioSource Source => (AudioSource)typeof(Sfx).GetField("source", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
    private static AudioClip tone;

    public static void Run()
    {
        if (!Application.isBatchMode) throw new Exception("Use an isolated batch project.");
        try
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WaterSource.prefab");
            Check(source.GetComponentsInChildren<Component>(true).All(c => c != null), "WaterSource has no missing scripts");
            Check(source.GetComponent<WaterSourceSound>() != null, "contact sound component is attached to WaterSource");
            AudioSource audio = source.GetComponent<AudioSource>();
            Check(audio.clip == AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/platformerPack/Sounds/freesound_gamestudio-find-408438.mp3"), "WaterSource uses the user's Level_1 clip");
            Check(!audio.playOnAwake && !audio.loop && audio.volume == 0.8f, "source is configured for a single contact cue");
            var ember = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/EmberSpitter.prefab").GetComponent<EmberSpitter>();
            Check(Get<AudioClip>(ember, "fireSound") == AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/platformerPack/Sounds/sfx_throw.ogg"), "Ember Fire Sound references sfx_throw.ogg");

            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            var next = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(next, "Assets/WaterContactNext.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/WaterContactNext.unity", true) };
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.playModeStateChanged += OnPlay;
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Finish(error); }
    }

    private static async void OnPlay(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        Application.logMessageReceived += Log;
        try
        {
            Application.runInBackground = true;
            var camera = new GameObject("Test camera");
            camera.tag = "MainCamera";
            camera.AddComponent<Camera>();
            camera.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0, 0, -5);
            Object.DontDestroyOnLoad(camera);
            AudioListener.volume = 0f;
            tone = AudioClip.Create("Contact test tone", 44100 * 4, 1, 44100, false);
            tone.SetData(Enumerable.Range(0, 44100 * 4).Select(i => 0.1f * Mathf.Sin(i * 440 * 2f * Mathf.PI / 44100)).ToArray(), 0);

            GameObject puddle = Spawn("WaterSource.prefab", Vector2.zero);
            AudioSource audio = puddle.GetComponent<AudioSource>();
            await Task.Delay(150);
            Check(!audio.isPlaying && !Playing, "WaterSource stays silent before contact");
            Player player = await PlayerAt(new Vector2(3, 0));
            WaterMeter water = player.GetComponent<WaterMeter>();
            int refills = 0;
            water.RefilledFromSource += () => refills++;
            Move(player, Vector2.zero);
            await Task.Delay(100);
            Check(Playing, "actual chosen WaterSource clip plays on full-water entry");
            Check(!audio.isPlaying, "shared Sfx player prevents duplicate local playback");
            Check(refills == 0, "full entry preserves existing refill event rules");
            Stop();
            await Task.Delay(180);
            Check(!Playing, "staying in the puddle does not restart the contact sound");

            Move(player, new Vector2(3, 0));
            await Task.Delay(100);
            water.Consume(10);
            Move(player, Vector2.zero);
            await Task.Delay(100);
            Check(Playing && refills == 1 && water.NormalizedWater == 1f, "re-entry plays once and still refills the single water meter");
            Check(Vector2.Distance(water.Checkpoint, puddle.transform.Find("RespawnPoint").position) < 0.01f, "contact still saves the source checkpoint");

            await SilentEntry(player, audio, () => audio.mute = true, "muted AudioSource suppresses contact sound");
            audio.mute = false;
            await SilentEntry(player, audio, () => audio.enabled = false, "disabled AudioSource suppresses contact sound");
            audio.enabled = true;
            await SilentEntry(player, audio, () => puddle.GetComponent<WaterSourceSound>().enabled = false, "disabled contact component stays silent");
            puddle.GetComponent<WaterSourceSound>().enabled = true;
            AudioClip selected = audio.clip;
            await SilentEntry(player, audio, () => audio.clip = null, "empty source slot stays silent without errors");
            audio.clip = selected;

            Move(player, new Vector2(3, 0));
            await Task.Delay(100);
            Stop();
            var nonPlayer = new GameObject("Non-player contact");
            objects.Add(nonPlayer);
            nonPlayer.AddComponent<Rigidbody2D>().gravityScale = 0;
            nonPlayer.AddComponent<CircleCollider2D>().radius = 0.2f;
            await Task.Delay(100);
            Check(!Playing, "non-player physics contacts do not play the water cue");
            Object.Destroy(nonPlayer);

            player.enabled = false;
            Stop();
            puddle.SendMessage("OnTriggerEnter2D", player.GetComponent<Collider2D>(), SendMessageOptions.DontRequireReceiver);
            Check(Playing, "contact sound works even when LevelExit already disabled Player in the same step");
            await Clean();

            player = await PlayerAt(new Vector2(3, 0));
            var enemy = Spawn("Enemies/EmberSpitter.prefab", Vector2.zero).GetComponent<EmberSpitter>();
            enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            Set(enemy, "windupDuration", 10f);
            Set(enemy, "cooldownDuration", 100f);
            await Until(() => enemy.CurrentState == EmberSpitter.ShotState.Windup);
            Check(!Playing, "no fire cue while preparing the shot");
            Set(enemy, "timer", 0f);
            await Until(() => Object.FindObjectsByType<EmberFireball>(FindObjectsSortMode.None).Length == 1);
            Check(Playing, "a real Ember projectile launch plays the assigned sfx_throw recording");
            Stop();
            await Task.Delay(180);
            Check(!Playing, "Ember cooldown does not loop the shot sound");
            await Clean();

            player = await PlayerAt(new Vector2(3, 0));
            enemy = Spawn("Enemies/EmberSpitter.prefab", Vector2.zero).GetComponent<EmberSpitter>();
            enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            Set(enemy, "windupDuration", 10f);
            await Until(() => enemy.CurrentState == EmberSpitter.ShotState.Windup);
            Move(player, new Vector2(30, 0));
            Set(enemy, "timer", 0f);
            await Task.Delay(100);
            Check(!Playing && Object.FindObjectsByType<EmberFireball>(FindObjectsSortMode.None).Length == 0, "canceled out-of-range shot produces neither projectile nor throw sound");
            await Clean();

            player = await PlayerAt(new Vector2(3, 0));
            puddle = Spawn("WaterSource.prefab", Vector2.zero);
            puddle.GetComponent<AudioSource>().clip = tone;
            var exit = puddle.GetComponent<LevelExit>();
            if (exit == null) exit = puddle.AddComponent<LevelExit>();
            Set(exit, "nextSceneName", "WaterContactNext");
            Set(exit, "fadeDuration", 0.2f);
            Stop();
            Move(player, Vector2.zero);
            await Until(() => SceneManager.GetActiveScene().name == "WaterContactNext");
            Check(puddle == null && player == null, "real LevelExit still loads the next scene and removes the old objects");
            Check(Playing, "contact cue continues after the source scene unloads");
            await Task.Delay(200);
            Check(Playing, "scene transition does not cut off the remainder of the cue");
            Check(errors.Count == 0, "no gameplay or audio errors during verification");
            Finish(null);
        }
        catch (Exception error) { Finish(error); }
    }

    private static async Task SilentEntry(Player player, AudioSource audio, Action configure, string label)
    {
        Move(player, new Vector2(3, 0));
        await Task.Delay(100);
        configure();
        Stop();
        Move(player, Vector2.zero);
        await Task.Delay(100);
        Check(!Playing, label);
    }

    private static GameObject Spawn(string path, Vector2 position)
    {
        var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + path), position, Quaternion.identity);
        objects.Add(root);
        return root;
    }

    private static async Task<Player> PlayerAt(Vector2 position)
    {
        var root = Spawn("Player.prefab", position);
        var water = root.GetComponent<WaterMeter>();
        foreach (string field in new[] { "dryDrainPerSecond", "wetDrainPerSecond", "extraDryDrainPerSecond", "dryIdleDrainPerSecond", "wetIdleDrainPerSecond", "extraDryIdleDrainPerSecond" }) Set(water, field, 0f);
        Set(water, "fallLimit", -10000f);
        root.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        root.GetComponent<Rigidbody2D>().gravityScale = 0;
        foreach (var receiver in root.GetComponentsInChildren<SoundOnAnimationEvent>()) receiver.enabled = false;
        await Task.Delay(100);
        return root.GetComponent<Player>();
    }

    private static void Move(Player player, Vector2 position) { player.GetComponent<Rigidbody2D>().position = position; Physics2D.SyncTransforms(); }
    private static bool Playing => Source != null && Source.isPlaying;
    private static void Stop() { if (Source != null) Source.Stop(); }
    private static async Task Clean() { foreach (var item in objects) if (item != null) Object.Destroy(item); objects.Clear(); await Task.Delay(100); Stop(); }
    private static async Task Until(Func<bool> condition) { for (int i = 0; i < 120; i++) { if (condition()) return; await Task.Delay(10); } throw new Exception("Timed out waiting for gameplay event"); }
    private static void Set(object target, string name, object value) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    private static T Get<T>(object target, string name) => (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    private static void Check(bool condition, string label) { if (!condition) throw new Exception(label); passed.Add(label); }
    private static void Log(string message, string stack, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception) return;
        if (message.StartsWith("ArgumentOutOfRangeException") && stack.Contains("UnityEditor.Search.")) indexerIssues.Add(message + "\n" + stack);
        else errors.Add(message + "\n" + stack);
    }
    private static void Finish(Exception error)
    {
        var lines = passed.Select(label => "PASS " + label).ToList();
        lines.AddRange(errors.Select(item => "LOG_ERROR " + item));
        lines.AddRange(indexerIssues.Select(item => "EDITOR_INDEXER_ISSUE " + item));
        lines.Add(error == null ? "RESULT PASS " + passed.Count : "RESULT FAIL " + error);
        File.WriteAllLines("water-ember-sound-result.txt", lines);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}
