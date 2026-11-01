using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>
/// One place to play short sound effects. Uses a single 2D AudioSource that survives
/// scene loads, so a sound is not cut off by a death reload or a level change.
/// PlayAt fades the sound with distance from the camera, so far-away events stay quiet.
/// </summary>
public static class Sfx
{
    private static AudioSource source;
    private static AudioMixerGroup output;

    /// <summary>Mixer group for all effects (set by VolumeSettings), so the volume slider applies.</summary>
    public static AudioMixerGroup Output
    {
        get => output;
        set
        {
            output = value;
            if (source != null)
                source.outputAudioMixerGroup = value;
        }
    }

    /// <summary>
    /// True while world sounds (enemies, traps, projectiles: everything played with PlayAt) are silenced,
    /// e.g. during the ending. Cleared when the next scene loads.
    /// </summary>
    public static bool WorldMuted { get; private set; }

    /// <summary>Stops the sounds already playing and silences PlayAt until the next scene loads.</summary>
    public static void MuteWorld()
    {
        WorldMuted = true;
        if (source != null)
            source.Stop();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        WorldMuted = false;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
            WorldMuted = false;
    }

    public static void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null || volume <= 0f)
            return;
        GetSource().PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public static void PlayAt(AudioClip clip, Vector2 position, float volume = 1f, float hearingDistance = 12f)
    {
        if (WorldMuted)
            return;
        Camera camera = Camera.main;
        if (camera != null)
        {
            float distance = Vector2.Distance(camera.transform.position, position);
            volume *= 1f - Mathf.Clamp01(distance / Mathf.Max(0.01f, hearingDistance));
        }
        if (volume > 0.01f)
            Play(clip, volume);
    }

    private static AudioSource GetSource()
    {
        if (source == null)
        {
            var holder = new GameObject("Sfx");
            Object.DontDestroyOnLoad(holder);
            source = holder.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = output;
        }
        return source;
    }
}
