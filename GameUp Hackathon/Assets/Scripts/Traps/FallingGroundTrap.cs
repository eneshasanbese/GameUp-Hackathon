using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class FallingGroundTrap : MonoBehaviour
{
    [Header("Fall")]
    [SerializeField, Min(0.01f)] private float fallDelay = 0.25f;
    [SerializeField, Min(0.01f)] private float gravityScale = 3f;
    [SerializeField, Min(0.01f)] private float maximumFallSpeed = 14f;
    [SerializeField, Min(0.01f)] private float despawnDelay = 3f;

    [Header("Sounds")]
    [Tooltip("First step that arms the platform, e.g. a short crack or rumble.")]
    [SerializeField] private AudioClip warningSound;
    [Tooltip("Played once when support is released and the platform starts falling.")]
    [SerializeField] private AudioClip fallSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 0.6f;
    [SerializeField, Min(0.5f)] private float hearingDistance = 12f;

    private Rigidbody2D body;
    private BoxCollider2D groundCollider;
    private bool triggered;
    private bool falling;
    private float remaining;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        groundCollider = GetComponent<BoxCollider2D>();
        body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnEnable()
    {
        body.bodyType = falling ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
        body.gravityScale = falling ? gravityScale : 0f;
        groundCollider.enabled = !falling;
    }

    private void FixedUpdate()
    {
        if (!triggered || SceneFader.IsBusy)
            return;

        remaining -= Time.fixedDeltaTime;
        if (!falling)
        {
            if (remaining > 0f)
                return;

            falling = true;
            remaining = despawnDelay;
            Sfx.PlayAt(fallSound, transform.position, soundVolume, hearingDistance);
            // Release support so the platform cannot carry or trap the Player.
            groundCollider.enabled = false;
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = gravityScale;
            return;
        }

        body.linearVelocity = new Vector2(0f, Mathf.Max(body.linearVelocity.y, -maximumFallSpeed));
        if (remaining <= 0f)
            Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) => TryTrigger(collision);
    private void OnCollisionStay2D(Collision2D collision) => TryTrigger(collision);

    private void TryTrigger(Collision2D collision)
    {
        if (!isActiveAndEnabled || triggered || Time.timeScale <= 0f || SceneFader.IsBusy ||
            collision.rigidbody == null || !collision.rigidbody.simulated)
            return;

        Player player = collision.rigidbody.GetComponent<Player>();
        if (player == null || !player.isActiveAndEnabled || collision.collider.isTrigger)
            return;

        WaterMeter water = player.GetComponent<WaterMeter>();
        if (water != null && (!water.isActiveAndEnabled || water.IsEmpty || water.IsRespawning))
            return;

        Vector2 playerCenter = collision.collider.bounds.center;
        if (playerCenter.y <= groundCollider.bounds.center.y)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            Vector2 normal = contact.normal;
            if (Vector2.Dot(normal, playerCenter - contact.point) < 0f)
                normal = -normal;
            if (normal.y < 0.5f)
                continue;

            triggered = true;
            remaining = fallDelay;
            Sfx.PlayAt(warningSound, transform.position, soundVolume, hearingDistance);
            return;
        }
    }

    private void OnDisable()
    {
        body.linearVelocity = Vector2.zero;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
    }
}
