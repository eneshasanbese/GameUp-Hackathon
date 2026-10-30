using UnityEngine;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class PhysicsControl : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Ground")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField, Range(0f, 89f)] private float maxGroundAngle = 50f;

    private Collider2D bodyCollider;
    private bool isJumpAscending;

    public bool grounded { get; private set; }

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        bodyCollider = GetComponent<Collider2D>();
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        RefreshGrounded();
    }

    public void RefreshGrounded()
    {
        // Ignore lingering floor contacts during a jump, but allow upward travel on slopes.
        if (rb.linearVelocity.y <= 0.01f)
            isJumpAscending = false;

        var groundFilter = new ContactFilter2D
        {
            useTriggers = false,
            useLayerMask = true,
            layerMask = groundLayers,
            useNormalAngle = true,
            minNormalAngle = 90f - maxGroundAngle,
            maxNormalAngle = 90f + maxGroundAngle
        };
        grounded = !isJumpAscending && bodyCollider.IsTouching(groundFilter);
    }

    public void SetHorizontalSpeed(float speed)
    {
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    public void Jump(float speed)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, speed);
        isJumpAscending = true;
        grounded = false;
    }
}
