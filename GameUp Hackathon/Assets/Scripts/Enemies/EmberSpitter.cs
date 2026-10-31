using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EmberSpitter : MonoBehaviour
{
    public enum ShotState { Idle, Windup, Cooldown }

    [SerializeField, Min(0.01f)] private float detectionRadius = 5f;
    [SerializeField, Min(0f)] private float windupDuration = 0.8f;
    [SerializeField, Min(0f)] private float cooldownDuration = 2.5f;
    [SerializeField] private EmberFireball projectilePrefab;
    [SerializeField] private Transform rangeOrigin;
    [SerializeField] private Transform firePointRight;
    [SerializeField] private Transform firePointLeft;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private LayerMask obstacleLayerMask;

    private Player target;
    private WaterMeter water;
    private float timer;
    private Vector3 visualScale;
    private Color visualColor;
    private readonly List<Collider2D> candidates = new List<Collider2D>();
    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private readonly List<RaycastHit2D> sightHits = new List<RaycastHit2D>();
    private readonly List<EmberFireball> projectiles = new List<EmberFireball>();

    public ShotState CurrentState { get; private set; }
    private Vector2 Origin => rangeOrigin != null ? (Vector2)rangeOrigin.position : (Vector2)transform.position;
    private Vector2 Aim => target.physicsControl.rb.worldCenterOfMass;

    private void Awake()
    {
        if (visual != null)
        {
            visualScale = visual.transform.localScale;
            visualColor = visual.color;
        }
    }

    private void FixedUpdate()
    {
        if (Time.timeScale <= 0f || SceneFader.IsBusy)
        {
            StopGameplay();
            return;
        }
        if (target != null && !EmberFireball.IsGameplayActive(target, water)) StopGameplay();
        if (CurrentState == ShotState.Cooldown)
        {
            timer -= Time.fixedDeltaTime;
            if (timer <= 0f) CurrentState = ShotState.Idle;
            return;
        }
        if (CurrentState == ShotState.Idle)
        {
            FindTarget();
            if (!CanFire(out _)) return;
            CurrentState = ShotState.Windup;
            timer = Mathf.Max(0f, windupDuration);
            return;
        }

        timer -= Time.fixedDeltaTime;
        if (visual != null)
        {
            float progress = 1f - Mathf.Clamp01(timer / Mathf.Max(0.001f, windupDuration));
            visual.transform.localScale = Vector3.Scale(visualScale, new Vector3(1f + 0.12f * progress, 1f - 0.12f * progress, 1f));
            visual.color = Color.Lerp(visualColor, new Color(1f, 0.65f, 0.35f, visualColor.a), progress);
        }
        if (timer > 0f) return;
        bool ready = CanFire(out Vector2 muzzle);
        if (ready)
        {
            projectiles.RemoveAll(projectile => projectile == null);
            EmberFireball projectile = Instantiate(projectilePrefab, muzzle, Quaternion.identity);
            projectiles.Add(projectile);
            projectile.Launch(Aim, target, transform, playerLayerMask, obstacleLayerMask);
        }
        ResetVisual();
        CurrentState = ShotState.Cooldown;
        timer = ready ? Mathf.Max(0f, cooldownDuration) : 0.25f;
    }

    private void FindTarget()
    {
        Player closest = null;
        float distance = detectionRadius * detectionRadius;
        var filter = new ContactFilter2D();
        filter.SetLayerMask(playerLayerMask);
        filter.useTriggers = false;
        candidates.Clear();
        Physics2D.OverlapCircle(Origin, detectionRadius, filter, candidates);
        foreach (Collider2D candidate in candidates)
        {
            Rigidbody2D body = candidate.attachedRigidbody;
            Player player = body != null ? body.GetComponent<Player>() : null;
            WaterMeter meter = player != null ? player.GetComponent<WaterMeter>() : null;
            if (!EmberFireball.IsGameplayActive(player, meter)) continue;
            float squared = (body.position - Origin).sqrMagnitude;
            if (squared <= distance) { closest = player; distance = squared; }
        }
        if (target == closest) return;
        if (target != null) target.GameplayStopped -= StopGameplay;
        target = closest;
        water = target != null ? target.GetComponent<WaterMeter>() : null;
        if (target != null) target.GameplayStopped += StopGameplay;
    }

    private bool CanFire(out Vector2 muzzle)
    {
        muzzle = Origin;
        if (projectilePrefab == null || !EmberFireball.IsGameplayActive(target, water) ||
            (target.physicsControl.rb.position - Origin).sqrMagnitude > detectionRadius * detectionRadius)
            return false;
        bool right = Aim.x >= Origin.x;
        if (visual != null) visual.flipX = !right;
        Transform point = right ? firePointRight : firePointLeft;
        if (point == null) return false;
        muzzle = point.position;
        float radius = projectilePrefab.WorldRadius;
        // Start at the body as well as the mouth: an offset muzzle must never bridge a thin wall.
        return !Blocked(Origin, muzzle, radius) && !Blocked(muzzle, Aim, 0.001f);
    }

    private bool Blocked(Vector2 start, Vector2 end, float radius)
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(obstacleLayerMask);
        filter.useTriggers = false;
        overlaps.Clear();
        Physics2D.OverlapCircle(start, radius, filter, overlaps);
        foreach (Collider2D other in overlaps)
            if (!other.transform.IsChildOf(transform)) return true;
        Vector2 delta = end - start;
        sightHits.Clear();
        Physics2D.CircleCast(start, radius, delta.normalized, filter, sightHits, delta.magnitude);
        foreach (RaycastHit2D hit in sightHits)
            if (!hit.collider.transform.IsChildOf(transform)) return true;
        // Explicit end overlap also works when Physics2D.queriesStartInColliders is disabled.
        overlaps.Clear();
        Physics2D.OverlapCircle(end, radius, filter, overlaps);
        foreach (Collider2D other in overlaps)
            if (!other.transform.IsChildOf(transform)) return true;
        return false;
    }

    private void ResetVisual()
    {
        if (visual == null) return;
        visual.transform.localScale = visualScale;
        visual.color = visualColor;
    }

    private void StopGameplay()
    {
        if (target != null) target.GameplayStopped -= StopGameplay;
        target = null;
        water = null;
        CurrentState = ShotState.Idle;
        timer = 0f;
        ResetVisual();
        foreach (EmberFireball projectile in projectiles)
            if (projectile != null) projectile.Retire();
        projectiles.Clear();
    }

    private void OnDisable() => StopGameplay();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0.1f);
        Gizmos.DrawWireSphere(Origin, detectionRadius);
    }
}
