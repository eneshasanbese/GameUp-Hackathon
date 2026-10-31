using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

// Run with -executeMethod EnemyChaseVerification.Run in a separate copy of the project.
public static class EnemyChaseVerification
{
    private static readonly List<GameObject> objects = new List<GameObject>();
    private static readonly List<string> passed = new List<string>();
    private static Player player;
    private static Rigidbody2D playerBody;
    private static EnemyPatrol enemy;
    private static Rigidbody2D enemyBody;
    private static int assetChecks;

    public static void Run()
    {
        try
        {
            if (!Application.isBatchMode)
                throw new InvalidOperationException("Use an isolated batch project; this runner replaces the test scene.");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            foreach (string name in new[] { "Saw", "SlimeFire", "HapsirikKoz" })
            {
                GameObject prefab = Load(name + "_Patrol");
                var settings = new SerializedObject(prefab.GetComponent<EnemyPatrol>());
                Check(settings.FindProperty("detectionRadius").floatValue == 4f &&
                    settings.FindProperty("horizontalChaseOnly").boolValue == (name != "Saw"), name + " chase defaults are saved");
                Check(prefab.GetComponent<EnemyContactKnockback>() != null &&
                    prefab.GetComponent<Collider2D>().isTrigger &&
                    prefab.GetComponentInChildren<Animator>().runtimeAnimatorController != null, name + " retains body hits and animation");
                Check(Load(name + "_Stationary").GetComponent<EnemyPatrol>() == null, name + " stationary variant remains stationary");
            }
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
            foreach (string name in new[] { "Saw", "SlimeFire", "HapsirikKoz" })
            {
                Setup(name);
                await Delay(0.12f);
                Check(!enemy.IsChasing && enemyBody.position.x > 0.1f, name + " patrols with player outside");
                Teleport(new Vector2(-2f, 1f));
                Vector2 before = enemyBody.position;
                await Delay(0.2f);
                Check(enemy.IsChasing && enemyBody.position.x < before.x - 0.15f, name + " interrupts patrol to chase player");
                Check(name == "Saw" ? enemyBody.position.y > 0.05f : Mathf.Abs(enemyBody.position.y) < 0.03f,
                    name + " follows its configured chase axes");
                if (name != "Saw")
                    Check(enemy.GetComponentInChildren<SpriteRenderer>().flipX != (name == "SlimeFire"), name + " faces the chase direction");
                Teleport(new Vector2(30f, 0f));
                await Delay(0.7f);
                Check(!enemy.IsChasing && enemyBody.position.x > before.x + 0.1f && Mathf.Abs(enemyBody.position.y) < 0.03f,
                    name + " returns to the interrupted route and resumes patrol");
                Check(enemy.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).length > 0f, name + " animation keeps playing during movement");
                Clear();
            }
            await RangeAndLifecycle();
            await PatrolRegression();
            await HeatAndBodyContact();
            Finish(null);
        }
        catch (Exception error) { Finish(error); }
    }

    private static async Task RangeAndLifecycle()
    {
        Setup("Saw");
        Teleport(new Vector2(-2f, 0f));
        await Delay(0.12f);
        Check(enemy.IsChasing, "Player initially inside the area is detected without an enter event");
        player.enabled = false;
        await Delay(0.06f);
        Check(!enemy.IsChasing, "Disabled Player releases chase");
        player.enabled = true;
        await Delay(0.06f);
        Check(enemy.IsChasing, "Re-enabled Player inside the area is reacquired");
        playerBody.simulated = false;
        await Delay(0.06f);
        Check(!enemy.IsChasing, "Unsimulated player releases chase");
        playerBody.simulated = true;
        CircleCollider2D collider = player.GetComponent<CircleCollider2D>();
        collider.enabled = false;
        await Delay(0.06f);
        Check(!enemy.IsChasing, "Disabled player collider releases chase");
        var child = new GameObject("Extra player collider");
        child.transform.SetParent(player.transform, false);
        var extra = child.AddComponent<CircleCollider2D>(); extra.radius = 0.2f;
        Physics2D.SyncTransforms();
        await Delay(0.06f);
        Check(enemy.IsChasing, "An attached child collider can maintain detection");
        extra.isTrigger = true;
        await Delay(0.06f);
        Check(!enemy.IsChasing, "Player trigger sensors alone do not start chase");
        collider.enabled = true;
        await Delay(0.06f);
        Check(enemy.IsChasing, "Restoring the physical player collider reacquires chase");
        Set(enemy, "detectionRadius", 0f);
        await Delay(0.06f);
        Check(!enemy.IsChasing, "Zero detection radius disables chase");
        Set(enemy, "detectionRadius", 4f);
        Teleport(enemyBody.position + Vector2.left * 4.1f);
        await Delay(0.06f);
        Check(enemy.IsChasing, "Physical player overlap at the area boundary is detected");
        Teleport(enemyBody.position + Vector2.left * 4.8f);
        await Delay(0.06f);
        Check(!enemy.IsChasing, "Crossing fully outside the area stops chase");
        Teleport(enemyBody.position + Vector2.left * 2f);
        await Delay(0.06f);
        enemy.enabled = false;
        Check(!enemy.IsChasing, "Disabling patrol clears chase state immediately");
        enemy.enabled = true;
        await Delay(0.06f);
        Check(enemy.IsChasing, "Re-enabled patrol reacquires the player");
        player.GetComponent<WaterMeter>().RespawnAtCheckpoint();
        await Delay(0.06f);
        Check(!enemy.IsChasing, "Respawning player is not chased");
        await Delay(0.5f);
        Teleport(enemyBody.position + Vector2.left * 2f);
        await Delay(0.06f);
        Check(enemy.IsChasing, "Living player can be reacquired after respawn");
        player.gameObject.SetActive(false);
        await Delay(0.06f);
        Check(!enemy.IsChasing, "Inactive player cannot leave stale chase state");
        player.gameObject.SetActive(true);
        Teleport(enemyBody.position + Vector2.left * 2f);
        await Delay(0.06f);
        Object.DestroyImmediate(player.gameObject);
        await Delay(0.06f);
        Check(!enemy.IsChasing, "Destroyed player safely releases chase");
        Clear();
    }

    private static async Task PatrolRegression()
    {
        Setup("Saw");
        Set(enemy, "endOffset", new Vector2(-0.6f, 0.6f));
        Teleport(new Vector2(30f, 0f));
        await Delay(0.3f);
        Check(enemyBody.position.x < -0.1f && Mathf.Abs(enemyBody.position.x + enemyBody.position.y) < 0.001f,
            "Negative diagonal patrol offsets still work");
        Teleport(enemyBody.position + Vector2.right);
        await Delay(0.16f);
        Check(enemy.IsChasing, "Diagonal patrol can be interrupted");
        Teleport(new Vector2(30f, 0f));
        await Delay(0.24f);
        Check(!enemy.IsChasing && Mathf.Abs(enemyBody.position.x + enemyBody.position.y) < 0.001f,
            "Chase restores the original diagonal route without moving its endpoints");
        Teleport(enemyBody.position + Vector2.left);
        Set(enemy, "speed", 0f);
        Vector2 frozen = enemyBody.position;
        await Delay(0.1f);
        Check(!enemy.IsChasing && Vector2.Distance(enemyBody.position, frozen) < 0.001f, "Zero speed stops patrol and chase");
        Set(enemy, "speed", 1.5f); Set(enemy, "endOffset", Vector2.zero);
        await Delay(0.1f);
        Check(!enemy.IsChasing && Vector2.Distance(enemyBody.position, frozen) < 0.001f, "Zero patrol offset remains stationary");
        Clear();
    }

    private static async Task HeatAndBodyContact()
    {
        Setup("SlimeFire");
        WaterMeter water = player.GetComponent<WaterMeter>();
        KnockbackAbility knockback = player.GetComponent<KnockbackAbility>();
        knockback.enabled = true;
        Teleport(new Vector2(2.8f, 0.35f));
        await Delay(0.1f);
        Check(enemy.IsChasing && water.DrainMultiplier == 1f && water.CurrentWater == 100f && !knockback.IsFlashing,
            "Chase detection outside the heat/body area adds no damage or knockback");
        Teleport(enemyBody.position + new Vector2(1.2f, 0.35f));
        await Delay(0.08f);
        Check(water.DrainMultiplier == 2f && !knockback.IsFlashing,
            "Chasing Slime retains its separate heat area without a body hit");
        Teleport(enemyBody.position + new Vector2(0f, 0.35f));
        await Delay(0.08f);
        Check(water.CurrentWater == 90f && knockback.IsFlashing,
            "Actual chasing enemy body contact still applies one hit and flash");
        Teleport(new Vector2(30f, 0f));
        await Delay(0.08f);
        Check(!enemy.IsChasing && water.DrainMultiplier == 1f,
            "Leaving a chasing Slime clears chase and heat independently");
        Clear();
    }

    private static GameObject Load(string name) => AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/" + name + ".prefab");

    private static void Setup(string name)
    {
        if (name != "Saw")
        {
            var floor = new GameObject("Ground for walking chase regression");
            objects.Add(floor);
            floor.layer = LayerMask.NameToLayer("Ground-Dry");
            floor.transform.position = new Vector3(0f, -0.5f);
            floor.AddComponent<BoxCollider2D>().size = new Vector2(100f, 1f);
        }
        GameObject root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab"), new Vector3(30f, 0f), Quaternion.identity);
        objects.Add(root); player = root.GetComponent<Player>(); playerBody = root.GetComponent<Rigidbody2D>();
        playerBody.constraints = RigidbodyConstraints2D.FreezeAll;
        root.GetComponent<KnockbackAbility>().enabled = false;
        WaterMeter water = root.GetComponent<WaterMeter>();
        Set(water, "dryDrainPerSecond", 0f); Set(water, "wetDrainPerSecond", 0f);
        Set(water, "extraDryDrainPerSecond", 0f);
        Set(water, "dryIdleDrainPerSecond", 0f); Set(water, "wetIdleDrainPerSecond", 0f);
        Set(water, "extraDryIdleDrainPerSecond", 0f);
        root = Object.Instantiate(Load(name + "_Patrol"), Vector3.zero, Quaternion.identity);
        objects.Add(root); enemy = root.GetComponent<EnemyPatrol>(); enemyBody = root.GetComponent<Rigidbody2D>();
        HapsirikKozSneeze sneeze = root.GetComponent<HapsirikKozSneeze>();
        if (sneeze != null) Set(sneeze, "automaticSneezing", false);
        Physics2D.SyncTransforms();
    }

    private static void Teleport(Vector2 position)
    {
        playerBody.position = position;
        player.GetComponent<WaterMeter>().ResetPositionTracking();
        Physics2D.SyncTransforms();
    }

    private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    private static async Task Delay(float seconds)
    {
        float until = Time.time + seconds;
        double timeout = EditorApplication.timeSinceStartup + 20f;
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
        string result = error == null ? "PASS: Unity " + Application.unityVersion + "; " + passed.Count + " checks (" + assetChecks + " asset, " + (passed.Count - assetChecks) + " Play Mode). Isolated project; main level and exported build not tested."
            : "FAIL after " + passed.Count + " checks: " + error;
        File.WriteAllText("enemy-chase-result.txt", result);
        File.WriteAllLines("enemy-chase-checks.txt", passed);
        Debug.Log(result);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}
