using UnityEngine;

/// <summary>
/// Plays the water source's AudioSource clip once when the player enters.
/// Uses Sfx so the cue can finish when this puddle also changes the level.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public sealed class WaterSourceSound : MonoBehaviour
{
    [SerializeField, Min(0.5f)] private float hearingDistance = 12f;

    private AudioSource source;

    private void Awake() => source = GetComponent<AudioSource>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActiveAndEnabled)
            return;

        Rigidbody2D body = other.attachedRigidbody;
        if (body == null || !body.TryGetComponent(out WaterMeter water) ||
            !water.isActiveAndEnabled || water.IsEmpty || water.IsRespawning ||
            !source.isActiveAndEnabled || source.mute)
            return;

        // LevelExit may already have disabled Player in this same physics step.
        Sfx.PlayAt(source.clip, transform.position, source.volume, hearingDistance);
    }
}
