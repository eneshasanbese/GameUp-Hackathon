using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

/// <summary>
/// The last living sapling (T27). When the drop touches it with water left, a short
/// cutscene plays once: the HUD fades out, the camera zooms in on the sapling, the drop
/// shrinks into it, the light beam brightens and the thirsty sprite turns into the green one.
/// Control and water drain stop for good.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class FinalSapling : MonoBehaviour
{
    [Header("Sapling")]
    [Tooltip("Living sapling, e.g. sapling_green_tall. The thirsty sprite stays on this object's Sprite Renderer.")]
    [SerializeField] private Sprite greenSprite;

    [Header("Light beam")]
    [Tooltip("Beam sprite (light_shaft). Found automatically in a child named \"Shaft\" when left empty.")]
    [SerializeField] private SpriteRenderer lightShaft;
    [SerializeField, Range(0f, 1f)] private float targetShaftAlpha = 1f;
    [Tooltip("Optional Light 2D. Found automatically among this object's children when left empty.")]
    [SerializeField] private Light2D saplingLight;
    [Tooltip("The light ends at its starting intensity times this value.")]
    [SerializeField, Min(0f)] private float lightIntensityMultiplier = 2f;

    [Header("Camera")]
    [Tooltip("Cinemachine camera to zoom. The one following the player is found automatically when left empty.")]
    [SerializeField] private CinemachineCamera playerCamera;
    [Tooltip("Point the camera moves to. Uses the sapling when left empty.")]
    [SerializeField] private Transform cameraFocus;
    [SerializeField, Min(0.5f)] private float zoomOrthographicSize = 3f;

    [Header("UI")]
    [Tooltip("UI hidden when the drop touches the sapling. The water bar is found automatically when left empty.")]
    [SerializeField] private GameObject[] hideOnTouch;

    [Header("Timing (seconds)")]
    [SerializeField, Min(0.01f)] private float uiFadeDuration = 0.4f;
    [SerializeField, Min(0.01f)] private float zoomDuration = 2f;
    [SerializeField, Min(0.01f)] private float shrinkDuration = 1.5f;
    [SerializeField, Min(0.01f)] private float bloomDuration = 2.5f;
    [SerializeField, Min(0f)] private float holdDuration = 1f;
    [Tooltip("Slides the shrinking drop to the sapling's centre, as if it is absorbed.")]
    [SerializeField] private bool pullDropToSapling = true;

    [Header("Music")]
    [Tooltip("Starts the moment the drop touches the sapling; the level music fades out underneath it. Leave empty to keep the level music.")]
    [SerializeField] private AudioClip endingMusic;
    [SerializeField, Range(0f, 1f)] private float endingMusicVolume = 0.4f;
    [Tooltip("Off for a short jingle, on for a track that should keep playing behind the win panel.")]
    [SerializeField] private bool loopEndingMusic = true;
    [Tooltip("Seconds for the level music to fade out under the ending music.")]
    [SerializeField, Min(0f)] private float musicFadeOut = 1f;
    [Tooltip("Seconds for the ending music to reach its volume. 0 starts it at full volume.")]
    [SerializeField, Min(0f)] private float musicFadeIn = 0.2f;

    [Header("After the cutscene")]
    [Tooltip("Called once the cutscene ends, e.g. to show the win panel.")]
    [SerializeField] private UnityEvent onFinished;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip absorbSound;
    [SerializeField] private AudioClip bloomSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip restartSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 0.8f;

    private bool started;

    private void Reset()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        if (sprite != null)
        {
            box.size = sprite.bounds.size;
            box.offset = sprite.bounds.center;
        }
    }

    private void Awake()
    {
        if (!GetComponent<BoxCollider2D>().isTrigger)
            Debug.LogWarning($"{name}: FinalSapling's Box Collider 2D should have Is Trigger enabled.", this);
        if (greenSprite == null)
            Debug.LogWarning($"{name}: FinalSapling has no Green Sprite; the sapling will not change.", this);
        if (lightShaft == null)
        {
            Transform shaft = transform.Find("Shaft");
            if (shaft != null)
                lightShaft = shaft.GetComponent<SpriteRenderer>();
        }
        if (saplingLight == null)
            saplingLight = GetComponentInChildren<Light2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) => TryStart(other);

    // Also checked while inside, e.g. if the drop arrived during knockback or a respawn.
    private void OnTriggerStay2D(Collider2D other) => TryStart(other);

    private void TryStart(Collider2D other)
    {
        if (started || other.isTrigger || other.attachedRigidbody == null)
            return;
        Rigidbody2D body = other.attachedRigidbody;
        WaterMeter water = body.GetComponent<WaterMeter>();
        Player player = body.GetComponent<Player>();
        // A drop with no water cannot save the sapling.
        if (water == null || player == null || !water.isActiveAndEnabled || !player.isActiveAndEnabled ||
            water.IsEmpty || water.IsRespawning)
            return;

        started = true;
        Sfx.Play(absorbSound, soundVolume);
        if (endingMusic != null)
            MusicPlayer.SwitchTo(endingMusic, endingMusicVolume, loopEndingMusic, musicFadeOut, musicFadeIn);
        StartCoroutine(Play(player, water, body));
    }

    private IEnumerator Play(Player player, WaterMeter water, Rigidbody2D body)
    {
        // The run is over: no input, no water drain, no death or respawn, no physics.
        player.enabled = false;
        water.enabled = false;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.simulated = false;

        // Camera: follow the sapling and zoom in.
        CinemachineCamera cam = FindCamera(player);
        CinemachineConfiner2D confiner = cam != null ? cam.GetComponent<CinemachineConfiner2D>() : null;
        float lensStart = cam != null ? cam.Lens.OrthographicSize : 0f;
        if (cam != null)
            cam.Follow = cameraFocus != null ? cameraFocus : transform;

        List<CanvasGroup> hud = CollectHud();
        SpriteRenderer drop = FindDropVisual(player);
        Light2D[] dropLights = player.GetComponentsInChildren<Light2D>();
        float[] dropLightStart = new float[dropLights.Length];
        for (int i = 0; i < dropLights.Length; i++)
            dropLightStart[i] = dropLights[i].intensity;
        SpriteRenderer thirsty = GetComponent<SpriteRenderer>();
        SpriteRenderer green = CreateGreenOverlay(thirsty);
        Vector3 dropScale = drop != null ? drop.transform.localScale : Vector3.one;
        float dropBottom = drop != null ? drop.bounds.min.y : 0f;
        Vector3 rootStart = player.transform.position;
        float lightStart = saplingLight != null ? saplingLight.intensity : 0f;
        float shaftStart = lightShaft != null ? lightShaft.color.a : 0f;

        float sequence = shrinkDuration + bloomDuration;
        float total = Mathf.Max(sequence, zoomDuration, uiFadeDuration);
        bool bloomSoundPlayed = false;
        for (float time = 0f; ; time += Time.deltaTime)
        {
            bool last = time >= total;
            if (last)
                time = total;
            if (!bloomSoundPlayed && time >= shrinkDuration)
            {
                bloomSoundPlayed = true;
                Sfx.Play(bloomSound, soundVolume);
            }
            float uiFade = Ease(time / uiFadeDuration);
            float zoom = Ease(time / zoomDuration);
            float shrink = Ease(time / shrinkDuration);
            float bloom = Ease((time - shrinkDuration) / bloomDuration);
            float glow = Ease(time / sequence);

            foreach (CanvasGroup group in hud)
                group.alpha = 1f - uiFade;

            if (cam != null)
            {
                cam.Lens.OrthographicSize = Mathf.Lerp(lensStart, zoomOrthographicSize, zoom);
                // The confiner does not notice lens changes by itself.
                if (confiner != null)
                    confiner.InvalidateLensCache();
            }

            if (drop != null)
            {
                if (pullDropToSapling)
                    player.transform.position = new Vector3(
                        Mathf.Lerp(rootStart.x, transform.position.x, shrink), rootStart.y, rootStart.z);
                drop.transform.localScale = dropScale * (1f - shrink);
                // Shrink towards the ground, not towards the sprite's centre.
                drop.transform.position += Vector3.up * (dropBottom - drop.bounds.min.y);
            }
            for (int i = 0; i < dropLights.Length; i++)
                dropLights[i].intensity = dropLightStart[i] * (1f - shrink);

            if (green != null)
            {
                SetAlpha(thirsty, 1f - bloom);
                SetAlpha(green, bloom);
            }
            if (lightShaft != null)
                SetAlpha(lightShaft, Mathf.Lerp(shaftStart, targetShaftAlpha, glow));
            if (saplingLight != null)
                saplingLight.intensity = Mathf.Lerp(lightStart, lightStart * lightIntensityMultiplier, glow);

            if (last)
                break;
            yield return null;
        }

        // Fully absorbed.
        if (drop != null)
            drop.enabled = false;
        foreach (CanvasGroup group in hud)
            group.blocksRaycasts = false;

        yield return new WaitForSeconds(holdDuration);
        Sfx.Play(victorySound, soundVolume);
        onFinished?.Invoke();
    }

    /// <summary>For the win panel's replay button: starts again from the first level.</summary>
    public void RestartGame()
    {
        if (SceneFader.FadeAndLoad(0, 0.5f, Color.black))
            Sfx.Play(restartSound, soundVolume);
    }

    private CinemachineCamera FindCamera(Player player)
    {
        if (playerCamera != null)
            return playerCamera;
        CinemachineCamera fallback = null;
        foreach (CinemachineCamera cam in FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None))
        {
            if (cam.Follow == player.transform)
                return cam;
            if (fallback == null && cam.isActiveAndEnabled)
                fallback = cam;
        }
        return fallback;
    }

    // Each hidden UI object fades through a CanvasGroup, added if it has none.
    private List<CanvasGroup> CollectHud()
    {
        var targets = new List<GameObject>();
        if (hideOnTouch != null && hideOnTouch.Length > 0)
        {
            foreach (GameObject target in hideOnTouch)
            {
                if (target != null)
                    targets.Add(target);
            }
        }
        else
        {
            foreach (WaterBarUI bar in FindObjectsByType<WaterBarUI>(FindObjectsSortMode.None))
                targets.Add(bar.gameObject);
        }

        var groups = new List<CanvasGroup>();
        foreach (GameObject target in targets)
        {
            if (!target.TryGetComponent(out CanvasGroup group))
                group = target.AddComponent<CanvasGroup>();
            groups.Add(group);
        }
        return groups;
    }

    private static SpriteRenderer FindDropVisual(Player player)
    {
        Transform visual = player.transform.Find("Visual");
        if (visual != null && visual.TryGetComponent(out SpriteRenderer renderer))
            return renderer;
        return player.GetComponentInChildren<SpriteRenderer>();
    }

    // The living sprite fades in on top of the thirsty one, with the same look and order.
    private SpriteRenderer CreateGreenOverlay(SpriteRenderer thirsty)
    {
        if (greenSprite == null)
            return null;
        var overlay = new GameObject("Green");
        overlay.transform.SetParent(transform, false);
        var green = overlay.AddComponent<SpriteRenderer>();
        green.sprite = greenSprite;
        green.sharedMaterial = thirsty.sharedMaterial;
        green.sortingLayerID = thirsty.sortingLayerID;
        green.sortingOrder = thirsty.sortingOrder + 1;
        green.flipX = thirsty.flipX;
        green.flipY = thirsty.flipY;
        green.color = thirsty.color;
        SetAlpha(green, 0f);
        return green;
    }

    private static float Ease(float t) => Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

    private static void SetAlpha(SpriteRenderer renderer, float alpha)
    {
        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }
}
