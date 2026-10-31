using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class EmberFireball : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float speed = 4f;
    [SerializeField, Min(0f)] private float damageWaterPoints = 10f;
    [SerializeField, Min(0.01f)] private float maxLifetime = 4f;
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private Transform visual;
    [SerializeField] private float rotationOffset;

    private Rigidbody2D body;
    private CircleCollider2D core;
    private Player target;
    private WaterMeter water;
    private Transform owner;
    private Vector2 direction;
    private float remaining;
    private bool launched;
    private bool spent;
    private readonly List<RaycastHit2D> hits = new List<RaycastHit2D>();
    private readonly List<Collider2D> overlaps = new List<Collider2D>();

    public float WorldRadius
    {
        get
        {
            CircleCollider2D shape = core != null ? core : GetComponent<CircleCollider2D>();
            return shape.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
        }
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        core = GetComponent<CircleCollider2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        core.isTrigger = true;
    }

    public void Launch(Vector2 aimPosition, Player intendedTarget, Transform source, LayerMask players, LayerMask obstacles)
    {
        if (launched || spent)
            return;
        target = intendedTarget;
        water = target != null ? target.GetComponent<WaterMeter>() : null;
        owner = source;
        playerLayerMask = players;
        obstacleLayerMask = obstacles;
        direction = (aimPosition - body.position).normalized;
        if (direction.sqrMagnitude < 0.5f || !IsGameplayActive(target, water))
        {
            Retire();
            return;
        }
        launched = true;
        remaining = Mathf.Max(0.01f, maxLifetime);
        target.GameplayStopped += Retire;
        if (visual != null)
            visual.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset);
    }

    internal static bool IsGameplayActive(Player player, WaterMeter meter)
        => Time.timeScale > 0f && !SceneFader.IsBusy && player != null && player.isActiveAndEnabled &&
            meter != null && meter.isActiveAndEnabled && !meter.IsEmpty && !meter.IsRespawning &&
            player.physicsControl != null && player.physicsControl.rb.simulated;

    private void FixedUpdate()
    {
        if (spent)
            return;
        if (!launched || !IsGameplayActive(target, water) || owner == null || !owner.gameObject.activeInHierarchy)
        {
            Retire();
            return;
        }
        float step = Mathf.Min(Time.fixedDeltaTime, remaining);
        Vector2 movement = direction * Mathf.Max(0f, speed) * step;
        var filter = new ContactFilter2D();
        filter.SetLayerMask(playerLayerMask.value | obstacleLayerMask.value);
        filter.useTriggers = false;

        // Check initial overlaps as well as the whole swept core; trigger CCD alone is insufficient.
        overlaps.Clear();
        Physics2D.OverlapCircle(body.position, WorldRadius, filter, overlaps);
        Collider2D first = null;
        foreach (Collider2D other in overlaps)
        {
            if (!IsRelevant(other)) continue;
            if (first == null || IsObstacle(other)) first = other;
        }
        if (first == null && movement.sqrMagnitude > 0f)
        {
            hits.Clear();
            Physics2D.CircleCast(body.position, WorldRadius, direction, filter, hits, movement.magnitude);
            float closest = float.PositiveInfinity;
            foreach (RaycastHit2D hit in hits)
            {
                if (!IsRelevant(hit.collider)) continue;
                if (hit.distance < closest || (Mathf.Approximately(hit.distance, closest) && IsObstacle(hit.collider)))
                {
                    closest = hit.distance;
                    first = hit.collider;
                }
            }
        }
        if (first != null)
        {
            Impact(first);
            return;
        }
        body.MovePosition(body.position + movement);
        remaining -= Time.fixedDeltaTime;
        if (remaining <= 0f) Retire();
    }

    private bool IsObstacle(Collider2D other) => (obstacleLayerMask.value & (1 << other.gameObject.layer)) != 0;

    private bool IsRelevant(Collider2D other)
    {
        if (other == null || other.isTrigger || other == core || other.transform.IsChildOf(owner))
            return false;
        if (IsObstacle(other)) return true;
        return (playerLayerMask.value & (1 << other.gameObject.layer)) != 0 && other.attachedRigidbody != null &&
            other.attachedRigidbody.GetComponent<Player>() != null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!spent && launched && owner != null && IsRelevant(other)) Impact(other);
    }

    private void Impact(Collider2D other)
    {
        if (spent) return;
        // Latch and disable before applying damage: a lethal hit synchronously disables the Player.
        spent = true;
        core.enabled = false;
        body.simulated = false;
        if (!IsObstacle(other) && IsGameplayActive(target, water))
        {
            Player hitPlayer = other.attachedRigidbody.GetComponent<Player>();
            WaterMeter hitWater = hitPlayer != null ? hitPlayer.GetComponent<WaterMeter>() : null;
            if (IsGameplayActive(hitPlayer, hitWater))
            {
                KnockbackAbility protection = hitPlayer.GetComponent<KnockbackAbility>();
                if (protection != null) protection.TryWaterHit(damageWaterPoints);
                else hitWater.Consume(damageWaterPoints);
            }
        }
        Destroy(gameObject);
    }

    public void Retire()
    {
        if (spent) return;
        spent = true;
        if (core != null) core.enabled = false;
        if (body != null) body.simulated = false;
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        if (target != null) target.GameplayStopped -= Retire;
        // These are single-use projectiles, including when a parent is disabled and re-enabled.
        spent = true;
        if (core != null) core.enabled = false;
        if (body != null) body.simulated = false;
        if (Application.isPlaying) Destroy(gameObject);
    }
}
