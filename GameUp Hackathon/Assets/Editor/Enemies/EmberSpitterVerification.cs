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

// Run in a separate batch project: -executeMethod EmberSpitterVerification.GenerateAndRun.
// Does not close, play, or save the user's currently open project.
public static class EmberSpitterVerification
{
    private static readonly List<GameObject> objects = new List<GameObject>();
    private static readonly List<string> passed = new List<string>();
    private static readonly HashSet<EmberFireball> observed = new HashSet<EmberFireball>();
    private static Player player;
    private static WaterMeter water;
    private static Rigidbody2D body;
    private static EmberSpitter enemy;
    private static Keyboard keyboard;
    private static int assetChecks;

    public static void GenerateAndRun() => Run(true);
    public static void UpgradeAndRun() => Run(false);

    private static void Run(bool rebuild)
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use an isolated batch project.");
        try
        {
            if (rebuild)
            {
                EmberSpitterPrefabBuilder.RebuildAll();
                string guid = AssetDatabase.AssetPathToGUID(EmberSpitterPrefabBuilder.EnemyPath);
                EmberSpitterPrefabBuilder.RebuildPrefabs();
                Check(guid == AssetDatabase.AssetPathToGUID(EmberSpitterPrefabBuilder.EnemyPath), "Rebuilding keeps the enemy prefab GUID");
            }
            else
            {
                GameObject previous = AssetDatabase.LoadAssetAtPath<GameObject>(EmberSpitterPrefabBuilder.EnemyPath);
                string rootId = AssetId(previous);
                string behaviourId = AssetId(previous.GetComponent<EmberSpitter>());
                string mouthId = AssetId(previous.transform.Find("FirePointRight"));
                EmberSpitterPrefabBuilder.ConfigureIdleAndGravity();
                EmberSpitterPrefabBuilder.ConfigureIdleAndGravity();
                GameObject upgraded = AssetDatabase.LoadAssetAtPath<GameObject>(EmberSpitterPrefabBuilder.EnemyPath);
                Check(rootId == AssetId(upgraded) && behaviourId == AssetId(upgraded.GetComponent<EmberSpitter>()) &&
                    mouthId == AssetId(upgraded.transform.Find("FirePointRight")), "In-place upgrade preserves prefab GUID and existing root, behaviour and muzzle fileIDs");
            }
            CheckAssets();
            assetChecks = passed.Count;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.playModeStateChanged += EnteredPlay;
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Finish(error); }
    }

    private static void CheckAssets()
    {
        GameObject spitter = AssetDatabase.LoadAssetAtPath<GameObject>(EmberSpitterPrefabBuilder.EnemyPath);
        GameObject ball = AssetDatabase.LoadAssetAtPath<GameObject>(EmberSpitterPrefabBuilder.ProjectilePath);
        Check(spitter != null && ball != null && spitter.GetComponentsInChildren<Component>(true).All(c => c != null) &&
            ball.GetComponentsInChildren<Component>(true).All(c => c != null), "Both saved prefabs load without missing scripts");
        Rigidbody2D groundBody = spitter.GetComponent<Rigidbody2D>();
        Check(groundBody != null && groundBody.bodyType == RigidbodyType2D.Dynamic && groundBody.gravityScale == 1f &&
            groundBody.constraints == (RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation) &&
            groundBody.collisionDetectionMode == CollisionDetectionMode2D.Continuous,
            "Stationary ground body has gravity with horizontal position and rotation locked");
        Collider2D terrain = spitter.transform.Find("TerrainBody").GetComponent<Collider2D>();
        int groundMask = WorldMask | LayerMask.GetMask("EnemyBoundary");
        Check(spitter.GetComponentsInChildren<Collider2D>().Length == 1 && !terrain.isTrigger && terrain.includeLayers == groundMask &&
            terrain.excludeLayers == ~groundMask && terrain.sharedMaterial != null && terrain.sharedMaterial.friction == 0f &&
            spitter.GetComponentsInChildren<EnemyContactKnockback>().Length == 0 && spitter.GetComponentsInChildren<EnemyHeatArea>().Length == 0 &&
            spitter.GetComponentsInChildren<EnemyPatrol>().Length == 0, "Terrain-only solid matches ground enemies without contact damage, patrol or Player pushing");
        var settings = new SerializedObject(spitter.GetComponent<EmberSpitter>());
        Check(settings.FindProperty("projectilePrefab").objectReferenceValue == ball.GetComponent<EmberFireball>() &&
            settings.FindProperty("playerLayerMask").intValue == LayerMask.GetMask("Player") &&
            settings.FindProperty("obstacleLayerMask").intValue == WorldMask, "Projectile and Player/three ground masks are wired");
        Check(settings.FindProperty("detectionRadius").floatValue == 5f && settings.FindProperty("windupDuration").floatValue == 0.8f &&
            settings.FindProperty("cooldownDuration").floatValue == 2.5f, "Enemy initial tuning is 5 units / 0.8 s / 2.5 s");
        var projectileSettings = new SerializedObject(ball.GetComponent<EmberFireball>());
        Check(projectileSettings.FindProperty("speed").floatValue == 4f && projectileSettings.FindProperty("damageWaterPoints").floatValue == 10f &&
            projectileSettings.FindProperty("maxLifetime").floatValue == 4f, "Projectile initial tuning is 4 units/s / 10 points / 4 s");
        Check(ball.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Kinematic && ball.GetComponent<Rigidbody2D>().gravityScale == 0f &&
            ball.GetComponent<CircleCollider2D>().isTrigger && ball.GetComponent<CircleCollider2D>().radius == 0.13f,
            "Projectile uses a gravity-free kinematic body and a small round core");
        Sprite idle = spitter.GetComponentInChildren<SpriteRenderer>().sprite;
        Sprite fly = ball.GetComponentInChildren<SpriteRenderer>().sprite;
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(idle, out string idleGuid, out long idleId);
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(fly, out string flyGuid, out long flyId);
        Check(idleGuid == "b1326f211e6d4d56a836aa65da22ff49" && idleId == -8381659573352531839L &&
            flyGuid == "ae0a234135b87cd4a960d40229835fcd" && flyId == 798397748300462308L,
            "Prepared Idle sheet and projectile sprite keep their original GUIDs and fileIDs");
        Check(idle.pixelsPerUnit == 128f && fly.pixelsPerUnit == 128f && idle.rect.size == new Vector2(128f, 128f) &&
            fly.rect.size == new Vector2(91f, 63f), "Idle uses aligned full cells and projectile retains its crop at 128 PPU");
        Animator animator = spitter.GetComponentInChildren<Animator>();
        Check(animator != null && animator.runtimeAnimatorController != null && !animator.applyRootMotion && ball.GetComponentInChildren<Animator>() == null &&
            spitter.GetComponentsInChildren<SpriteRenderer>().Length == 1 && ball.GetComponentsInChildren<SpriteRenderer>().Length == 1,
            "Idle Animator is connected only to Visual; projectile and physics remain independent");
        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(EmberSpitterPrefabBuilder.IdleClipPath);
        ObjectReferenceKeyframe[] idleFrames = AnimationUtility.GetObjectReferenceCurve(idleClip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"));
        Check(idleFrames.Length == 12 && idleFrames.Select(key => ((Sprite)key.value).name).SequenceEqual(Enumerable.Range(0, 12).Select(i => "Idle_" + i.ToString("D2"))) &&
            idleClip.frameRate == 12f && Mathf.Abs(idleClip.length - 1f) < 0.001f && AnimationUtility.GetAnimationClipSettings(idleClip).loopTime &&
            AnimationUtility.GetCurveBindings(idleClip).Length == 0, "Idle clip has twelve ordered frames, a full one-second loop and no transform curves");
        Scene demo = EditorSceneManager.OpenScene(EmberSpitterPrefabBuilder.DemoPath, OpenSceneMode.Single);
        var roots = demo.GetRootGameObjects();
        Player drop = roots.SelectMany(root => root.GetComponentsInChildren<Player>()).Single();
        WaterBarUI hud = roots.SelectMany(root => root.GetComponentsInChildren<WaterBarUI>()).Single();
        Check(new SerializedObject(hud).FindProperty("waterMeter").objectReferenceValue == drop.GetComponent<WaterMeter>() &&
            roots.SelectMany(root => root.GetComponentsInChildren<EmberSpitter>()).Count() == 1, "Demo has one shooter and the real water HUD connected to its Player");
        Check(roots.Any(root => root.name == "Sight Wall - jump over") && roots.Any(root => root.name == "Thin Platform - swept hit") &&
            roots.Any(root => root.GetComponent<EmberSpitterDemoControls>() != null), "Saved demo includes cover, a thin platform, flat ground and restart controls");
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
            await AnimationAndGravity();
            await RangeAndCadence();
            await CoverAndCancellation();
            await FlightAndDamage();
            await SweptHits();
            await Lifecycle();
            await MovementAndEscape();
            await SavedDemo();
            Finish(null);
        }
        catch (Exception error) { Finish(error); }
    }

    private static async Task AnimationAndGravity()
    {
        foreach (string ground in new[] { "Ground-Dry", "Ground-Wet", "Ground-Extra-Dry" })
        {
            await Setup(new Vector2(7f, 0.51f));
            GameObject floor = objects.First(root => root.name == "Fixture block");
            floor.layer = LayerMask.NameToLayer(ground);
            Rigidbody2D falling = enemy.GetComponent<Rigidbody2D>();
            falling.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            falling.position = new Vector2(0f, 2f);
            Physics2D.SyncTransforms();
            await Delay(0.2f);
            Check(falling.position.y < 1.9f && falling.linearVelocity.y < -1f, "Spitter actually falls under gravity above " + ground);
            await Delay(0.85f);
            Check(Mathf.Abs(falling.position.y) < 0.03f && Mathf.Abs(falling.linearVelocity.y) < 0.1f && Mathf.Abs(falling.position.x) < 0.001f,
                "Spitter lands on " + ground + " without sinking, rotating or walking");
            if (ground == "Ground-Dry")
            {
                var seenFrames = new HashSet<string>();
                SpriteRenderer renderer = enemy.GetComponentInChildren<SpriteRenderer>();
                await Delay(2.2f, () => seenFrames.Add(renderer.sprite.name));
                Check(seenFrames.Count == 12 && enemy.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime > 2f,
                    "Real Animator displays all twelve Idle sprites and keeps looping without a target");
                Check(renderer.transform.localScale == Vector3.one * 1.75f && enemy.transform.localScale == Vector3.one,
                    "Idle animation leaves Visual scale and physical root scale intact");
            }
            Object.DestroyImmediate(floor);
            await Delay(0.3f);
            Check(falling.position.y < -0.2f, "Removing " + ground + " support makes the stationary enemy fall again");
        }
        await Setup(new Vector2(-2f, 0.51f)); enemy.enabled = false;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        Keys(Key.D); await Delay(0.7f); Keys();
        Check(body.position.x > 1f && water.CurrentWater == 100f && enemy.transform.position.x == 0f,
            "Player crosses the new body without physical pushing or contact water damage");
        await Setup(new Vector2(3f, 0.51f)); enemy.enabled = false;
        Shoot(new Vector2(-1f, 0.51f), 8f); await Delay(0.6f);
        Check(water.CurrentWater == 90f, "Projectile passes through its owner's new terrain body before hitting Player");
    }

    private static async Task RangeAndCadence()
    {
        await Setup(new Vector2(6f, 0.51f));
        await Delay(1f);
        Check(observed.Count == 0 && enemy.CurrentState == EmberSpitter.ShotState.Idle, "No shots outside detection range");
        Teleport(new Vector2(3f, 0.51f));
        await Delay(0.35f);
        Check(enemy.CurrentState == EmberSpitter.ShotState.Windup && observed.Count == 0, "Open line of sight starts a visible warning before spawning");
        Check(enemy.transform.localScale == Vector3.one && enemy.transform.rotation == Quaternion.identity,
            "Windup affects only Visual; stationary root and detection geometry stay fixed");
        await Delay(0.55f);
        Check(observed.Count == 1 && enemy.CurrentState == EmberSpitter.ShotState.Cooldown, "Exactly one projectile appears after the 0.8-second windup");
        await Delay(2.3f);
        Check(observed.Count == 1, "Cooldown does not overlap another shot");
        await Delay(1.1f);
        Check(observed.Count == 2, "Second cycle produces one new shot after cooldown and a new windup");
        Check(enemy.transform.position == Vector3.zero, "Shooter stays fixed throughout both cycles");
    }

    private static async Task CoverAndCancellation()
    {
        await Setup(new Vector2(3f, 0.51f));
        GameObject wall = Block(new Vector2(1.5f, 1f), new Vector2(0.2f, 2f));
        await Delay(1f);
        Check(observed.Count == 0 && enemy.CurrentState == EmberSpitter.ShotState.Idle, "A solid wall prevents starting windup");
        wall.GetComponent<Collider2D>().isTrigger = true;
        await Delay(0.15f);
        Check(enemy.CurrentState == EmberSpitter.ShotState.Windup, "Unrelated trigger on an obstacle layer does not block sight");
        wall.GetComponent<Collider2D>().isTrigger = false;
        await Delay(0.85f);
        Check(observed.Count == 0, "Hiding during windup cancels the pending shot");
        Object.DestroyImmediate(wall);
        await Delay(0.5f);
        Teleport(new Vector2(7f, 0.51f));
        await Delay(0.9f);
        Check(observed.Count == 0, "Leaving detection range during windup cancels the shot");

        foreach (float x in new[] { 0.3f, 0.64f, 0f })
        {
            await Setup(new Vector2(3f, 0.51f));
            Block(new Vector2(x, 0.9f), new Vector2(0.02f, 0.5f));
            Physics2D.queriesStartInColliders = false;
            await Delay(0.95f);
            Check(observed.Count == 0, "Muzzle clearance blocks a thin wall at x=" + x + " even with queriesStartInColliders disabled");
            Physics2D.queriesStartInColliders = true;
        }
        await Setup(new Vector2(-3f, 0.51f));
        await Delay(0.87f);
        EmberFireball ball = Balls().Single();
        Check(enemy.GetComponentInChildren<SpriteRenderer>().flipX && ball.GetComponent<Rigidbody2D>().position.x < -0.64f,
            "Left-facing shot exits the left mouth without rotating the body");
        await Setup(new Vector2(3f, 0.51f));
        await Delay(0.87f);
        Check(!enemy.GetComponentInChildren<SpriteRenderer>().flipX && Balls().Single().GetComponent<Rigidbody2D>().position.x > 0.64f,
            "Right-facing shot exits the right mouth");
    }

    private static async Task FlightAndDamage()
    {
        await Setup(new Vector2(4f, 2.5f)); enemy.enabled = false;
        Vector2 launchDirection = (body.worldCenterOfMass - new Vector2(0f, 2.5f)).normalized;
        EmberFireball ball = Shoot(new Vector2(0f, 2.5f));
        await Delay(0.12f);
        Vector2 before = ball.GetComponent<Rigidbody2D>().position;
        Teleport(new Vector2(4f, 4f));
        await Delay(0.2f);
        Vector2 after = ball.GetComponent<Rigidbody2D>().position;
        Vector2 travelled = after - before;
        Check(after.x > before.x + 0.6f && Vector2.Distance(travelled, launchDirection * Vector2.Dot(travelled, launchDirection)) < 0.001f,
            "Projectile keeps its launch direction after target changes position; no homing or gravity");
        float launchAngle = Mathf.Atan2(launchDirection.y, launchDirection.x) * Mathf.Rad2Deg;
        Check(Mathf.Abs(Mathf.DeltaAngle(ball.transform.Find("Visual").eulerAngles.z, launchAngle)) < 0.01f,
            "Supplied +X sprite rotates to the actual Rigidbody centre, including the player's collider offset");
        Object.DestroyImmediate(ball.gameObject);
        foreach (Vector2 aim in new[] { Vector2.up, Vector2.down, Vector2.left })
        {
            Teleport(new Vector2(0f, 3f) + aim * 3f);
            ball = Shoot(new Vector2(0f, 3f));
            Vector2 actualAim = body.worldCenterOfMass - new Vector2(0f, 3f);
            float expected = Mathf.Atan2(actualAim.y, actualAim.x) * Mathf.Rad2Deg;
            Check(Mathf.Abs(Mathf.DeltaAngle(ball.transform.Find("Visual").eulerAngles.z, expected)) < 0.01f,
                "Visual rotates around the round core toward " + aim);
            Object.DestroyImmediate(ball.gameObject);
        }

        await Setup(new Vector2(3f, 0.51f)); enemy.enabled = false;
        water.Consume(40f);
        var extra = new GameObject("Second player collider"); extra.layer = LayerMask.NameToLayer("Player");
        extra.transform.SetParent(player.transform, false); extra.AddComponent<CircleCollider2D>().radius = 0.3f;
        Shoot(new Vector2(0f, 0.51f)); Shoot(new Vector2(0f, 0.51f));
        await Delay(0.85f);
        Check(Mathf.Abs(water.CurrentWater - 50f) < 0.001f && Balls().Length == 0,
            "Two simultaneous shots and multiple Player colliders lose exactly 10 fixed points from 60, consuming both shots");
        KnockbackAbility protection = player.GetComponent<KnockbackAbility>();
        Check(protection.IsFlashing && !protection.TryKnockback(Vector2.zero), "Projectile uses the existing flash immunity and blocks a new body hit");
        Shoot(new Vector2(2.4f, 0.51f)); await Delay(0.15f);
        Check(water.CurrentWater == 50f && Balls().Length == 0, "Projectile hitting an invulnerable player is still destroyed without damage");
        await Setup(new Vector2(3f, 0.51f)); enemy.enabled = false;
        protection = player.GetComponent<KnockbackAbility>();
        Check(protection.TryKnockback(Vector2.zero), "Existing contact-hit path still accepts its first hit");
        float contactWater = water.CurrentWater;
        Shoot(new Vector2(2.4f, 0.51f)); await Delay(0.15f);
        Check(water.CurrentWater == contactWater && Balls().Length == 0, "Body-hit immunity also rejects and consumes an incoming projectile");
    }

    private static async Task SweptHits()
    {
        foreach (float step in new[] { 0.01f, 0.02f, 0.033333f })
        {
            Time.fixedDeltaTime = step;
            await Setup(new Vector2(3f, 0.51f)); enemy.enabled = false;
            Block(new Vector2(1.4f, 0.51f), new Vector2(0.02f, 1f));
            Shoot(new Vector2(0f, 0.51f), 300f);
            await Delay(0.1f);
            Check(Balls().Length == 0 && water.CurrentWater == 100f, "300-unit/s shot stops at a 0.02-unit wall at fixed step " + step);
        }
        Time.fixedDeltaTime = 0.02f;
        await Setup(new Vector2(0f, 0.51f)); enemy.enabled = false;
        Block(new Vector2(0f, 1.8f), new Vector2(2f, 0.02f));
        Shoot(new Vector2(0f, 4f), 300f);
        await Delay(0.1f);
        Check(Balls().Length == 0 && water.CurrentWater == 100f, "Vertical high-speed shot stops at the thin horizontal platform");
        await Setup(new Vector2(3f, 0.51f)); enemy.enabled = false;
        GameObject decoration = Block(new Vector2(1.4f, 0.51f), new Vector2(0.1f, 1f));
        decoration.GetComponent<Collider2D>().isTrigger = true;
        Shoot(new Vector2(0f, 0.51f), 50f);
        await Delay(0.15f);
        Check(water.CurrentWater == 90f, "Unrelated trigger neither absorbs a shot nor blocks its damage");
        await Setup(new Vector2(7f, 3f)); enemy.enabled = false;
        EmberFireball ball = SpawnBall(new Vector2(0f, 3f)); Set(ball, "maxLifetime", 0.12f); Launch(ball);
        await Delay(0.2f);
        Check(Balls().Length == 0, "A projectile with no collision is cleaned up by maxLifetime");
        await Setup(new Vector2(3f, 0.51f)); enemy.enabled = false;
        Block(new Vector2(2.2f, 0.51f), new Vector2(0.05f, 1f));
        Shoot(new Vector2(0f, 0.51f), 300f); await Delay(0.1f);
        Check(water.CurrentWater == 100f && Balls().Length == 0, "Nearest world hit wins over a Player hit in the same swept movement");
    }

    private static async Task Lifecycle()
    {
        await Setup(new Vector2(3f, 0.51f));
        await Delay(0.4f);
        EmberFireball old = Shoot(new Vector2(-3f, 3f));
        water.RespawnAtCheckpoint();
        Check(!old.GetComponent<Rigidbody2D>().simulated && enemy.CurrentState == EmberSpitter.ShotState.Idle,
            "Respawn immediately disarms an old projectile and cancels windup through the Player stop event");
        await Delay(0.55f);
        Check(Balls().Length == 0 && !water.IsRespawning && water.CurrentWater == 100f, "Respawn restores the existing water/control flow before any fresh full windup fires");
        old = Shoot(new Vector2(-3f, 3f));
        player.enabled = false; player.enabled = true;
        Check(!old.GetComponent<Rigidbody2D>().simulated, "Even same-frame Player disable/enable invalidates old shots");
        await Setup(new Vector2(3f, 0.51f));
        await Delay(0.35f);
        for (int i = 0; i < 10; i++) { enemy.enabled = false; enemy.enabled = true; }
        await Delay(0.87f);
        Check(observed.Count == 1 && Balls().Length == 1, "Repeated shooter disable/enable starts one fresh cycle without duplicate timers");
        enemy.enabled = false; await Delay(0.05f);
        Check(Balls().Length == 0 && enemy.transform.Find("Visual").localScale == Vector3.one * 1.75f,
            "Disabling shooter cleans owned projectiles and resets the warning visual");
        await Setup(new Vector2(3f, 0.51f));
        await Delay(0.87f);
        player.enabled = false; body.simulated = false;
        await Delay(0.9f);
        Check(Balls().Length == 0 && observed.Count == 1 && water.CurrentWater == 100f,
            "Level-exit control/physics freeze prevents new shots and damage and clears active shots");
        await Setup(new Vector2(3f, 0.51f)); enemy.enabled = false;
        water.Consume(95f); int depleted = 0; water.Depleted += () => depleted++;
        Shoot(new Vector2(2.4f, 0.51f)); Shoot(new Vector2(2.4f, 0.51f));
        await Delay(0.1f);
        Check(water.IsRespawning && water.CurrentWater == 0f && depleted == 1 && Balls().Length == 0,
            "Two lethal contacts clamp water at zero and enter exactly one existing death flow");
        await Delay(0.4f);
        Check(!water.IsRespawning && water.CurrentWater == 100f && player.isActiveAndEnabled && body.simulated,
            "Lethal projectile respawn restores full water and controls without old shots");
        await Setup(new Vector2(3f, 0.51f)); enemy.enabled = false;
        Shoot(new Vector2(-3f, 3f)); Object.DestroyImmediate(player.gameObject);
        await Delay(0.06f);
        Check(Balls().Length == 0, "Destroyed target references retire safely");
    }

    private static async Task MovementAndEscape()
    {
        await Setup(new Vector2(3f, 0.51f)); enemy.enabled = false;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        Set(water, "dryDrainPerSecond", 3f); Set(water, "dryIdleDrainPerSecond", 0.5f);
        await Delay(0.15f); float before = water.CurrentWater;
        await Delay(0.2f);
        Check(water.CurrentWater < before, "Existing positive idle consumption still works");
        Keys(Key.D); float startX = body.position.x;
        await Delay(0.3f);
        Check(body.position.x > startX + 0.6f && player.CurrentState == PlayerStates.State.Move, "Real input still drives the original Player movement");
        Keys(Key.D, Key.Space); await Delay(0.12f); Keys(Key.D);
        Check(body.position.y > 0.8f && player.CurrentState == PlayerStates.State.Jump, "Real Space input still jumps and releases through the existing ability");
        await Setup(new Vector2(3f, 0.51f));
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        await Delay(0.86f);
        Check(Balls().Length == 1, "Escape trial begins with the default shot after its readable warning");
        Keys(Key.Space); await Delay(0.15f);
        Check(body.position.y > 0.8f && player.CurrentState == PlayerStates.State.Jump, "Escape input actually puts the Player above the incoming shot");
        await Delay(0.45f); Keys(); await Delay(0.35f);
        Check(water.CurrentWater == 100f,
            "At 3 units, holding a normal jump after the warned default shot dodges its 4-unit/s flight; water=" + water.CurrentWater);
    }

    private static async Task SavedDemo()
    {
        await Clear();
        Scene demo = EditorSceneManager.LoadSceneInPlayMode(EmberSpitterPrefabBuilder.DemoPath, new LoadSceneParameters(LoadSceneMode.Additive));
        await Delay(0.2f);
        SceneManager.SetActiveScene(demo);
        var roots = demo.GetRootGameObjects();
        player = roots.SelectMany(root => root.GetComponentsInChildren<Player>()).Single();
        water = player.GetComponent<WaterMeter>(); body = player.GetComponent<Rigidbody2D>();
        Check(player.IsGrounded && body.position.x < -6.6f && Balls().Length == 0 && water.CurrentWater > 99f,
            "Actual saved demo starts grounded outside range with normal water consumption");
        Teleport(new Vector2(0f, 0.51f)); await Delay(1.4f);
        Check(water.CurrentWater < 90f, "Actual demo prefab fires and damages its actual Player through the water system");
        WaterBarUI hud = roots.SelectMany(root => root.GetComponentsInChildren<WaterBarUI>()).Single();
        RectTransform fill = (RectTransform)new SerializedObject(hud).FindProperty("fill").objectReferenceValue;
        Check(Mathf.Abs(fill.anchorMax.x - water.NormalizedWater) < 0.001f, "Actual demo water bar reflects projectile damage immediately");
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
        await Delay(0.08f); Keys(); await Delay(0.45f);
        Check(!water.IsRespawning && player.isActiveAndEnabled && body.position.x < -6.6f && water.CurrentWater > 99f && Balls().Length == 0,
            "Actual demo R key returns to the start and clears previous-attempt shots");
        Camera camera = roots.Select(root => root.GetComponent<Camera>()).First(c => c != null);
        var texture = new RenderTexture(1280, 720, 24);
        var capture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        RenderTexture previous = RenderTexture.active;
        try
        {
            camera.targetTexture = texture; camera.Render(); RenderTexture.active = texture;
            capture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); capture.Apply();
            File.WriteAllBytes("ember-spitter-demo.png", capture.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = null; RenderTexture.active = previous;
            Object.DestroyImmediate(texture); Object.DestroyImmediate(capture);
        }
        Check(new FileInfo("ember-spitter-demo.png").Length > 10000, "Demo camera rendered a 1280x720 verification capture");
    }

    private static int WorldMask => LayerMask.GetMask("Ground-Dry", "Ground-Wet", "Ground-Extra-Dry");
    private static EmberFireball[] Balls() => Object.FindObjectsByType<EmberFireball>();

    private static async Task Setup(Vector2 position)
    {
        await Clear();
        Block(new Vector2(0f, -0.5f), new Vector2(40f, 1f));
        GameObject root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab"), position, Quaternion.identity);
        objects.Add(root); player = root.GetComponent<Player>(); water = root.GetComponent<WaterMeter>(); body = root.GetComponent<Rigidbody2D>();
        Set(player.GetComponent<PhysicsControl>(), "extraDryGroundLayer", (LayerMask)LayerMask.GetMask("Ground-Extra-Dry"));
        body.constraints = RigidbodyConstraints2D.FreezeAll;
        foreach (string field in new[] { "dryDrainPerSecond", "wetDrainPerSecond", "extraDryDrainPerSecond", "dryIdleDrainPerSecond", "wetIdleDrainPerSecond", "extraDryIdleDrainPerSecond" }) Set(water, field, 0f);
        enemy = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(EmberSpitterPrefabBuilder.EnemyPath), Vector3.zero, Quaternion.identity).GetComponent<EmberSpitter>();
        objects.Add(enemy.gameObject);
        // Fixed obstruction fixtures intentionally intersect the source with walls. Actual gravity is tested separately above.
        enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        Physics2D.SyncTransforms();
    }

    private static GameObject Block(Vector2 position, Vector2 size)
    {
        var root = new GameObject("Fixture block"); objects.Add(root);
        root.layer = LayerMask.NameToLayer("Ground-Dry"); root.transform.position = position;
        root.AddComponent<BoxCollider2D>().size = size;
        Physics2D.SyncTransforms(); return root;
    }

    private static EmberFireball SpawnBall(Vector2 position)
    {
        EmberFireball ball = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(EmberSpitterPrefabBuilder.ProjectilePath), position, Quaternion.identity).GetComponent<EmberFireball>();
        objects.Add(ball.gameObject); return ball;
    }
    private static EmberFireball Shoot(Vector2 position, float speed = 4f)
    {
        EmberFireball ball = SpawnBall(position); Set(ball, "speed", speed); Launch(ball); return ball;
    }
    private static void Launch(EmberFireball ball) => ball.Launch(body.worldCenterOfMass, player, enemy.transform, LayerMask.GetMask("Player"), WorldMask);
    private static void Teleport(Vector2 position) { body.position = position; player.GetComponent<PhysicsControl>().ResetMotion(); water.ResetPositionTracking(); Physics2D.SyncTransforms(); }
    private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    private static void Keys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); InputSystem.Update(); }

    private static async Task Delay(float seconds, Action sample = null)
    {
        float end = Time.time + seconds;
        double deadline = EditorApplication.timeSinceStartup + 30f;
        while (Time.time < end)
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Play Mode did not advance");
            foreach (EmberFireball ball in Balls()) observed.Add(ball);
            sample?.Invoke();
            await Task.Delay(2);
        }
        foreach (EmberFireball ball in Balls()) observed.Add(ball);
        sample?.Invoke();
    }
    private static string AssetId(Object asset)
    {
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long fileId);
        return guid + ":" + fileId;
    }
    private static async Task Clear()
    {
        if (keyboard != null) Keys();
        foreach (GameObject root in objects) if (root != null) Object.DestroyImmediate(root);
        objects.Clear();
        foreach (EmberFireball ball in Balls()) Object.DestroyImmediate(ball.gameObject);
        await Delay(0.04f); observed.Clear();
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        passed.Add(message); Debug.Log("SPITTER_PASS " + message);
    }
    private static void Finish(Exception error)
    {
        Time.timeScale = 1f; Time.fixedDeltaTime = 0.02f; Physics2D.queriesStartInColliders = true;
        string report = error == null ? "PASS: Unity " + Application.unityVersion + "; " + passed.Count + " checks (" + assetChecks + " asset, " + (passed.Count - assetChecks) + " Play Mode). Isolated project using current Player, scripts, input, sprites and prefabs. Main levels and exported build not tested."
            : "FAIL after " + passed.Count + " checks: " + error;
        File.WriteAllText("ember-spitter-result.txt", report);
        File.WriteAllLines("ember-spitter-checks.txt", passed);
        Debug.Log("SPITTER_RESULT " + report);
        if (error != null) Debug.LogException(error);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}
