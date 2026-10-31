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

// Run in an isolated batch project; never plays or saves the user's open scene.
public static class FallingStoneTrapVerification
{
    private const string PrefabPath = "Assets/Prefabs/Traps/Trap_stoneSpike.prefab";
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
            if (configure) ConfigurePrefab();
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Check(AssetDatabase.AssetPathToGUID(PrefabPath) == "da13560975de47940ae537d8e390984d", "Original prefab GUID preserved");
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string guid, out long rootId);
            Check(rootId == 6916983635194185045L, "Original root fileID preserved for Level_2 instance");
            Check(prefab.GetComponentsInChildren<Component>(true).All(c => c != null), "Prefab imports without missing scripts");
            Check(prefab.GetComponent<FallingStoneTrap>() != null && prefab.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Kinematic,
                "Saved prefab is wired and waits without gravity");
            PolygonCollider2D collider = prefab.GetComponent<PolygonCollider2D>();
            Check(collider.isTrigger && !collider.enabled && collider.pathCount > 0, "Sprite-shaped hitbox is inactive until release and cannot block terrain");
            Check(collider.includeLayers == LayerMask.GetMask("Player") && collider.excludeLayers == ~LayerMask.GetMask("Player"),
                "Hitbox interacts only with Player, leaving global physics settings intact");
            SpriteRenderer visual = prefab.GetComponent<SpriteRenderer>();
            Check(visual.sprite != null && visual.sortingLayerName == "Level" && visual.sortingOrder == -1,
                "Existing sprite draws on Level order -1 behind Level_2 ground order 0");
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(visual.sprite, out string spriteGuid, out long spriteId);
            Check(spriteGuid == "c96dd4ea2fd5f5e4694712c72d6053ab" && spriteId == 21300000,
                "Original sprite reference is preserved");
            Check(Quaternion.Angle(prefab.transform.localRotation, Quaternion.Euler(0f, 0f, 180f)) < 0.01f,
                "Original downward sprite rotation is preserved");
            assetChecks = passed.Count;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.playModeStateChanged += EnteredPlay;
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Finish(error); }
    }

    private static void ConfigurePrefab()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            Rigidbody2D body = root.GetComponent<Rigidbody2D>();
            if (body == null) body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            PolygonCollider2D hitbox = root.GetComponent<PolygonCollider2D>();
            if (hitbox == null) hitbox = root.AddComponent<PolygonCollider2D>();
            hitbox.isTrigger = true;
            hitbox.enabled = false;
            hitbox.includeLayers = LayerMask.GetMask("Player");
            hitbox.excludeLayers = ~LayerMask.GetMask("Player");
            hitbox.layerOverridePriority = 1;
            FallingStoneTrap trap = root.GetComponent<FallingStoneTrap>();
            if (trap == null) trap = root.AddComponent<FallingStoneTrap>();
            var settings = new SerializedObject(trap);
            settings.FindProperty("playerLayerMask").intValue = LayerMask.GetMask("Player");
            settings.ApplyModifiedPropertiesWithoutUndo();
            root.GetComponent<SpriteRenderer>().sortingLayerName = "Level";
            root.GetComponent<SpriteRenderer>().sortingOrder = -1;
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            AssetDatabase.SaveAssets();
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
            await Detection();
            await FallAndCleanup();
            await Contact();
            Finish(null);
        }
        catch (Exception error) { Finish(error); }
    }

    private static async Task Detection()
    {
        FallingStoneTrap trap = SpawnTrap();
        var decoy = new GameObject("Non-Player decoy"); objects.Add(decoy);
        decoy.layer = LayerMask.NameToLayer("Player");
        decoy.transform.position = new Vector2(0f, 1f);
        decoy.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        decoy.AddComponent<CircleCollider2D>();
        await Delay(0.15f);
        Check(!Falling(trap) && trap.transform.position.y == 4f, "Non-Player collider on Player layer cannot release trap");
        Object.DestroyImmediate(decoy);
        Player player = SpawnPlayer(new Vector2(3f, 1f));
        await Delay(0.12f);
        Check(!Falling(trap), "Player outside horizontal range does not release trap");
        Teleport(player, new Vector2(0f, 5.5f)); await Delay(0.1f);
        Check(!Falling(trap), "Player above the stone does not release trap");
        Teleport(player, new Vector2(0f, 1f));
        player.GetComponent<CircleCollider2D>().isTrigger = true; await Delay(0.1f);
        Check(!Falling(trap), "Player trigger sensors do not release trap");
        player.GetComponent<CircleCollider2D>().isTrigger = false;
        player.enabled = false; await Delay(0.1f);
        Check(!Falling(trap), "Disabled Player cannot release trap");
        player.enabled = true;
        player.GetComponent<Rigidbody2D>().simulated = false; await Delay(0.1f);
        Check(!Falling(trap), "Player with stopped physics cannot release trap");
        player.GetComponent<Rigidbody2D>().simulated = true;
        player.GetComponent<CircleCollider2D>().enabled = false; await Delay(0.1f);
        Check(!Falling(trap), "Disabled Player collider cannot release trap");
        player.GetComponent<CircleCollider2D>().enabled = true;
        player.gameObject.layer = LayerMask.NameToLayer("Default"); await Delay(0.1f);
        Check(!Falling(trap), "Player mask filters other layers");
        player.gameObject.layer = LayerMask.NameToLayer("Player");
        Teleport(player, new Vector2(-1.3f, 1f)); await Delay(0.1f);
        Check(Falling(trap) && trap.GetComponent<Rigidbody2D>().linearVelocity.y < 0f, "Approach from the left releases the actual rotated prefab downward");
        Teleport(player, new Vector2(8f, 1f)); float previous = trap.transform.position.y; await Delay(0.15f);
        Check(Falling(trap) && trap.transform.position.y < previous, "Released stone keeps falling after Player leaves");
        Clear();
        trap = SpawnTrap(); SpawnPlayer(new Vector2(1.3f, 1f)); await Delay(0.1f);
        Check(Falling(trap), "Approach from the right also releases stone");
        Clear();
    }

    private static async Task FallAndCleanup()
    {
        foreach (string layer in new[] { "Ground-Dry", "Ground-Wet", "Ground-Extra-Dry" })
        {
            FallingStoneTrap trap = SpawnTrap();
            var floor = new GameObject(layer); objects.Add(floor);
            floor.layer = LayerMask.NameToLayer(layer);
            floor.AddComponent<BoxCollider2D>().size = new Vector2(8f, 0.5f);
            Player player = SpawnPlayer(new Vector2(1.3f, 1f));
            await Delay(0.1f); Object.DestroyImmediate(player.gameObject);
            await Delay(0.9f);
            Check(trap != null && trap.transform.position.y < -1f && Mathf.Abs(trap.transform.position.x) < 0.001f,
                "Stone passes through " + layer + " vertically without rotating or stopping");
            Check(Quaternion.Angle(trap.transform.rotation, Quaternion.Euler(0f, 0f, 180f)) < 0.01f,
                "Fall preserves orientation through " + layer);
            Check(trap.GetComponent<Rigidbody2D>().linearVelocity.y > -15f, "Fall speed stays bounded on " + layer);
            if (layer == "Ground-Dry")
            {
                await Delay(1.6f); Check(trap != null, "Stone remains until its configured three-second lifetime");
                await Delay(0.6f); Check(trap == null, "Stone destroys itself after three seconds even after Player is removed");
            }
            Clear();
        }
        FallingStoneTrap suspended = SpawnTrap(); SpawnPlayer(new Vector2(1.3f, 1f)); await Delay(0.15f);
        suspended.enabled = false; float y = suspended.GetComponent<Rigidbody2D>().position.y; await Delay(0.2f);
        Check(Mathf.Abs(suspended.GetComponent<Rigidbody2D>().position.y - y) < 0.001f && !suspended.GetComponent<PolygonCollider2D>().enabled,
            "Disabling behaviour suspends falling physics and damage");
        suspended.enabled = true; await Delay(0.15f);
        Check(suspended.transform.position.y < y, "Re-enabling resumes the same one-shot fall");
        Clear();
    }

    private static async Task Contact()
    {
        FallingStoneTrap trap = SpawnTrap(); Player player = SpawnPlayer(new Vector2(0f, 1f));
        WaterMeter water = player.GetComponent<WaterMeter>();
        KnockbackAbility knockback = player.GetComponent<KnockbackAbility>();
        await Delay(0.65f);
        Check(knockback.IsFlashing && Mathf.Abs(water.CurrentWater - 90f) < 0.01f,
            "Falling contact uses the real Player's ten-point water hit and flash protection");
        Check(player.CurrentState == PlayerStates.State.Knockback || player.CurrentState == PlayerStates.State.Jump,
            "Hit enters the existing knockback/movement state flow");
        Check(!trap.GetComponent<PolygonCollider2D>().enabled, "Stone disables its damage hitbox after its first Player contact");
        await Delay(0.35f);
        Check(Mathf.Abs(water.CurrentWater - 90f) < 0.01f, "Continued falling cannot deal duplicate water damage");
        Clear();
        trap = SpawnTrap(); player = SpawnPlayer(new Vector2(0f, 1f));
        knockback = player.GetComponent<KnockbackAbility>(); water = player.GetComponent<WaterMeter>();
        Check(knockback.TryWaterHit(1f), "Fixture starts existing shared immunity");
        await Delay(0.7f);
        Check(Mathf.Abs(water.CurrentWater - 99f) < 0.01f, "Existing flash immunity rejects stone damage");
        Clear();
        trap = SpawnTrap(); player = SpawnPlayer(new Vector2(0f, 1f)); water = player.GetComponent<WaterMeter>();
        water.Consume(95f); int deaths = 0; water.Depleted += () => deaths++;
        await Delay(0.65f);
        Check(water.IsRespawning && !player.isActiveAndEnabled && deaths == 1,
            "Lethal stone contact starts exactly one existing respawn with controls stopped");
        await Delay(1.5f);
        Check(player.isActiveAndEnabled && !water.IsRespawning && water.CurrentWater == WaterMeter.Capacity && deaths == 1,
            "Existing respawn restores Player and water without a second death");
        Clear();
    }

    private static FallingStoneTrap SpawnTrap()
    {
        GameObject root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
        root.transform.position = new Vector2(0f, 4f); objects.Add(root); Physics2D.SyncTransforms();
        return root.GetComponent<FallingStoneTrap>();
    }

    private static Player SpawnPlayer(Vector2 position)
    {
        GameObject root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab"), position, Quaternion.identity);
        objects.Add(root);
        root.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
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

    private static bool Falling(FallingStoneTrap trap) => trap.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Dynamic;
    private static async Task Delay(float seconds)
    {
        float until = Time.time + seconds;
        double timeout = EditorApplication.timeSinceStartup + 30f;
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
        Debug.unityLogger.filterLogType = LogType.Log;
        string result = error == null
            ? "PASS: Unity " + Application.unityVersion + "; " + passed.Count + " checks (" + assetChecks + " asset, " + (passed.Count - assetChecks) + " Play Mode). Current Player, input, scripts and original stone prefab. Isolated project; Level_2 gameplay and exported build not tested."
            : "FAIL after " + passed.Count + " checks: " + error;
        File.WriteAllText("stone-spike-result.txt", result);
        File.WriteAllLines("stone-spike-checks.txt", passed);
        Debug.Log(result);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}
