using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

// Run only in an isolated copy: -executeMethod EnemyTerrainVerification.Run (without -quit).
public static class EnemyTerrainVerification
{
    private static readonly List<GameObject> objects = new List<GameObject>();
    private static readonly List<string> passed = new List<string>();
    private static int assetChecks;

    public static void Run()
    {
        try
        {
            if (!Application.isBatchMode)
                throw new InvalidOperationException("Use an isolated batch project; this runner replaces the test scene.");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EnemyBoundaryBuilder.Configure();
            int ground = LayerMask.GetMask("Ground-Dry", "Ground-Wet", "Ground-Extra-Dry");
            foreach (string name in new[] { "Saw", "SlimeFire", "HapsirikKoz" })
            {
                foreach (string variant in new[] { "Stationary", "Patrol" })
                {
                    GameObject prefab = Load(name + "_" + variant);
                    Rigidbody2D body = prefab.GetComponent<Rigidbody2D>();
                    Check(body.gravityScale == (name == "Saw" ? 0f : 1f) &&
                        body.bodyType == (name == "Saw" && variant == "Stationary" ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic),
                        prefab.name + " saves its flying/ground body settings");
                    Collider2D solid = prefab.transform.Find("TerrainBody").GetComponent<Collider2D>();
                    int terrainLayers = name == "Saw" ? ground : ground | LayerMask.GetMask(EnemyBoundaryBuilder.BoundaryLayer);
                    Check(!solid.isTrigger && solid.includeLayers == terrainLayers && solid.excludeLayers == ~terrainLayers &&
                        solid.sharedMaterial != null && solid.sharedMaterial.friction == 0f,
                        prefab.name + " blocks all three ground layers with a separate frictionless shape");
                    Check(name == "Saw" || solid.gameObject.layer == LayerMask.NameToLayer(EnemyBoundaryBuilder.GroundEnemyLayer),
                        prefab.name + " saves the appropriate terrain collision layer");
                    Check(prefab.GetComponent<Collider2D>().isTrigger && prefab.GetComponent<EnemyContactKnockback>() != null,
                        prefab.name + " preserves the original body damage trigger");
                }
            }
            Check(Load("HapsirikKoz_Smoke").GetComponent<Rigidbody2D>().gravityScale == 0f &&
                Load("KozOcagi_Stationary").GetComponent<Rigidbody2D>() == null,
                "Smoke and anchored Koz Ocagi keep their environmental behaviour");
            GameObject boundary = Load("GroundEnemyBoundary");
            EdgeCollider2D edge = boundary.GetComponent<EdgeCollider2D>();
            Check(edge != null && !edge.isTrigger && boundary.GetComponent<Renderer>() == null && edge.points.Length == 2,
                "Invisible boundary prefab has an editable solid edge and no renderer");
            int walking = LayerMask.GetMask(EnemyBoundaryBuilder.GroundEnemyLayer);
            Check(boundary.layer == LayerMask.NameToLayer(EnemyBoundaryBuilder.BoundaryLayer) &&
                edge.includeLayers == walking && edge.excludeLayers == ~walking && edge.layerOverridePriority == 2,
                "Boundary collides only with ground enemy physics bodies");
            assetChecks = passed.Count;
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
            Debug.unityLogger.filterLogType = LogType.Warning;
            foreach (string ground in new[] { "Ground-Dry", "Ground-Wet", "Ground-Extra-Dry" })
            {
                foreach (string name in new[] { "SlimeFire_Stationary", "SlimeFire_Patrol", "HapsirikKoz_Stationary", "HapsirikKoz_Patrol" })
                {
                    Terrain(new Vector2(0f, -0.5f), new Vector2(12f, 1f), ground);
                    Rigidbody2D body = Spawn(name, new Vector2(0f, 2f));
                    EnemyPatrol motor = body.GetComponent<EnemyPatrol>();
                    if (motor != null) Set(motor, "speed", 0f);
                    await Delay(0.2f);
                    Check(body.position.y < 1.9f && body.linearVelocity.y < -1f, name + " falls with motor stopped above " + ground);
                    await Delay(0.9f);
                    Check(Mathf.Abs(body.position.y) < 0.04f && Mathf.Abs(body.linearVelocity.y) < 0.1f,
                        name + " lands on " + ground + " without entering it");
                    Clear();
                }
            }
            await WallsAndFlight();
            await FallingAndRecovery();
            await EnemyBoundaries();
            await ContactAndHeat();
            Finish(null);
        }
        catch (Exception error) { Finish(error); }
    }

    private static async Task WallsAndFlight()
    {
        foreach (string name in new[] { "Saw_Patrol", "SlimeFire_Patrol", "HapsirikKoz_Patrol" })
        {
            Terrain(new Vector2(0f, -0.5f), new Vector2(12f, 1f));
            Terrain(new Vector2(1.5f, 1.5f), new Vector2(0.2f, 4f));
            Rigidbody2D body = Spawn(name, new Vector2(0f, name == "Saw_Patrol" ? 0.7f : 0f));
            Player target = SpawnPlayer(new Vector2(2.7f, 0.7f));
            await Delay(1.25f);
            Check(body.GetComponent<EnemyPatrol>().IsChasing && body.position.x > 0.5f,
                name + " chases a player across a wall");
            Check(body.transform.Find("TerrainBody").GetComponent<Collider2D>().bounds.max.x <= 1.42f,
                name + " cannot pass through the wall while chasing");
            await Delay(0.4f);
            Check(body.transform.Find("TerrainBody").GetComponent<Collider2D>().bounds.max.x <= 1.42f,
                name + " remains blocked under sustained chase");
            Teleport(target, new Vector2(-2f, 0.7f));
            float x = body.position.x;
            await Delay(0.3f);
            Check(body.position.x < x - 0.2f, name + " can leave a wall when the player changes sides");
            Clear();
        }
        Rigidbody2D saw = Spawn("Saw_Patrol", new Vector2(0f, 2f));
        Set(saw.GetComponent<EnemyPatrol>(), "speed", 0f);
        await Delay(0.5f);
        Check(Mathf.Abs(saw.position.y - 2f) < 0.01f, "Flying Saw does not fall with its motor stopped");
        Clear();

        Terrain(new Vector2(0f, -0.5f), new Vector2(10f, 1f));
        saw = Spawn("Saw_Patrol", new Vector2(0f, 1.5f));
        Player below = SpawnPlayer(new Vector2(0f, -1f));
        await Delay(1.1f);
        Check(saw.GetComponent<EnemyPatrol>().IsChasing && saw.position.y >= 0.43f && saw.position.y < 0.7f,
            "Saw flies downward to its target but cannot enter the floor");
        Clear();

        Terrain(new Vector2(0f, 2.5f), new Vector2(10f, 0.2f));
        saw = Spawn("Saw_Patrol", new Vector2(0f, 1f));
        SpawnPlayer(new Vector2(0f, 3.5f));
        await Delay(1.2f);
        Check(saw.position.y > 1.5f && saw.position.y <= 1.97f, "Saw can fly upward but cannot cross a ceiling");
        Clear();
    }

    private static async Task FallingAndRecovery()
    {
        foreach (string name in new[] { "SlimeFire_Patrol", "HapsirikKoz_Patrol" })
        {
            Terrain(new Vector2(0f, -0.5f), new Vector2(2f, 1f));
            Rigidbody2D body = Spawn(name, Vector2.zero);
            Player target = SpawnPlayer(new Vector2(2.8f, 1f));
            await Delay(1.4f);
            Check(body.position.x > 1.5f && body.position.y < -0.2f,
                name + " follows the player off a ledge and falls");
            Teleport(target, new Vector2(30f, 0f));
            float y = body.position.y;
            await Delay(0.25f);
            Check(!body.GetComponent<EnemyPatrol>().IsChasing && body.position.y < y - 0.2f,
                name + " keeps falling during route recovery instead of returning upward through terrain");
            float x = body.position.x;
            body.GetComponent<EnemyPatrol>().enabled = false;
            y = body.position.y;
            await Delay(0.2f);
            Check(Mathf.Abs(body.position.x - x) < 0.03f && body.position.y < y - 0.2f,
                name + " stops steering when disabled while gravity continues");
            Clear();

            Terrain(new Vector2(0f, -0.5f), new Vector2(2f, 1f));
            body = Spawn(name, Vector2.zero);
            Set(body.GetComponent<EnemyPatrol>(), "detectionRadius", 0f);
            await Delay(1.4f);
            Check(body.position.x > 1.5f && body.position.y < -0.2f, name + " also falls off ledges during normal patrol");
            Clear();

            body = Spawn(name, new Vector2(0f, 2f));
            Set(body.GetComponent<EnemyPatrol>(), "endOffset", new Vector2(0f, 3f));
            await Delay(0.3f);
            Check(body.position.y < 1.7f && Mathf.Abs(body.position.x) < 0.01f,
                name + " ignores vertical patrol offsets and does not suspend gravity");
            Clear();
        }
    }

    private static async Task EnemyBoundaries()
    {
        foreach (string name in new[] { "SlimeFire_Patrol", "HapsirikKoz_Patrol" })
        {
            foreach (float direction in new[] { -1f, 1f })
            {
                Terrain(new Vector2(0f, -0.5f), new Vector2(2f, 1f));
                EdgeCollider2D edge = Boundary(new Vector2(direction * 0.8f, 0f));
                Rigidbody2D body = Spawn(name, Vector2.zero);
                Player target = SpawnPlayer(new Vector2(direction * 2.7f, 0.35f));
                await Delay(1.4f);
                Check(body.GetComponent<EnemyPatrol>().IsChasing && direction * body.position.x > 0.15f &&
                    direction * body.position.x < 0.6f && Mathf.Abs(body.position.y) < 0.04f,
                    name + " stops before the " + (direction < 0f ? "left" : "right") + " cliff while chasing");
                Teleport(target, new Vector2(-direction * 2f, 0.35f));
                float x = body.position.x;
                await Delay(0.2f);
                Check(direction * (body.position.x - x) < -0.1f,
                    name + " can walk away from the enemy-only boundary");
                Teleport(target, new Vector2(direction * 2.7f, 0.35f));
                edge.enabled = false;
                await Delay(1.5f);
                Check(direction * body.position.x > 1.5f && body.position.y < -0.2f,
                    name + " can cross and fall after its boundary is disabled");
                Clear();
            }
        }

        Terrain(new Vector2(0f, -0.5f), new Vector2(2f, 1f));
        Boundary(new Vector2(0.8f, 0f));
        Rigidbody2D saw = Spawn("Saw_Patrol", new Vector2(0f, 0.7f));
        SpawnPlayer(new Vector2(2.7f, 0.7f));
        await Delay(1f);
        Check(saw.position.x > 1.1f && Mathf.Abs(saw.position.y - 0.7f) < 0.02f,
            "Flying Saw passes through ground enemy boundaries while continuing its chase");
        Clear();

        Boundary(Vector2.zero);
        Player player = SpawnPlayer(new Vector2(-1f, 0.35f));
        player.enabled = false;
        player.GetComponent<JumpAbility>().enabled = false;
        Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
        playerBody.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
        playerBody.gravityScale = 0f;
        playerBody.linearVelocity = new Vector2(2f, 0f);
        await Delay(1.2f);
        Check(playerBody.position.x > 1f && player.GetComponent<WaterMeter>().CurrentWater == 100f &&
            !player.GetComponent<KnockbackAbility>().IsFlashing,
            "Real player body crosses the invisible boundary without blocking or damage");
        Clear();
    }

    private static EdgeCollider2D Boundary(Vector2 position)
    {
        GameObject root = Object.Instantiate(Load("GroundEnemyBoundary"), position, Quaternion.identity);
        objects.Add(root);
        Physics2D.SyncTransforms();
        return root.GetComponent<EdgeCollider2D>();
    }

    private static async Task ContactAndHeat()
    {
        Terrain(new Vector2(0f, -0.5f), new Vector2(10f, 1f));
        Rigidbody2D slime = Spawn("SlimeFire_Stationary", Vector2.zero);
        Player player = SpawnPlayer(new Vector2(1.2f, 0.35f));
        WaterMeter water = player.GetComponent<WaterMeter>();
        KnockbackAbility hit = player.GetComponent<KnockbackAbility>();
        hit.enabled = true;
        await Delay(0.12f);
        Check(water.DrainMultiplier == 2f && !hit.IsFlashing, "Separate terrain body preserves Slime heat without a body hit");
        float before = water.CurrentWater;
        Teleport(player, new Vector2(0f, 0.35f));
        await Delay(0.08f);
        Check(hit.IsFlashing && before - water.CurrentWater > 9.8f && before - water.CurrentWater < 10.3f,
            "Original trigger still applies one water hit and knockback flash");
        Check(Mathf.Abs(player.transform.position.x) < 0.01f && Mathf.Abs(slime.position.x) < 0.01f,
            "Player can overlap the damage trigger without terrain body pushing either character");
        Teleport(player, new Vector2(8f, 0.35f));
        await Delay(0.08f);
        Check(water.DrainMultiplier == 1f, "Leaving the heat trigger clears the multiplier after physics changes");
        Clear();
    }

    private static GameObject Load(string name) => AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/" + name + ".prefab");

    private static Rigidbody2D Spawn(string name, Vector2 position)
    {
        GameObject root = Object.Instantiate(Load(name), position, Quaternion.identity);
        objects.Add(root);
        HapsirikKozSneeze sneeze = root.GetComponent<HapsirikKozSneeze>();
        if (sneeze != null) Set(sneeze, "automaticSneezing", false);
        Physics2D.SyncTransforms();
        return root.GetComponent<Rigidbody2D>();
    }

    private static Player SpawnPlayer(Vector2 position)
    {
        GameObject root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab"), position, Quaternion.identity);
        objects.Add(root);
        root.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        root.GetComponent<KnockbackAbility>().enabled = false;
        WaterMeter water = root.GetComponent<WaterMeter>();
        foreach (string field in new[] { "dryIdleDrainPerSecond", "wetIdleDrainPerSecond", "extraDryIdleDrainPerSecond", "dryDrainPerSecond", "wetDrainPerSecond", "extraDryDrainPerSecond", "airDrainPerSecond" })
        {
            FieldInfo info = typeof(WaterMeter).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
            if (info != null) info.SetValue(water, 0f);
        }
        Physics2D.SyncTransforms();
        return root.GetComponent<Player>();
    }

    private static void Terrain(Vector2 position, Vector2 size, string layer = "Ground-Dry")
    {
        var root = new GameObject(layer);
        objects.Add(root);
        root.layer = LayerMask.NameToLayer(layer);
        root.transform.position = position;
        root.AddComponent<BoxCollider2D>().size = size;
        Physics2D.SyncTransforms();
    }

    private static void Teleport(Player player, Vector2 position)
    {
        player.GetComponent<Rigidbody2D>().position = position;
        player.GetComponent<WaterMeter>().ResetPositionTracking();
        Physics2D.SyncTransforms();
    }

    private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    private static async Task Delay(float seconds)
    {
        float until = Time.time + seconds;
        double timeout = EditorApplication.timeSinceStartup + 30f;
        while (Time.time < until)
        {
            if (EditorApplication.timeSinceStartup > timeout) throw new TimeoutException("Play Mode stopped advancing");
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
        string result = error == null ? "PASS: Unity " + Application.unityVersion + "; " + passed.Count + " checks (" + assetChecks + " asset, " + (passed.Count - assetChecks) + " Play Mode). Isolated project; main levels and exported build not tested."
            : "FAIL after " + passed.Count + " checks: " + error;
        File.WriteAllText("enemy-terrain-result.txt", result);
        File.WriteAllLines("enemy-terrain-checks.txt", passed);
        Debug.Log(result);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}
