using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// End of a level. When the player enters this trigger with water left,
/// the next level is loaded once behind a short screen fade (SceneFader).
/// The new scene starts with full water.
/// Add it to the level's last WaterSource instance (not to the prefab),
/// so reaching that puddle finishes the level.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    [Tooltip("Scene to load, e.g. Level_2. Leave empty to load the next scene in the build list.")]
    [SerializeField] private string nextSceneName;

    [Header("Transition")]
    [Tooltip("Seconds for the screen to darken, and again to clear in the next level. 0 = instant cut.")]
    [SerializeField, Min(0f)] private float fadeDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;
    [Tooltip("Optional sound when the level is finished, e.g. sfx_gem.")]
    [SerializeField] private AudioClip exitSound;
    [SerializeField, Range(0f, 1f)] private float exitSoundVolume = 0.8f;

    private bool loading;
    private bool warned;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        if (!GetComponent<Collider2D>().isTrigger)
            Debug.LogWarning($"{name}: LevelExit collider should have Is Trigger enabled.", this);
    }

    private void OnTriggerEnter2D(Collider2D other) => TryExit(other);

    // Also checked while inside, e.g. if the player was respawning on entry.
    private void OnTriggerStay2D(Collider2D other) => TryExit(other);

    private void TryExit(Collider2D other)
    {
        if (loading || other.attachedRigidbody == null)
            return;
        WaterMeter water = other.attachedRigidbody.GetComponent<WaterMeter>();
        // A drop with no water cannot finish a level.
        if (water == null || !water.isActiveAndEnabled || water.IsEmpty || water.IsRespawning)
            return;
        if (SceneFader.IsBusy || !CanLoadNext())
            return;

        bool started = string.IsNullOrWhiteSpace(nextSceneName)
            ? SceneFader.FadeAndLoad(SceneManager.GetActiveScene().buildIndex + 1, fadeDuration, fadeColor)
            : SceneFader.FadeAndLoad(nextSceneName, fadeDuration, fadeColor);
        if (!started)
            return;

        loading = true;
        Sfx.Play(exitSound, exitSoundVolume);
        Freeze(water, other.attachedRigidbody);
    }

    // Holds the drop in place and stops input and water drain while the screen fades out.
    private static void Freeze(WaterMeter water, Rigidbody2D body)
    {
        Player player = water.GetComponent<Player>();
        if (player != null)
            player.enabled = false;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.simulated = false;
    }

    private bool CanLoadNext()
    {
        bool canLoad;
        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            canLoad = next > 0 && next < SceneManager.sceneCountInBuildSettings;
        }
        else
        {
            canLoad = Application.CanStreamedLevelBeLoaded(nextSceneName);
        }

        if (!canLoad && !warned)
        {
            warned = true;
            Debug.LogWarning($"{name}: next scene is not in the build list. Add it in File > Build Profiles > Scene List.", this);
        }
        return canLoad;
    }
}
