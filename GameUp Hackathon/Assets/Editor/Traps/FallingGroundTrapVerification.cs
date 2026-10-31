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

// Use an isolated batch project so the user's open scene is never played or saved.
public static class FallingGroundTrapVerification
{
    private static readonly string[] paths = {
        "Assets/Prefabs/Traps/FallingGround_Sand.prefab",
        "Assets/Prefabs/Traps/FallingGround_Rock.prefab"
    };
    private static readonly string[] guids = { "d3be7f3f7b9ab2b4aaa584e058a0ed9b", "15e4f0ad88567ec478ee740d212673f0" };
    private static readonly long[] rootIds = { 3346137768618089391L, 8756696476681863780L };
    private static readonly List<string> passed = new List<string>();
    private static readonly List<GameObject> objects = new List<GameObject>();
    private static int assetChecks;

    public static void ConfigureAndRun() => Start(true);
    public static void Run() => Start(false);

    private static void Start(bool configure)
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use a separate batch project.");
        try
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (configure) Configure(paths[i], i);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string guid, out long rootId);
                Check(guid == guids[i] && rootId == rootIds[i], paths[i] + ": original GUID and root fileID preserved");
                Check(prefab.GetComponentsInChildren<Component>(true).All(c => c != null), paths[i] + ": no missing scripts");
                Check(prefab.GetComponentsInChildren<SpriteRenderer>().Length == 2 &&
                    prefab.GetComponentsInChildren<SpriteRenderer>().All(s => s.sprite != null), paths[i] + ": both original sprites import");
                Check(prefab.layer == LayerMask.NameToLayer(i == 0 ? "Ground-Dry" : "Ground-Extra-Dry") &&
                    prefab.GetComponent<GroundSurface>().SurfaceMaterial == (i == 0 ? GroundSurface.Material.Sand : GroundSurface.Material.BlackStone),
                    paths[i] + ": existing dryness and material systems wired");
                Rigidbody2D body = prefab.GetComponent<Rigidbody2D>();
                BoxCollider2D collider = prefab.GetComponent<BoxCollider2D>();
                Check(body.bodyType == RigidbodyType2D.Kinematic && body.gravityScale == 0f && collider.enabled && !collider.isTrigger &&
                    collider.size == new Vector2(2f, 1f), paths[i] + ": solid two-tile support waits without gravity");
                Check(Mathf.Approximately(new SerializedObject(prefab.GetComponent<FallingGroundTrap>()).FindProperty("fallDelay").floatValue, 0.25f),
                    paths[i] + ": quarter-second default delay");
            }
            assetChecks = passed.Count;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.playModeStateChanged += EnteredPlay;
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Finish(error); }
    }

    private static void Configure(string path, int index)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            root.layer = LayerMask.NameToLayer(index == 0 ? "Ground-Dry" : "Ground-Extra-Dry");
            Rigidbody2D body = root.GetComponent<Rigidbody2D>();
            if (body == null) body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            BoxCollider2D collider = root.GetComponent<BoxCollider2D>();
            if (collider == null) collider = root.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(2f, 1f);
            collider.offset = Vector2.zero;
            collider.isTrigger = false;
            if (root.GetComponent<FallingGroundTrap>() == null) root.AddComponent<FallingGroundTrap>();
            GroundSurface surface = root.GetComponent<GroundSurface>();
            if (surface == null) surface = root.AddComponent<GroundSurface>();
            var settings = new SerializedObject(surface);
            settings.FindProperty("surfaceMaterial").enumValueIndex = index == 0 ? 1 : 3;
            settings.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static async void EnteredPlay(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        try
        {
            Debug.unityLogger.filterLogType = LogType.Warning;
            Application.targetFrameRate = 120;
            for (int i = 0; i < paths.Length; i++)
            {
                await Detection(i);
                await LandingAndFall(i);
            }
            await Lifecycle();
            Finish(null);
        }
        catch (Exception error) { Finish(error); }
    }

    private static async Task Detection(int index)
    {
        FallingGroundTrap trap = SpawnTrap(index);
        await Delay(0.35f);
        Check(!Triggered(trap) && trap.GetComponent<Rigidbody2D>().position == new Vector2(0f, 4f), paths[index] + ": no Player, no fall");
        var decoy = new GameObject("Non-Player weight"); objects.Add(decoy);
        decoy.layer = LayerMask.NameToLayer("Player"); decoy.transform.position = new Vector2(0f, 5f);
        decoy.AddComponent<Rigidbody2D>().gravityScale = 1f; decoy.AddComponent<CircleCollider2D>().radius = 0.5f;
        await Delay(0.45f);
        Check(!Triggered(trap), paths[index] + ": non-Player weight does not trigger");
        Object.DestroyImmediate(decoy);
        Player player = SpawnPlayer(new Vector2(1.4f, 4f));
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        body.gravityScale = 0f; body.linearVelocity = Vector2.left;
        await Delay(0.12f);
        Check(!Triggered(trap), paths[index] + ": side contact does not trigger");
        Teleport(player, new Vector2(0f, 3.1f)); body.linearVelocity = Vector2.up;
        await Delay(0.12f);
        Check(!Triggered(trap), paths[index] + ": underside contact does not trigger");
        Teleport(player, new Vector2(0f, 4.95f)); body.linearVelocity = Vector2.down;
        player.enabled = false;
        await Delay(0.35f);
        Check(!Triggered(trap), paths[index] + ": disabled Player does not trigger");
        Clear();
    }

    private static async Task LandingAndFall(int index)
    {
        FallingGroundTrap trap = SpawnTrap(index);
        Player player = SpawnPlayer(new Vector2(0f, 5.3f));
        await Until(() => Triggered(trap));
        // Collision callbacks follow PhysicsControl's FixedUpdate; let it observe the new contact.
        await Delay(0.04f);
        Check(player.IsGrounded, paths[index] + ": actual Player lands and recognizes support");
        Check(player.physicsControl.GroundMaterial == (index == 0 ? GroundSurface.Material.Sand : GroundSurface.Material.BlackStone),
            paths[index] + ": actual Player reads ground material");
        await Delay(0.1f);
        Check(!Falling(trap) && trap.GetComponent<BoxCollider2D>().enabled &&
            Mathf.Abs(trap.GetComponent<Rigidbody2D>().position.y - 4f) < 0.001f, paths[index] + ": platform stays solid during initial delay");
        Teleport(player, new Vector2(5f, 5f));
        await Delay(0.2f);
        Check(Falling(trap) && !trap.GetComponent<BoxCollider2D>().enabled, paths[index] + ": platform falls after delay even when Player leaves");
        await Delay(0.3f);
        Rigidbody2D body = trap.GetComponent<Rigidbody2D>();
        Check(body.position.y < 3.5f && Mathf.Abs(body.position.x) < 0.001f && Mathf.Abs(body.rotation) < 0.001f,
            paths[index] + ": platform falls vertically without rotation");
        await Delay(0.5f);
        Check(body.linearVelocity.y > -15f, paths[index] + ": downward speed stays bounded");
        await Delay(2.3f);
        Check(trap == null, paths[index] + ": fallen platform is cleaned up");
        Clear();
    }

    private static async Task Lifecycle()
    {
        FallingGroundTrap trap = SpawnTrap(0);
        Player player = SpawnPlayer(new Vector2(0f, 5.3f));
        var settings = new SerializedObject(trap);
        settings.FindProperty("fallDelay").floatValue = 0.6f; settings.ApplyModifiedPropertiesWithoutUndo();
        await Until(() => Triggered(trap));
        await Delay(0.3f);
        Check(!Falling(trap), "Inspector delay controls the grace period");
        trap.enabled = false;
        await Delay(0.4f);
        Check(!Falling(trap), "Disabled component suspends the countdown");
        trap.enabled = true;
        await Delay(0.35f);
        Check(Falling(trap), "Re-enabled component resumes the countdown");
        trap.enabled = false; float y = trap.GetComponent<Rigidbody2D>().position.y;
        await Delay(0.2f);
        Check(Mathf.Abs(trap.GetComponent<Rigidbody2D>().position.y - y) < 0.001f, "Disabling during fall suspends motion");
        trap.enabled = true; await Delay(0.15f);
        Check(trap.GetComponent<Rigidbody2D>().position.y < y, "Re-enabling resumes the same fall");
        Clear();
        trap = SpawnTrap(1); player = SpawnPlayer(new Vector2(0f, 5.3f));
        await Until(() => Triggered(trap));
        float remaining = Remaining(trap); Time.timeScale = 0f;
        await Task.Delay(200);
        Check(Mathf.Approximately(remaining, Remaining(trap)) && !Falling(trap), "Pause suspends the countdown");
        Time.timeScale = 1f; await Delay(0.3f);
        Check(Falling(trap), "Unpause resumes the countdown");
        await Delay(0.12f);
        Check(!player.IsGrounded, "Player loses support after platform releases");
        Clear();
    }

    private static FallingGroundTrap SpawnTrap(int index)
    {
        GameObject root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(paths[index]));
        root.transform.position = new Vector2(0f, 4f); objects.Add(root); Physics2D.SyncTransforms();
        return root.GetComponent<FallingGroundTrap>();
    }

    private static Player SpawnPlayer(Vector2 position)
    {
        GameObject root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab"), position, Quaternion.identity);
        objects.Add(root);
        WaterMeter water = root.GetComponent<WaterMeter>();
        foreach (string field in new[] { "dryIdleDrainPerSecond", "wetIdleDrainPerSecond", "extraDryIdleDrainPerSecond", "dryDrainPerSecond", "wetDrainPerSecond", "extraDryDrainPerSecond" })
            typeof(WaterMeter).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(water, 0f);
        Physics2D.SyncTransforms();
        return root.GetComponent<Player>();
    }

    private static void Teleport(Player player, Vector2 position)
    {
        player.GetComponent<Rigidbody2D>().position = position;
        player.GetComponent<WaterMeter>().ResetPositionTracking(); Physics2D.SyncTransforms();
    }

    private static bool Triggered(FallingGroundTrap trap) => (bool)typeof(FallingGroundTrap).GetField("triggered", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(trap);
    private static float Remaining(FallingGroundTrap trap) => (float)typeof(FallingGroundTrap).GetField("remaining", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(trap);
    private static bool Falling(FallingGroundTrap trap) => trap.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Dynamic;
    private static async Task Until(Func<bool> condition)
    {
        double timeout = EditorApplication.timeSinceStartup + 10f;
        while (!condition())
        {
            if (EditorApplication.timeSinceStartup > timeout) throw new TimeoutException("Player did not land");
            await Task.Delay(2);
        }
    }

    private static async Task Delay(float seconds)
    {
        float until = Time.time + seconds;
        double timeout = EditorApplication.timeSinceStartup + 15f;
        while (Time.time < until)
        {
            if (EditorApplication.timeSinceStartup > timeout) throw new TimeoutException("Play Mode did not advance");
            await Task.Delay(2);
        }
    }

    private static void Clear()
    {
        foreach (GameObject root in objects) if (root != null) Object.DestroyImmediate(root);
        objects.Clear();
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        passed.Add(message);
    }

    private static void Finish(Exception error)
    {
        Time.timeScale = 1f;
        string result = error == null
            ? "PASS: Unity " + Application.unityVersion + "; " + passed.Count + " checks (" + assetChecks + " asset, " + (passed.Count - assetChecks) + " Play Mode). Actual Player and both ground prefabs in isolated project; main level and exported build not tested."
            : "FAIL after " + passed.Count + " checks: " + error;
        File.WriteAllText("falling-ground-result.txt", result);
        File.WriteAllLines("falling-ground-checks.txt", passed);
        Debug.unityLogger.filterLogType = LogType.Log; Debug.Log(result);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}
