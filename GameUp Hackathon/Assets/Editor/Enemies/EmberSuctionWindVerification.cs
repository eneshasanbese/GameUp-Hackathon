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

// Explicitly run in an isolated batch project, never in the user's open editor.
public static class EmberSuctionWindVerification
{
    private static readonly List<string> passed = new List<string>();
    private static int assetChecks;
    private static bool includeHeatAura;
    private static GameObject furnace;
    private static EmberSuctionEnemy suction;
    private static EmberSuctionWindVisual wind;
    private static SpriteRenderer renderer;
    private static Animator animator;
    private static Player player;
    private static WaterMeter water;
    private static Rigidbody2D body;

    public static void GenerateHeatAuraAndRun()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use a separate batch project.");
        try
        {
            includeHeatAura = true;
            GameObject original = AssetDatabase.LoadAssetAtPath<GameObject>(EmberSuctionPrefabBuilder.PrefabPath);
            float[] before = GameplaySettings(original);
            string guid = AssetDatabase.AssetPathToGUID(EmberSuctionPrefabBuilder.PrefabPath);
            EmberSuctionHeatAuraBuilder.Rebuild();
            EmberSuctionHeatAuraBuilder.Rebuild();
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EmberSuctionPrefabBuilder.PrefabPath);
            Transform aura = prefab.transform.Find("EffectArea/HeatAura");
            Transform template = AssetDatabase.LoadAssetAtPath<GameObject>(EmberSuctionHeatAuraBuilder.SlimePath).transform.Find("HeatArea/HeatAura");
            Check(GameplaySettings(prefab).SequenceEqual(before) && AssetDatabase.AssetPathToGUID(EmberSuctionPrefabBuilder.PrefabPath) == guid,
                "Heat integration preserves the tuned suction/water values, collider radius and existing prefab GUID");
            Check(prefab.GetComponentsInChildren<EnemyHeatAuraVisual>().Length == 1 && prefab.GetComponentsInChildren<EnemyHeatArea>().Length == 0,
                "Repeated heat rebuild adds one visual without introducing another heat damage source");
            Check(aura.GetComponent<Animator>().runtimeAnimatorController == template.GetComponent<Animator>().runtimeAnimatorController &&
                aura.GetComponent<SpriteRenderer>().sprite == template.GetComponent<SpriteRenderer>().sprite,
                "Furnace reuses SlimeFire's exact existing HeatAura controller, clip and sprite");
            var visual = new SerializedObject(aura.GetComponent<EnemyHeatAuraVisual>());
            Check(visual.FindProperty("suctionSource").objectReferenceValue == prefab.GetComponentInChildren<EmberSuctionEnemy>() &&
                visual.FindProperty("heatArea").objectReferenceValue == null &&
                visual.FindProperty("heatCollider").objectReferenceValue == prefab.GetComponentInChildren<CircleCollider2D>(),
                "Continuous heat visual is wired to the original suction component and trigger");
            Check(aura.GetComponent<SpriteRenderer>().sortingOrder < prefab.transform.Find("SuctionWind").GetComponent<SpriteRenderer>().sortingOrder,
                "Heat is sorted below the wind and furnace body");
            AnimationClip clip = aura.GetComponent<Animator>().runtimeAnimatorController.animationClips.Single();
            Check(clip.isLooping && clip.frameRate == 12f && Mathf.Abs(clip.length - 2f) < 0.001f,
                "Shared heat animation remains the original 12 FPS, two-second continuous loop");
            GenerateAndRun();
        }
        catch (Exception error) { Finish(error); }
    }

    public static void GenerateAndRun()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Use a separate batch project.");
        try
        {
            GameObject original = AssetDatabase.LoadAssetAtPath<GameObject>(EmberSuctionPrefabBuilder.PrefabPath);
            float[] before = GameplaySettings(original);
            string prefabGuid = AssetDatabase.AssetPathToGUID(EmberSuctionPrefabBuilder.PrefabPath);
            EmberSuctionWindBuilder.Rebuild();
            string controllerGuid = AssetDatabase.AssetPathToGUID(EmberSuctionWindBuilder.ControllerPath);
            EmberSuctionWindBuilder.Rebuild();
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EmberSuctionPrefabBuilder.PrefabPath);
            Check(GameplaySettings(prefab).SequenceEqual(before) && AssetDatabase.AssetPathToGUID(EmberSuctionPrefabBuilder.PrefabPath) == prefabGuid,
                "Wind integration preserves the existing prefab GUID, trigger radius and all tuned gameplay values");
            Check(AssetDatabase.AssetPathToGUID(EmberSuctionWindBuilder.ControllerPath) == controllerGuid &&
                prefab.GetComponentsInChildren<EmberSuctionWindVisual>().Length == 1, "Repeated wind rebuild keeps asset identity and one wind child");
            Check(prefab.GetComponentsInChildren<Component>(true).All(component => component != null) && prefab.GetComponentsInChildren<Collider2D>().Length == 1 &&
                prefab.GetComponentsInChildren<Rigidbody2D>().Length == 0, "Saved prefab has no missing scripts or added physics bodies/colliders");
            var windSettings = new SerializedObject(prefab.GetComponentInChildren<EmberSuctionWindVisual>());
            Check(windSettings.FindProperty("suction").objectReferenceValue == prefab.GetComponentInChildren<EmberSuctionEnemy>(), "Saved visual references the existing suction source");
            SpriteRenderer savedRenderer = prefab.transform.Find("SuctionWind").GetComponent<SpriteRenderer>();
            SpriteRenderer furnaceRenderer = prefab.transform.Find("Visual").GetComponent<SpriteRenderer>();
            Check(savedRenderer.sortingLayerID == furnaceRenderer.sortingLayerID && savedRenderer.sortingOrder < furnaceRenderer.sortingOrder,
                "Wind is sorted behind the furnace mouth and the Player");
            foreach (var definition in new[] { (name: "IdleWind", fps: 12f), (name: "PullWind", fps: 16f), (name: "PullWindStrong", fps: 24f) })
            {
                string sheet = EmberSuctionWindBuilder.SheetFolder + definition.name + ".png";
                var importer = (TextureImporter)AssetImporter.GetAtPath(sheet);
                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(sheet).OfType<Sprite>().ToArray();
                Check(sprites.Length == 24 && sprites.All(sprite => sprite.rect.size == new Vector2(256f, 256f) && sprite.pixelsPerUnit == 256f &&
                    Vector2.Distance(sprite.pivot / 256f, new Vector2(0.14f, 0.5f)) < 0.001f) && !importer.mipmapEnabled &&
                    importer.filterMode == FilterMode.Bilinear && importer.textureCompression == TextureImporterCompression.Uncompressed,
                    definition.name + " imports all 24 full-size frames at the shared mouth pivot without compression or mipmaps");
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(EmberSuctionWindBuilder.AnimationFolder + "KozOcagi" + definition.name + ".anim");
                Check(Mathf.Abs(clip.length - 24f / definition.fps) < 0.001f && clip.isLooping && clip.frameRate == definition.fps,
                    definition.name + " has the authored FPS and full loop duration, including the last frame");
            }
            assetChecks = passed.Count;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            EditorApplication.playModeStateChanged += EnteredPlay;
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Finish(error); }
    }

    private static async void EnteredPlay(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.EnteredPlayMode) return;
        try
        {
            Application.targetFrameRate = 120;
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            furnace = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(EmberSuctionPrefabBuilder.PrefabPath));
            suction = furnace.GetComponentInChildren<EmberSuctionEnemy>();
            wind = furnace.GetComponentInChildren<EmberSuctionWindVisual>();
            renderer = wind.GetComponent<SpriteRenderer>(); animator = wind.GetComponent<Animator>();
            GameObject drop = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab"), new Vector3(8f, 0.51f), Quaternion.identity);
            player = drop.GetComponent<Player>(); water = drop.GetComponent<WaterMeter>(); body = drop.GetComponent<Rigidbody2D>();
            body.constraints = RigidbodyConstraints2D.FreezeAll;
            foreach (string field in new[] { "dryDrainPerSecond", "wetDrainPerSecond", "extraDryDrainPerSecond", "dryIdleDrainPerSecond", "wetIdleDrainPerSecond", "extraDryIdleDrainPerSecond" }) Set(water, field, 0f);
            await Delay(0.6f);
            Check(IsClip("IdleWind") && renderer.color.a > 0.3f && !suction.TryGetWindTarget(out _, out _, out _), "Outside range only the small ambient wind loops");
            Vector3 idleScale = wind.transform.lossyScale;
            float outsideWater = water.CurrentWater;
            await Delay(0.2f);
            Check(Mathf.Approximately(outsideWater, water.CurrentWater), "Ambient wind causes no water loss outside suction range");

            if (includeHeatAura) await CheckContinuousHeatAura();

            Teleport(4f); await Delay(0.5f);
            Check(IsClip("PullWind") && renderer.color.a > 0.4f && suction.TryGetWindTarget(out _, out _, out _), "Real suction activation starts the normal wind loop and fades it in");
            Check(EndpointMatches() && Vector3.Distance(wind.transform.position, suction.MouthPosition) < 0.001f && wind.transform.right.x > 0.99f,
                "Right-side flow spans the horizontal mouth-to-player distance with its pivot fixed inside the mouth");
            string firstFrame = renderer.sprite.name;
            await Delay(0.14f);
            Check(renderer.sprite.name != firstFrame && renderer.sprite.name.StartsWith("PullWind_"), "Animator advances the supplied sprite frames during suction");
            float cycle = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            await Delay(1.7f);
            Check(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > cycle + 1f && renderer.color.a > 0.4f, "Wind continues across a full loop while the player stays in range");
            Capture("suction-wind-right.png");

            Teleport(1.5f); await Delay(0.3f);
            Check(IsClip("PullWindStrong") && renderer.color.a > 0.75f && EndpointMatches(), "Close range uses the denser, faster strong wind with the same mouth alignment");
            Teleport(2.55f); await Delay(0.08f);
            Check(IsClip("PullWindStrong"), "Strong-wind hysteresis avoids flicker around the proximity threshold");
            Teleport(-4f); await Delay(0.15f);
            Check(IsClip("PullWind") && wind.transform.right.x < -0.99f && EndpointMatches(), "Crossing the furnace reverses inward flow about the mouth without moving the pivot");
            Teleport(-1.5f); await Delay(0.25f);
            Capture("suction-wind-left-strong.png");
            Check(furnace.transform.position == Vector3.zero && furnace.transform.rotation == Quaternion.identity && furnace.transform.localScale == Vector3.one,
                "All wind scaling and rotation leave the stationary furnace root unchanged");

            Teleport(-4f); await Delay(0.1f);
            furnace.transform.localScale = new Vector3(2f, 2f, 1f); await Delay(0.12f);
            Check(EndpointMatches(), "Wind still spans world-space distance when the furnace is scaled");
            furnace.transform.localScale = Vector3.one;
            Teleport(0.01f); await Delay(0.6f);
            Check(IsClip("IdleWind"), "The horizontal no-pull zone does not display a false full-strength wind beam");

            Teleport(4f); await Delay(0.4f);
            float beforeExit = renderer.color.a;
            Teleport(8f); await Delay(0.1f);
            Check(IsClip("PullWind") && renderer.color.a < beforeExit && renderer.color.a > 0f, "Range exit fades the running loop from its current phase instead of jumping to FadeOut_00");
            Teleport(4f); await Delay(0.12f);
            Check(IsClip("PullWind") && EndpointMatches() && renderer.color.a > 0f && suction.TryGetWindTarget(out _, out _, out float reentryRamp) && reentryRamp > 0f,
                "Re-entry during fade immediately reconnects the current wind and resumes the source activation ramp");
            Teleport(8f); await Delay(0.6f);
            Check(IsClip("IdleWind") && Vector3.Distance(wind.transform.lossyScale, idleScale) < 0.001f, "Completed exit fade returns to a small ambient effect without leaving a stretched beam");

            Teleport(4f); await Delay(0.4f);
            suction.GetComponent<CircleCollider2D>().enabled = false; await Delay(0.06f);
            Check(!renderer.enabled, "Disabling the suction trigger hides all wind");
            suction.GetComponent<CircleCollider2D>().enabled = true; await Delay(0.4f);
            Check(renderer.enabled && IsClip("PullWind"), "Re-enabling the trigger restores wind from actual suction state");
            suction.enabled = false; await Delay(0.06f);
            Check(!renderer.enabled, "Disabling the source hides wind and clears old targets");
            suction.enabled = true; await Delay(0.4f);
            player.enabled = false; await Delay(0.6f);
            Check(IsClip("IdleWind") && !suction.TryGetWindTarget(out _, out _, out _), "Disabled player releases the active wind and leaves only ambient motion");
            player.enabled = true; await Delay(0.4f);
            body.simulated = false; await Delay(0.6f);
            Check(IsClip("IdleWind"), "Suspending player physics does not leave stale active wind");
            body.simulated = true; await Delay(0.4f);

            float time = animator.GetCurrentAnimatorStateInfo(0).normalizedTime, alpha = renderer.color.a;
            Time.timeScale = 0f; await Task.Delay(180);
            Check(Mathf.Abs(animator.GetCurrentAnimatorStateInfo(0).normalizedTime - time) < 0.01f && Mathf.Abs(renderer.color.a - alpha) < 0.01f,
                "Pause freezes both animation phase and wind opacity");
            Time.timeScale = 1f; await Delay(0.1f);
            float waterBefore = water.CurrentWater, start = Time.time;
            await Delay(0.5f);
            float visibleRate = (waterBefore - water.CurrentWater) / (Time.time - start);
            wind.enabled = false; waterBefore = water.CurrentWater; start = Time.time;
            await Delay(0.5f);
            float hiddenRate = (waterBefore - water.CurrentWater) / (Time.time - start);
            Check(!renderer.enabled && hiddenRate > 1f && Mathf.Abs(visibleRate - hiddenRate) < 0.2f && suction.TryGetWindTarget(out _, out _, out _),
                "Disabling only the visual leaves the independent water loss and real suction target active");
            wind.enabled = true; await Delay(0.3f);
            furnace.SetActive(false); await Delay(0.05f);
            Check(!renderer.enabled, "Disabling the furnace leaves no visible wind");
            furnace.SetActive(true); await Delay(0.4f);
            Check(renderer.enabled && IsClip("PullWind"), "Reactivated furnace starts a fresh wind cycle without stale fade state");
            Finish(null);
        }
        catch (Exception error) { Finish(error); }
    }

    private static float[] GameplaySettings(GameObject root)
    {
        var settings = new SerializedObject(root.GetComponentInChildren<EmberSuctionEnemy>());
        return new[] { "effectRadius", "innerFullEffectRadius", "minWaterLossPerSecond", "maxWaterLossPerSecond", "minPullStrength", "maxPullStrength", "activationRampDuration", "horizontalDeadZone" }
            .Select(name => settings.FindProperty(name).floatValue).Append(root.GetComponentInChildren<CircleCollider2D>().radius).ToArray();
    }

    private static async Task CheckContinuousHeatAura()
    {
        Transform aura = furnace.transform.Find("EffectArea/HeatAura");
        SpriteRenderer heat = aura.GetComponent<SpriteRenderer>();
        Animator heatAnimator = aura.GetComponent<Animator>();
        Check(heat.enabled && heat.gameObject.activeInHierarchy && heat.color.a > 0.2f,
            "Heat remains continuously visible while the Player is outside suction range");
        string frame = heat.sprite.name;
        await Delay(0.14f);
        Check(heat.sprite.name != frame, "Continuous heat advances the existing sprite animation with no nearby target");
        float cycle = heatAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        await Delay(2.1f);
        Check(heatAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > cycle + 1f && heat.enabled,
            "Heat loops continuously across its full two-second duration outside the player activation range");
        Capture("suction-heat-aura.png", 7f);

        Set(suction, "effectRadius", 3f); await Delay(0.08f);
        Check(Mathf.Abs(aura.lossyScale.x * (202f / 256f) - 6f) < 0.01f,
            "Heat visible diameter follows the actual configured suction range");
        Set(suction, "effectRadius", 6f);
        furnace.transform.localScale = Vector3.one * 2f; await Delay(0.08f);
        Check(Mathf.Abs(aura.lossyScale.x * (202f / 256f) - 12f) < 0.01f,
            "Scaling the furnace preserves the heat source's configured world-space radius");
        furnace.transform.localScale = Vector3.one;
        Transform origin = furnace.transform.Find("RangeOrigin"); Vector3 savedPosition = origin.position;
        origin.position += new Vector3(0.4f, 0.3f); await Delay(0.08f);
        Check(Vector3.Distance(aura.position, origin.position) < 0.001f, "Heat centre follows the actual range-origin offset");
        origin.position = savedPosition;
        suction.GetComponent<CircleCollider2D>().enabled = false; await Delay(0.06f);
        Check(!heat.enabled, "Disabling the suction trigger also hides its heat visual");
        suction.GetComponent<CircleCollider2D>().enabled = true; suction.enabled = false; await Delay(0.06f);
        Check(!heat.enabled, "Disabling the source hides heat even if its trigger still exists");
        suction.enabled = true; await Delay(0.08f);
        Check(heat.enabled, "Re-enabling the source restores continuous heat without needing a Player in range");

        GameObject slime = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(EmberSuctionHeatAuraBuilder.SlimePath), new Vector3(100f, 0f), Quaternion.identity);
        slime.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        EnemyHeatArea slimeSource = slime.GetComponentInChildren<EnemyHeatArea>();
        SpriteRenderer slimeHeat = slime.transform.Find("HeatArea/HeatAura").GetComponent<SpriteRenderer>();
        await Delay(0.08f);
        Check(slimeHeat.enabled, "The shared visual still displays SlimeFire's original heat-source path");
        slimeSource.enabled = false; await Delay(0.06f);
        Check(!slimeHeat.enabled, "SlimeFire's heat-source disable still hides its aura after the shared-component extension");
        slimeSource.enabled = true; await Delay(0.06f);
        Check(slimeHeat.enabled, "SlimeFire's original aura restores correctly when its heat source is enabled again");
        Object.DestroyImmediate(slime);

        Teleport(4f); await Delay(0.5f);
        Check(heat.enabled && IsClip("PullWind"), "Continuous heat and directional suction wind run together in range");
        float before = water.CurrentWater, start = Time.time;
        await Delay(0.5f); float visibleRate = (before - water.CurrentWater) / (Time.time - start);
        aura.gameObject.SetActive(false); before = water.CurrentWater; start = Time.time;
        await Delay(0.5f); float hiddenRate = (before - water.CurrentWater) / (Time.time - start);
        Check(hiddenRate > 1f && Mathf.Abs(visibleRate - hiddenRate) < 0.2f,
            "Adding or hiding the heat visual leaves the original absolute water-loss rate unchanged");
        aura.gameObject.SetActive(true); Teleport(8f); water.Refill(); await Delay(0.6f);
    }

    private static bool IsClip(string name) => animator.GetCurrentAnimatorStateInfo(0).IsName(name);
    private static bool EndpointMatches() => Mathf.Abs(wind.transform.TransformPoint(new Vector3(0.758f, 0f)).x - body.worldCenterOfMass.x) < 0.015f;
    private static void Teleport(float x)
    {
        body.position = new Vector2(x, 0.51f); player.GetComponent<PhysicsControl>().ResetMotion();
        water.ResetPositionTracking(); Physics2D.SyncTransforms();
    }
    private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    private static async Task Delay(float seconds)
    {
        float end = Time.time + seconds;
        double deadline = EditorApplication.timeSinceStartup + 25f;
        while (Time.time < end)
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Play Mode stopped advancing");
            await Task.Delay(2);
        }
    }
    private static void Capture(string path, float orthographicSize = 2.2f)
    {
        var cameraObject = new GameObject("Verification camera");
        Camera camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = orthographicSize;
        camera.transform.position = new Vector3(body.position.x * 0.5f, 0.8f, -10f);
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.09f, 0.13f, 0.17f);
        foreach (SpriteRenderer visual in player.GetComponentsInChildren<SpriteRenderer>()) visual.sharedMaterial = renderer.sharedMaterial;
        var texture = new RenderTexture(1280, 720, 24); var capture = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
        RenderTexture previous = RenderTexture.active;
        try
        {
            camera.targetTexture = texture; camera.Render(); RenderTexture.active = texture;
            capture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); capture.Apply();
            File.WriteAllBytes(path, capture.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = null; RenderTexture.active = previous;
            Object.DestroyImmediate(texture); Object.DestroyImmediate(capture); Object.DestroyImmediate(cameraObject);
        }
        Check(new FileInfo(path).Length > 5000, "Rendered the real prefab and Player at 1280x720: " + path);
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        passed.Add(message); Debug.Log("SUCTION_WIND_PASS " + message);
    }
    private static void Finish(Exception error)
    {
        Time.timeScale = 1f;
        string result = error == null ? "PASS: Unity " + Application.unityVersion + "; " + passed.Count + " checks (" + assetChecks + " asset, " + (passed.Count - assetChecks) + " Play Mode/render). Current real Player, suction prefab and wind sheets in an isolated batch project. Main levels and exported build not tested."
            : "FAIL after " + passed.Count + " checks: " + error;
        string prefix = includeHeatAura ? "suction-heat-aura" : "suction-wind";
        File.WriteAllText(prefix + "-result.txt", result);
        File.WriteAllLines(prefix + "-checks.txt", passed);
        Debug.Log("SUCTION_WIND_RESULT " + result);
        if (error != null) Debug.LogException(error);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}
