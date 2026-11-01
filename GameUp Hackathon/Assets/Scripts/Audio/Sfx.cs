using UnityEngine;
using UnityEngine.Audio;

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

    public static void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null || volume <= 0f)
            return;
        GetSource().PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public static void PlayAt(AudioClip clip, Vector2 position, float volume = 1f, float hearingDistance = 12f)
    {
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
