using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bölüm müziğini ölüm sonrası sahne yeniden yüklemesinde kesmeden sürdürür.
/// Aynı sahne tekrar yüklenince yeni Music kopyası kendini kapatır, çalan müzik devam eder.
/// Başka bir sahneye geçilince o sahnenin Music nesnesi devralır; yeni sahnede Music yoksa müzik durur.
/// <see cref="SwitchTo"/> bölüm müziğini kısıp başka bir parçayı çalar (ör. final sinematiği).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public sealed class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer current;

    private string ownerScenePath;
    private AudioSource source;
    private AudioSource overlay;
    private Coroutine switching;

    // Keeps the static reference clean when domain reload is disabled.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => current = null;

    /// <summary>
    /// Starts <paramref name="clip"/> at once (optional fade-in) while the level music fades out.
    /// The clip runs on its own AudioSource; the next level's Music object replaces it as usual.
    /// </summary>
    public static void SwitchTo(AudioClip clip, float volume, bool loop, float fadeOut, float fadeIn)
    {
        if (clip == null)
            return;
        if (current == null)
            new GameObject("Music").AddComponent<MusicPlayer>();   // Awake makes it current.
        if (current.switching != null)
            current.StopCoroutine(current.switching);
        current.switching = current.StartCoroutine(current.Switch(clip, volume, loop, fadeOut, fadeIn));
    }

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        string scenePath = gameObject.scene.path;
        if (current != null && current != this)
        {
            if (current.ownerScenePath == scenePath)
            {
                // Same level reloaded after a death: keep the music that is already playing.
                source.Stop();
                source.enabled = false;
                Destroy(gameObject);
                return;
            }
            Destroy(current.gameObject);
        }

        current = this;
        ownerScenePath = scenePath;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (current == this)
            current = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // A new level without its own Music object ends the previous level's music.
        if (current == this && mode == LoadSceneMode.Single && scene.path != ownerScenePath)
            Destroy(gameObject);
    }

    // The new clip starts at once; the old music fades out underneath it (crossfade).
    // Unscaled time: the switch keeps going if a cutscene or panel pauses the game.
    private IEnumerator Switch(AudioClip clip, float volume, bool loop, float fadeOut, float fadeIn)
    {
        if (overlay == null)
            overlay = gameObject.AddComponent<AudioSource>();
        else if (overlay.isPlaying)
            overlay.Stop();   // A second switch replaces the previous clip.
        overlay.playOnAwake = false;
        overlay.spatialBlend = 0f;
        overlay.priority = 0;
        overlay.outputAudioMixerGroup = source.outputAudioMixerGroup;
        overlay.loop = loop;
        overlay.clip = clip;
        overlay.volume = fadeIn > 0f ? 0f : volume;
        overlay.Play();

        float startVolume = source.volume;
        float duration = Mathf.Max(fadeOut, fadeIn);
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            source.volume = Mathf.Lerp(startVolume, 0f, fadeOut > 0f ? t / fadeOut : 1f);
            overlay.volume = Mathf.Lerp(0f, volume, fadeIn > 0f ? t / fadeIn : 1f);
            yield return null;
        }
        source.Stop();
        source.volume = startVolume;
        overlay.volume = volume;
        switching = null;
    }
}
