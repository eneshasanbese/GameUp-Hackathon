using UnityEngine;

/// <summary>
/// Receives Animation Events on the Animator's GameObject and plays their audio clips.
/// Assign the clip in the Animation Event's object parameter.
/// </summary>
[DisallowMultipleComponent]
public sealed class SoundOnAnimationEvent : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float volume = 0.7f;
    [SerializeField, Min(0.5f)] private float hearingDistance = 12f;

    public void PlaySound(AudioClip clip)
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
            return;

        Sfx.PlayAt(clip, transform.position, volume, hearingDistance);
    }
}
