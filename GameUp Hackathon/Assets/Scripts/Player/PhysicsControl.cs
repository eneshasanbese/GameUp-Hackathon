using UnityEngine;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class PhysicsControl : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Ground Raycasts")]
    [SerializeField] private LayerMask dryGroundLayer;
    [SerializeField] private LayerMask wetGroundLayer;
    [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.45f);
    [SerializeField, Min(0.01f)] private float groundRayDistance = 0.15f;

    private bool isJumpAscending;

    public bool dryGrounded { get; private set; }
    public bool wetGrounded { get; private set; }
    public bool grounded => dryGrounded || wetGrounded;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        RefreshGrounded();
    }

    public void RefreshGrounded()
    {
        if (rb.linearVelocity.y <= 0f)
            isJumpAscending = false;

        Vector2 origin = rb.position + groundCheckOffset;
        RaycastHit2D dryHit = Physics2D.Raycast(origin, Vector2.down, groundRayDistance, dryGroundLayer);
        RaycastHit2D wetHit = Physics2D.Raycast(origin, Vector2.down, groundRayDistance, wetGroundLayer);

        // If layers overlap below the ray, use the closest surface.
        wetGrounded = !isJumpAscending && wetHit && (!dryHit || wetHit.distance < dryHit.distance);
        dryGrounded = !isJumpAscending && dryHit && !wetGrounded;
    }

    public void SetHorizontalSpeed(float speed)
    {
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    public void ApproachHorizontalSpeed(float targetSpeed, float acceleration)
    {
        SetHorizontalSpeed(Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed,
            acceleration * Time.fixedDeltaTime));
    }

    public void ResetMotion()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        isJumpAscending = false;
        dryGrounded = wetGrounded = false;
    }

    public void Jump(float speed)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, speed);
        isJumpAscending = true;
        dryGrounded = wetGrounded = false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.TransformPoint(groundCheckOffset);
        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundRayDistance);
    }
}
