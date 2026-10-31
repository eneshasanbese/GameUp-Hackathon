using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class PhysicsControl : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Ground Contact")]
    [SerializeField] private LayerMask dryGroundLayer;
    [SerializeField] private LayerMask wetGroundLayer;
    [SerializeField] private LayerMask extraDryGroundLayer;
    [FormerlySerializedAs("groundRayDistance")]
    [SerializeField, Min(0.01f)] private float groundCheckDistance = 0.15f;
    [SerializeField, Range(0f, 85f)] private float maximumGroundAngle = 60f;

    private readonly ContactPoint2D[] contacts = new ContactPoint2D[16];
    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[16];
    private readonly Collider2D[] resizeOverlaps = new Collider2D[16];
    private CircleCollider2D bodyCollider;
    private float fullRadius;
    private Vector2 fullOffset;
    private float groundDistance;
    private float minimumGroundNormalY;
    private Collider2D groundCollider;
    private bool isJumpAscending;
    private readonly Dictionary<UnityEngine.Object, float> externalHorizontalSpeeds = new Dictionary<UnityEngine.Object, float>();

    public bool dryGrounded { get; private set; }
    public bool wetGrounded { get; private set; }
    public bool extraDryGrounded { get; private set; }
    public bool grounded => dryGrounded || wetGrounded || extraDryGrounded;
    public Vector2 GroundNormal { get; private set; } = Vector2.up;
    public float BodyScale { get; private set; } = 1f;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CircleCollider2D>();
        fullRadius = bodyCollider.radius;
        fullOffset = bodyCollider.offset;
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        if (extraDryGroundLayer.value == 0)
            Debug.LogWarning($"{name}: PhysicsControl Extra Dry Ground Layer is not set; that ground will not count as grounded.", this);
    }

    private void FixedUpdate()
    {
        RefreshGrounded();
    }

    public void RefreshGrounded()
    {
        bool wasGrounded = grounded;
        dryGrounded = wetGrounded = extraDryGrounded = false;
        groundCollider = null;
        groundDistance = float.PositiveInfinity;
        GroundNormal = Vector2.up;
        if (rb.linearVelocity.y <= 0f)
            isJumpAscending = false;
        if (isJumpAscending || !rb.simulated || !bodyCollider.enabled)
            return;

        minimumGroundNormalY = Mathf.Cos(maximumGroundAngle * Mathf.Deg2Rad);
        var filter = new ContactFilter2D();
        filter.SetLayerMask(dryGroundLayer.value | wetGroundLayer.value | extraDryGroundLayer.value);
        filter.useTriggers = false;
        int contactCount = bodyCollider.GetContacts(filter, contacts);
        Vector2 center = bodyCollider.bounds.center;
        for (int i = 0; i < contactCount; i++)
        {
            ContactPoint2D contact = contacts[i];
            Collider2D other = contact.collider == bodyCollider ? contact.otherCollider : contact.collider;
            Vector2 normal = contact.normal;
            if (Vector2.Dot(normal, center - contact.point) < 0f)
                normal = -normal;
            ConsiderGround(other, normal, 0f);
        }

        if (groundCollider == null)
        {
            float probeDistance = groundCheckDistance * BodyScale;
            int hitCount = bodyCollider.Cast(Vector2.down, filter, groundHits, probeDistance);
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = groundHits[i];
                Vector2 normal = hit.normal;
                // A cast starting in contact can report the inverse cast direction, even at a wall.
                if (hit.distance <= 0.0001f)
                {
                    Vector2 away = center - hit.collider.ClosestPoint(center);
                    if (away.sqrMagnitude > 0.000001f)
                        normal = away.normalized;
                }
                ConsiderGround(hit.collider, normal, hit.distance);
            }
            float contactDistance = 0.02f * BodyScale;
            if (groundCollider != null && (wasGrounded || groundDistance <= contactDistance) &&
                (wasGrounded || rb.linearVelocity.y <= 0f))
                rb.position += Vector2.down * groundDistance;
            else
                groundCollider = null;
        }

        if (groundCollider == null)
        {
            GroundNormal = Vector2.up;
            return;
        }
        int groundBit = 1 << groundCollider.gameObject.layer;
        wetGrounded = (wetGroundLayer.value & groundBit) != 0;
        extraDryGrounded = !wetGrounded && (extraDryGroundLayer.value & groundBit) != 0;
        dryGrounded = !wetGrounded && !extraDryGrounded;
    }

    private void ConsiderGround(Collider2D other, Vector2 normal, float distance)
    {
        if (other == null || other.attachedRigidbody == rb || normal.y < minimumGroundNormalY ||
            distance >= groundDistance)
            return;
        groundCollider = other;
        GroundNormal = normal;
        groundDistance = distance;
    }

    public void SetBodyScale(float scale)
    {
        scale = Mathf.Clamp(scale, 0.1f, 1f);
        if (Mathf.Abs(scale - BodyScale) < 0.00001f)
            return;

        if (scale > BodyScale && !CanFitBodyScale(scale))
        {
            // Refill must not expand the body through a low ceiling. Retry as space becomes free.
            float low = BodyScale;
            float high = scale;
            for (int i = 0; i < 8; i++)
            {
                float middle = (low + high) * 0.5f;
                if (CanFitBodyScale(middle))
                    low = middle;
                else
                    high = middle;
            }
            scale = low;
        }

        float radiusChange = fullRadius * (scale - BodyScale);
        rb.position += GetSlopeSizeAdjustment(radiusChange);
        bodyCollider.radius = fullRadius * scale;
        bodyCollider.offset = fullOffset + Vector2.up * fullRadius * (scale - 1f);
        BodyScale = scale;
    }

    private Vector2 GetSlopeSizeAdjustment(float radiusChange)
    {
        // Keep the support point on the ramp while the foot-anchored circle changes size.
        float worldScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
        return grounded ? Vector2.up * radiusChange * worldScale * (1f / GroundNormal.y - 1f) : Vector2.zero;
    }

    private bool CanFitBodyScale(float scale)
    {
        Vector2 offset = fullOffset + Vector2.up * fullRadius * (scale - 1f);
        Vector2 center = rb.position + (Vector2)transform.TransformVector(offset) +
            GetSlopeSizeAdjustment(fullRadius * (scale - BodyScale));
        float worldScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
        float radius = fullRadius * scale * worldScale;
        var filter = new ContactFilter2D();
        filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        filter.useTriggers = false;
        Vector2 currentCenter = rb.position + (Vector2)transform.TransformVector(bodyCollider.offset);
        float currentRadius = bodyCollider.radius * worldScale;
        int count = Physics2D.OverlapCircle(center, radius, filter, resizeOverlaps);
        for (int i = 0; i < count; i++)
        {
            Collider2D other = resizeOverlaps[i];
            if (other.attachedRigidbody == rb)
                continue;
            float clearance = Vector2.Distance(other.ClosestPoint(center), center) - radius;
            float previousClearance = Vector2.Distance(other.ClosestPoint(currentCenter), currentCenter) - currentRadius;
            // Preserve existing contact skin at the supporting floor without creating new penetration.
            if (clearance < Mathf.Min(-0.002f, previousClearance - 0.0001f))
                return false;
        }
        return count < resizeOverlaps.Length;
    }

    public void SetHorizontalSpeed(float speed)
    {
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    public void ApproachHorizontalSpeed(float targetSpeed, float acceleration)
    {
        // Environmental entries are velocity offsets (world units/second), not forces.
        // Compose the motor target here so Idle/Move/Jump cannot overwrite a separate AddForce.
        foreach (var entry in externalHorizontalSpeeds)
        {
            if (entry.Key != null)
                targetSpeed += entry.Value;
        }
        float speed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
        if (!grounded)
        {
            SetHorizontalSpeed(speed);
            return;
        }

        // Follow the surface tangent, so releasing input removes uphill velocity too.
        rb.linearVelocity = new Vector2(speed, -GroundNormal.x / GroundNormal.y * speed);
        rb.AddForce(-Physics2D.gravity * rb.gravityScale * rb.mass);
    }

    public void ResetMotion()
    {
        ClearExternalHorizontalSpeeds();
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        isJumpAscending = false;
        dryGrounded = wetGrounded = extraDryGrounded = false;
        GroundNormal = Vector2.up;
    }

    public void SetExternalHorizontalSpeed(UnityEngine.Object source, float speed)
    {
        if (source == null || !isActiveAndEnabled || !rb.simulated || float.IsNaN(speed) || float.IsInfinity(speed))
            return;
        externalHorizontalSpeeds[source] = speed;
    }

    public void RemoveExternalHorizontalSpeed(UnityEngine.Object source)
    {
        if (!ReferenceEquals(source, null))
            externalHorizontalSpeeds.Remove(source);
    }

    public void ClearExternalHorizontalSpeeds() => externalHorizontalSpeeds.Clear();

    private void OnDisable() => ClearExternalHorizontalSpeeds();

    public void Jump(float speed)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, speed);
        isJumpAscending = true;
        dryGrounded = wetGrounded = extraDryGrounded = false;
    }

    private void OnDrawGizmosSelected()
    {
        CircleCollider2D shape = bodyCollider != null ? bodyCollider : GetComponent<CircleCollider2D>();
        if (shape == null)
            return;
        Vector3 center = transform.TransformPoint(shape.offset);
        float radius = shape.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
        Vector3 end = center + Vector3.down * groundCheckDistance * BodyScale;
        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(center, radius);
        Gizmos.DrawWireSphere(end, radius);
        Gizmos.DrawLine(center, end);
    }
}
