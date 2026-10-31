using UnityEngine;

/// <summary>
/// Instant death area, e.g. lava. Touching it empties the player's water,
/// which starts WaterMeter's normal death and respawn at the last checkpoint.
/// Works with any trigger collider on this object, including a Tilemap Collider 2D.
/// </summary>
[DisallowMultipleComponent]
public class KillZone : MonoBehaviour
{
    private void Awake()
    {
        foreach (Collider2D zone in GetComponents<Collider2D>())
        {
            if (zone.isTrigger)
                return;
        }
        Debug.LogWarning($"{name}: KillZone needs a Collider2D with Is Trigger enabled.", this);
    }

    private void OnTriggerEnter2D(Collider2D other) => Kill(other);

    // Also checked while inside, in case the first contact came during a respawn.
    private void OnTriggerStay2D(Collider2D other) => Kill(other);

    private void Kill(Collider2D other)
    {
        if (!isActiveAndEnabled || other.isTrigger || other.attachedRigidbody == null)
            return;
        WaterMeter water = other.attachedRigidbody.GetComponent<WaterMeter>();
        if (water == null || !water.isActiveAndEnabled || water.IsRespawning || water.IsEmpty)
            return;
        // Empty water = the single existing death flow (evaporate, respawn, refill).
        water.Consume(water.CurrentWater);
    }
}
