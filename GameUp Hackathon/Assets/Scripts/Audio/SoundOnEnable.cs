using UnityEngine;

/// <summary>
/// Plays a sound when this object appears. Meant for objects spawned during play,
/// e.g. an Ember Spitter fireball or a HapsirikKoz smoke cloud. The sound is quieter
/// the farther the object is from the camera, and silent beyond Hearing Distance.
/// </summary>
public class SoundOnEnable : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.7f;
    [SerializeField, Min(0.5f)] private float hearingDistance = 12f;

    private void OnEnable() => Sfx.PlayAt(clip, transform.position, volume, hearingDistance);
}
