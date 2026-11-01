using UnityEngine;

public sealed class KnockbackSoundProbe : MonoBehaviour
{
    public static AudioClip ObservedClip;
    public int Events { get; private set; }

    public void PlaySound(AudioClip clip)
    {
        if (clip == ObservedClip)
            Events++;
    }
}
