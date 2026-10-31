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

    private Rigidbody2D body;
    private Vector2 start;
    private bool returning;
    private bool initialFlipX;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        start = body.position;
        if (bodyVisual != null)
            initialFlipX = bodyVisual.flipX;
    }

    private void FixedUpdate()
    {
        if (speed <= 0f || endOffset.sqrMagnitude < 0.000001f)
            return;

        Vector2 target = returning ? start : start + endOffset;
        Vector2 position = body.position;
        Vector2 next = Vector2.MoveTowards(position, target, speed * Time.fixedDeltaTime);
        if (faceMovement && bodyVisual != null && Mathf.Abs(next.x - position.x) > 0.00001f)
            bodyVisual.flipX = next.x < position.x ? !initialFlipX : initialFlipX;
        body.MovePosition(next);
        if ((next - target).sqrMagnitude < 0.000001f)
            returning = !returning;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? (Vector3)start : transform.position;
        Vector3 end = origin + (Vector3)endOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, end);
        Gizmos.DrawWireSphere(origin, 0.12f);
        Gizmos.DrawWireSphere(end, 0.12f);
    }
}
