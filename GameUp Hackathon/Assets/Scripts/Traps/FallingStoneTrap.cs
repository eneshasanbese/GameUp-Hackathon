using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(PolygonCollider2D))]
public sealed class FallingStoneTrap : MonoBehaviour
{
    [Header("Player Detection (World Space)")]
    [SerializeField] private Vector2 detectionSize = new Vector2(3f, 7f);
    [SerializeField] private Vector2 detectionOffset = new Vector2(0f, -3.5f);
    [SerializeField] private LayerMask playerLayerMask;

    [Header("Fall")]
    [SerializeField, Min(0.01f)] private float gravityScale = 3f;
    [SerializeField, Min(0.01f)] private float maximumFallSpeed = 14f;
    [SerializeField, Min(0.01f)] private float despawnDelay = 3f;

    private readonly List<Collider2D> candidates = new List<Collider2D>();
    private Rigidbody2D body;
    private PolygonCollider2D hitbox;
    private bool falling;
    private bool hitPlayer;
    private float remaining;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<PolygonCollider2D>();
        hitbox.isTrigger = true;
        body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnEnable()
    {
        body.bodyType = falling ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
        body.gravityScale = falling ? gravityScale : 0f;
        hitbox.enabled = falling && !hitPlayer;
    }

    private void FixedUpdate()
    {
        if (!falling)
        {
            if (Time.timeScale <= 0f || SceneFader.IsBusy)
                return;
            var filter = new ContactFilter2D();
            filter.SetLayerMask(playerLayerMask);
            filter.useTriggers = false;
            candidates.Clear();
            Physics2D.OverlapBox((Vector2)transform.position + detectionOffset, detectionSize, 0f, filter, candidates);
            foreach (Collider2D other in candidates)
            {
                if (!IsActivePlayer(other))
                    continue;
                falling = true;
                remaining = despawnDelay;
                body.bodyType = RigidbodyType2D.Dynamic;
                body.gravityScale = gravityScale;
                hitbox.enabled = true;
                break;
            }
            return;
        }

        body.linearVelocity = new Vector2(0f, Mathf.Max(body.linearVelocity.y, -maximumFallSpeed));
        remaining -= Time.fixedDeltaTime;
        if (remaining <= 0f)
            Destroy(gameObject);
    }

    private static bool IsActivePlayer(Collider2D other)
    {
        if (other == null || other.isTrigger || other.attachedRigidbody == null || !other.attachedRigidbody.simulated)
            return false;
        Player player = other.attachedRigidbody.GetComponent<Player>();
        if (player == null || !player.isActiveAndEnabled)
            return false;
        WaterMeter water = player.GetComponent<WaterMeter>();
        return water == null || (water.isActiveAndEnabled && !water.IsEmpty && !water.IsRespawning);
    }

    private void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    private void OnTriggerStay2D(Collider2D other) => TryHit(other);

    private void TryHit(Collider2D other)
    {
        if (!isActiveAndEnabled || !falling || hitPlayer || !hitbox.enabled ||
            Time.timeScale <= 0f || SceneFader.IsBusy ||
            (playerLayerMask.value & (1 << other.gameObject.layer)) == 0 || !IsActivePlayer(other))
            return;
        KnockbackAbility knockback = other.attachedRigidbody.GetComponent<KnockbackAbility>();
        if (knockback == null)
            return;

        // Latch before a lethal hit can synchronously start the Player's respawn flow.
        hitPlayer = true;
        hitbox.enabled = false;
        knockback.TryKnockback(body.position);
    }

    private void OnDisable()
    {
        hitbox.enabled = false;
        body.linearVelocity = Vector2.zero;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.65f, 0.1f, 0.8f);
        Gizmos.DrawWireCube((Vector2)transform.position + detectionOffset, detectionSize);
    }
}
