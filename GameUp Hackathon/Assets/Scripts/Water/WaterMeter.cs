using System;
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(100)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
public sealed class WaterMeter : MonoBehaviour
{
    public const float Capacity = 100f;

    [Header("Water")]
    [SerializeField, Range(0f, Capacity)] private float currentWater = Capacity;
    [SerializeField, Min(0f)] private float dryDrainPerSecond = 3f;
    [SerializeField, Min(0f)] private float wetDrainPerSecond = 1f;
    [SerializeField, Min(0f)] private float minimumGroundSpeed = 0.05f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer bodyVisual;
    [SerializeField, Range(0.7f, 1f)] private float minimumScale = 0.7f;
    [SerializeField, Min(0.01f)] private float recoverySpeed = 12f;
    [SerializeField] private TrailRenderer trail;

    [Header("Refill and Respawn")]
    [SerializeField] private LayerMask waterSourceLayer;
    [SerializeField, Min(0.01f)] private float evaporationDuration = 0.3f;
    [SerializeField] private float fallLimit = -12f;

    private Player player;
    private PhysicsControl physics;
    private Rigidbody2D body;
    private Vector2 previousPosition;
    private Vector2 checkpoint;
    private Vector3 visualScale;
    private Vector3 visualPosition;
    private Vector3 deformation = Vector3.one;
    private float spriteBottom;
    private float landingTimer;
    private bool wasGrounded;
    private bool depletionNotified;
    private bool restorePlayerEnabled;
    private bool restoreSimulated;
    private SpriteRenderer[] renderers;
    private Color[] colors;

    public float CurrentWater => currentWater;
    public float NormalizedWater => currentWater / Capacity;
    public bool IsEmpty => currentWater <= 0f;
    public bool IsRespawning { get; private set; }
    public Vector2 Checkpoint => checkpoint;
    public event Action<float> WaterChanged;
    public event Action Depleted;
    public event Action RefilledFromSource;

    private void Awake()
    {
        player = GetComponent<Player>();
        physics = GetComponent<PhysicsControl>();
        body = GetComponent<Rigidbody2D>();
        checkpoint = body.position;
        currentWater = Mathf.Clamp(currentWater, 0f, Capacity);
        if (bodyVisual == null)
            bodyVisual = GetComponentInChildren<SpriteRenderer>();
        if (bodyVisual != null)
        {
            visualScale = bodyVisual.transform.localScale;
            visualPosition = bodyVisual.transform.localPosition;
            spriteBottom = bodyVisual.sprite.bounds.min.y * visualScale.y;
        }
        renderers = GetComponentsInChildren<SpriteRenderer>();
        colors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            colors[i] = renderers[i].color;

        if (trail == null)
            return;
        trail.time = 0.6f;
        trail.minVertexDistance = 0.07f;
        trail.endWidth = 0f;
        trail.numCapVertices = trail.numCornerVertices = 4;
        Color color = bodyVisual != null ? bodyVisual.color : Color.cyan;
        trail.colorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(0.55f, 0f), new GradientAlphaKey(0f, 1f) }
        };
        ClearTrail();
    }

    private void OnEnable()
    {
        ResetPositionTracking();
        if (IsEmpty)
            RespawnAtCheckpoint();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (IsRespawning)
            RestoreControls();
        ClearTrail();
    }

    private void FixedUpdate()
    {
        float speed = Mathf.Abs(body.position.x - previousPosition.x) / Time.fixedDeltaTime;
        previousPosition = body.position;
        if (IsRespawning || !player.isActiveAndEnabled || !body.simulated)
            return;
        if (IsEmpty)
        {
            RespawnAtCheckpoint();
            return;
        }
        if (physics.grounded && speed <= minimumGroundSpeed)
            return;

        float rate = physics.wetGrounded ? wetDrainPerSecond : dryDrainPerSecond;
        Consume(rate * Time.fixedDeltaTime);
    }

    private void Update()
    {
        if (player.isActiveAndEnabled && body.position.y < fallLimit)
            RespawnAtCheckpoint();
    }

    private void LateUpdate()
    {
        UpdateVisual(Time.deltaTime);
    }

    private void UpdateVisual(float deltaTime)
    {
        if (physics.grounded && !wasGrounded)
            landingTimer = 0.16f;
        wasGrounded = physics.grounded;
        Vector3 shape = physics.grounded
            ? Vector3.Lerp(new Vector3(1.04f, 0.96f, 1f), new Vector3(1.18f, 0.85f, 1f), Mathf.Abs(body.linearVelocity.x) / 6f)
            : new Vector3(0.85f, 1.18f, 1f);
        bool landing = physics.grounded && landingTimer > 0f;
        if (landing)
            shape = Vector3.Lerp(shape, new Vector3(1.3f, 0.75f, 1f), landingTimer / 0.16f);
        landingTimer = Mathf.Max(0f, landingTimer - deltaTime);
        deformation = Vector3.Lerp(deformation, shape, 1f - Mathf.Exp(-(landing ? 30f : recoverySpeed) * deltaTime));

        float size = Mathf.Lerp(minimumScale, 1f, NormalizedWater);
        if (bodyVisual != null)
        {
            bodyVisual.transform.localScale = Vector3.Scale(visualScale, deformation) * size;
            bodyVisual.transform.localPosition = visualPosition + Vector3.up * spriteBottom * (1f - size * deformation.y);
        }
        if (trail == null)
            return;
        if (bodyVisual != null)
            trail.transform.position = bodyVisual.bounds.center;
        trail.startWidth = 0.33f * size;
        trail.emitting = player.isActiveAndEnabled && body.simulated && !IsEmpty &&
            body.linearVelocity.sqrMagnitude > minimumGroundSpeed * minimumGroundSpeed;
    }

    public void Consume(float amount)
    {
        if (amount <= 0f || IsEmpty || IsRespawning)
            return;
        SetWater(currentWater - amount);
    }

    public void Refill() => SetWater(Capacity);
    public void SetCheckpoint(Vector2 position) => checkpoint = position;
    public void ResetPositionTracking() => previousPosition = body.position;

    private void SetWater(float value)
    {
        float previous = currentWater;
        currentWater = Mathf.Clamp(value, 0f, Capacity);
        if (!IsEmpty)
            depletionNotified = false;
        if (!Mathf.Approximately(previous, currentWater))
            WaterChanged?.Invoke(currentWater);
        if (IsEmpty)
            RespawnAtCheckpoint();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool needsWater = currentWater < Capacity;
        if (RefillFromSource(other) && needsWater)
            RefilledFromSource?.Invoke();
    }

    private void OnTriggerStay2D(Collider2D other) => RefillFromSource(other);

    private bool RefillFromSource(Collider2D other)
    {
        if (IsRespawning || !player.isActiveAndEnabled || (waterSourceLayer.value & (1 << other.gameObject.layer)) == 0)
            return false;
        Refill();
        Transform point = other.transform.Find("RespawnPoint");
        if (point != null)
            checkpoint = point.position;
        return true;
    }

    public void ClearTrail()
    {
        if (trail == null)
            return;
        trail.emitting = false;
        if (bodyVisual != null)
            trail.transform.position = bodyVisual.bounds.center;
        trail.Clear();
    }

    public void RespawnAtCheckpoint()
    {
        if (IsRespawning || !isActiveAndEnabled)
            return;
        if (IsEmpty && !depletionNotified)
        {
            depletionNotified = true;
            Depleted?.Invoke();
        }
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        IsRespawning = true;
        restorePlayerEnabled = player.enabled;
        restoreSimulated = body.simulated;
        player.enabled = false;
        physics.ResetMotion();
        body.simulated = false;
        ClearTrail();
        for (float elapsed = 0f; elapsed < evaporationDuration; elapsed += Time.deltaTime)
        {
            SetOpacity(1f - elapsed / evaporationDuration);
            yield return null;
        }
        body.position = checkpoint;
        transform.position = new Vector3(checkpoint.x, checkpoint.y, transform.position.z);
        physics.ResetMotion();
        Refill();
        ResetPositionTracking();
        deformation = Vector3.one;
        landingTimer = 0f;
        wasGrounded = false;
        UpdateVisual(0f);
        ClearTrail();
        RestoreControls();
    }

    private void RestoreControls()
    {
        SetOpacity(1f);
        body.simulated = restoreSimulated;
        player.enabled = restorePlayerEnabled;
        IsRespawning = false;
    }

    private void SetOpacity(float opacity)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Color color = colors[i];
            color.a *= opacity;
            renderers[i].color = color;
        }
    }
}
