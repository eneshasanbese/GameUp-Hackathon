using System.Collections.Generic;
using UnityEngine;

// Publish before Jump/Player motor processing; no competing Rigidbody velocity writer.
[DefaultExecutionOrder(-150)]
[DisallowMultipleComponent]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class EmberSuctionEnemy : MonoBehaviour
{
    [Header("Range (world units)")]
    [SerializeField] private Transform rangeOrigin;
    [SerializeField] private Transform pullPoint;
    [SerializeField, Min(0.01f)] private float effectRadius = 3f;
    [SerializeField, Min(0f)] private float innerFullEffectRadius = 0.75f;
    [SerializeField] private LayerMask playerLayerMask;

    [Header("Additional water points / second")]
    [SerializeField, Min(0f)] private float minWaterLossPerSecond = 1f;
    [SerializeField, Min(0f)] private float maxWaterLossPerSecond = 6f;

    [Header("Horizontal motor velocity offset (world units / second)")]
    [SerializeField, Min(0f)] private float minPullStrength = 0.4f;
    [SerializeField, Min(0f)] private float maxPullStrength = 2.5f;
    [SerializeField, Min(0f)] private float activationRampDuration = 0.35f;
    [SerializeField, Min(0.001f)] private float horizontalDeadZone = 0.08f;

    private sealed class Target
    {
        public Player player;
        public PhysicsControl physics;
        public WaterMeter water;
        public float elapsed;
        public bool active;
    }

    private CircleCollider2D area;
    private readonly Dictionary<Rigidbody2D, Target> targets = new Dictionary<Rigidbody2D, Target>();
    private readonly List<Rigidbody2D> staleTargets = new List<Rigidbody2D>();
    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private readonly HashSet<Rigidbody2D> overlappingBodies = new HashSet<Rigidbody2D>();

    private Vector2 Origin => rangeOrigin != null ? (Vector2)rangeOrigin.position : (Vector2)transform.position;
    private Vector2 Mouth => pullPoint != null ? (Vector2)pullPoint.position : Origin;

    public Vector2 MouthPosition => Mouth;
    public bool IsWindEnabled => isActiveAndEnabled && area != null && area.enabled;

    // Visuals read the real suction targets; they never run a second detection or damage path.
    public bool TryGetWindTarget(out Vector2 position, out float proximity, out float ramp)
    {
        position = Mouth;
        proximity = ramp = 0f;
        if (!IsWindEnabled || maxPullStrength <= 0f)
            return false;

        float nearest = effectRadius;
        bool found = false;
        foreach (var entry in targets)
        {
            Rigidbody2D body = entry.Key;
            Target target = entry.Value;
            if (!target.active || !IsPlayable(body, target))
                continue;
            Vector2 centre = body.worldCenterOfMass;
            float distance = Vector2.Distance(Origin, centre);
            if (distance >= nearest || Mathf.Abs(Mouth.x - centre.x) <= horizontalDeadZone)
                continue;
            nearest = distance;
            position = centre;
            proximity = Mathf.Clamp01((effectRadius - distance) / (effectRadius - innerFullEffectRadius));
            ramp = activationRampDuration > 0f ? Mathf.Clamp01(target.elapsed / activationRampDuration) : 1f;
            found = true;
        }
        return found;
    }

    private static bool IsPlayable(Rigidbody2D body, Target target)
        => body != null && body.simulated && target.player != null && target.player.isActiveAndEnabled &&
            target.physics != null && target.physics.isActiveAndEnabled && target.water != null &&
            target.water.isActiveAndEnabled && !target.water.IsEmpty && !target.water.IsRespawning;

    private void Awake()
    {
        area = GetComponent<CircleCollider2D>();
        ValidateSettings();
        SynchronizeArea();
    }

    private void OnValidate()
    {
        ValidateSettings();
        area = GetComponent<CircleCollider2D>();
        SynchronizeArea();
    }

    private static float NonNegative(float value, float fallback)
        => float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Max(0f, value);

    private void ValidateSettings()
    {
        effectRadius = Mathf.Max(0.01f, NonNegative(effectRadius, 3f));
        innerFullEffectRadius = Mathf.Min(NonNegative(innerFullEffectRadius, 0f), effectRadius - 0.001f);
        minWaterLossPerSecond = NonNegative(minWaterLossPerSecond, 0f);
        maxWaterLossPerSecond = Mathf.Max(minWaterLossPerSecond, NonNegative(maxWaterLossPerSecond, 0f));
        minPullStrength = NonNegative(minPullStrength, 0f);
        maxPullStrength = Mathf.Max(minPullStrength, NonNegative(maxPullStrength, 0f));
        activationRampDuration = NonNegative(activationRampDuration, 0f);
        horizontalDeadZone = Mathf.Max(0.001f, NonNegative(horizontalDeadZone, 0.08f));
    }

    private void SynchronizeArea()
    {
        if (area == null)
            return;
        // CircleCollider2D uses the largest XY scale. Keep the configured radius in world units.
        float scale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y), 0.0001f);
        area.radius = effectRadius / scale;
        area.offset = transform.InverseTransformPoint(Origin);
        area.isTrigger = true;
    }

    private void FixedUpdate()
    {
        SynchronizeArea();
        if (!area.enabled)
        {
            ClearTargets();
            return;
        }

        // Query the actual trigger each tick instead of depending on Exit/Stay delivery.
        // One Rigidbody entry handles any number of player colliders and teleport/disable cleanup.
        var filter = new ContactFilter2D();
        filter.SetLayerMask(playerLayerMask);
        filter.useTriggers = false;
        overlaps.Clear();
        area.Overlap(filter, overlaps);
        overlappingBodies.Clear();
        foreach (Collider2D other in overlaps)
        {
            Rigidbody2D body = other.attachedRigidbody;
            if (body == null || !body.simulated || !overlappingBodies.Add(body))
                continue;
            if (!targets.ContainsKey(body))
            {
                Player player = body.GetComponent<Player>();
                PhysicsControl physics = body.GetComponent<PhysicsControl>();
                WaterMeter water = body.GetComponent<WaterMeter>();
                if (player == null || physics == null || water == null)
                    continue;
                targets.Add(body, new Target { player = player, physics = physics, water = water });
            }
        }

        staleTargets.Clear();
        foreach (var entry in targets)
        {
            Rigidbody2D body = entry.Key;
            Target target = entry.Value;
            float distance = body != null ? Vector2.Distance(Origin, body.worldCenterOfMass) : float.PositiveInfinity;
            bool playable = IsPlayable(body, target) && Time.timeScale > 0f;
            if (!overlappingBodies.Contains(body) || !playable || distance >= effectRadius)
            {
                RemoveEffect(target);
                staleTargets.Add(body);
                continue;
            }

            float proximity = Mathf.Clamp01((effectRadius - distance) / (effectRadius - innerFullEffectRadius));
            target.elapsed += Time.fixedDeltaTime;
            float ramp = activationRampDuration > 0f ? Mathf.Clamp01(target.elapsed / activationRampDuration) : 1f;
            float difference = Mouth.x - body.worldCenterOfMass.x;
            // Zero inside the dead zone, with a short taper beyond it to avoid mouth chatter.
            float direction = Mathf.Sign(difference) * Mathf.Clamp01((Mathf.Abs(difference) - horizontalDeadZone) / horizontalDeadZone);
            float pull = Mathf.Lerp(minPullStrength, maxPullStrength, proximity) * ramp * direction;
            target.physics.SetExternalHorizontalSpeed(this, pull);
            target.active = true;
            // Absolute points, independent of remaining water, heat multipliers and body-hit flash.
            target.water.Consume(Mathf.Lerp(minWaterLossPerSecond, maxWaterLossPerSecond, proximity) * Time.fixedDeltaTime);
            if (target.water.IsRespawning || !target.player.isActiveAndEnabled)
                RemoveEffect(target);
        }
        foreach (Rigidbody2D body in staleTargets)
            targets.Remove(body);
    }

    private void RemoveEffect(Target target)
    {
        if (target.physics != null && target.active)
            target.physics.RemoveExternalHorizontalSpeed(this);
        target.active = false;
        target.elapsed = 0f;
    }

    private void ClearTargets()
    {
        foreach (Target target in targets.Values)
            RemoveEffect(target);
        targets.Clear();
        staleTargets.Clear();
        overlaps.Clear();
        overlappingBodies.Clear();
    }

    private void OnDisable() => ClearTargets();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.45f, 0.1f, 0.9f);
        Gizmos.DrawWireSphere(Origin, effectRadius);
        Gizmos.color = new Color(1f, 0.1f, 0f, 0.9f);
        Gizmos.DrawWireSphere(Origin, innerFullEffectRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Mouth, horizontalDeadZone);
    }
}
