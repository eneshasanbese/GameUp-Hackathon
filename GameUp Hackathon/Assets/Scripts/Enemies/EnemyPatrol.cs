using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyPatrol : MonoBehaviour
{
    [Tooltip("Displacement from the placed starting point, in world units. Zero keeps the enemy stationary.")]
    [SerializeField] private Vector2 endOffset = new Vector2(3f, 0f);
    [SerializeField, Min(0f)] private float speed = 1.5f;
    [SerializeField] private SpriteRenderer bodyVisual;
    [SerializeField] private bool faceMovement;

    [Header("Player Chase")]
    [Tooltip("Detection radius in world units, centred on and moving with the enemy. Zero disables chasing.")]
    [SerializeField, Min(0f)] private float detectionRadius = 4f;
    [Tooltip("Keep the enemy at its current height while chasing a jumping player.")]
    [SerializeField] private bool horizontalChaseOnly = true;

    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private Rigidbody2D body;
    private Vector2 start;
    private bool returning;
    private bool initialFlipX;
    private bool resumingPatrol;
    private Vector2 resumePosition;

    public bool IsChasing { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        start = body.position;
        if (bodyVisual != null)
            initialFlipX = bodyVisual.flipX;
    }

    private void FixedUpdate()
    {
        if (!body.simulated || speed <= 0f || endOffset.sqrMagnitude < 0.000001f)
        {
            IsChasing = false;
            return;
        }

        Vector2 position = body.position;
        Rigidbody2D playerBody = FindPlayer();
        IsChasing = playerBody != null;
        Vector2 target;
        if (IsChasing)
        {
            if (!resumingPatrol)
            {
                resumePosition = position;
                resumingPatrol = true;
            }
            target = playerBody.position;
            if (horizontalChaseOnly)
                target.y = position.y;
        }
        else
        {
            target = resumingPatrol ? resumePosition : returning ? start : start + endOffset;
        }

        Vector2 next = Vector2.MoveTowards(position, target, speed * Time.fixedDeltaTime);
        if (faceMovement && bodyVisual != null && Mathf.Abs(next.x - position.x) > 0.00001f)
            bodyVisual.flipX = next.x < position.x ? !initialFlipX : initialFlipX;
        body.MovePosition(next);
        if (!IsChasing && (next - target).sqrMagnitude < 0.000001f)
        {
            if (resumingPatrol)
                resumingPatrol = false;
            else
                returning = !returning;
        }
    }

    private Rigidbody2D FindPlayer()
    {
        if (detectionRadius <= 0f)
            return null;

        // Query real player colliders so exit, teleport and disabled objects cannot leave stale contacts.
        var filter = new ContactFilter2D { useTriggers = false };
        Physics2D.OverlapCircle(body.position, detectionRadius, filter, overlaps);
        Rigidbody2D closest = null;
        float closestDistance = float.PositiveInfinity;
        foreach (Collider2D other in overlaps)
        {
            Rigidbody2D candidate = other.attachedRigidbody;
            if (candidate == null || !candidate.simulated ||
                !candidate.TryGetComponent(out Player player) || !player.isActiveAndEnabled)
                continue;
            if (candidate.TryGetComponent(out WaterMeter water) &&
                (!water.isActiveAndEnabled || water.IsRespawning || water.IsEmpty))
                continue;

            float distance = (candidate.position - body.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closest = candidate;
                closestDistance = distance;
            }
        }
        return closest;
    }

    private void OnDisable()
    {
        IsChasing = false;
        overlaps.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? (Vector3)start : transform.position;
        Vector3 end = origin + (Vector3)endOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, end);
        Gizmos.DrawWireSphere(origin, 0.12f);
        Gizmos.DrawWireSphere(end, 0.12f);
        Gizmos.color = IsChasing ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying && body != null ? (Vector3)body.position : transform.position,
            detectionRadius);
    }
}
