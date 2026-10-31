using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField, Min(0f)] private float extraDryDrainPerSecond = 6f;
    [Header("Ground Idle Drain")]
    [SerializeField, Min(0f)] private float wetIdleDrainPerSecond = 0.25f;
    [SerializeField, Min(0f)] private float dryIdleDrainPerSecond = 0.5f;
    [SerializeField, Min(0f)] private float extraDryIdleDrainPerSecond = 2f;

    [Header("Ground Material Multipliers")]
    [Tooltip("Independent of the ground's wet/dry/extra dry layer. Unmarked ground uses 1.")]
    [SerializeField, Min(0.01f)] private float sandDrainMultiplier = 1f;
    [SerializeField, Min(0.01f)] private float grassDrainMultiplier = 1f;
    [SerializeField, Min(0.01f)] private float blackStoneDrainMultiplier = 1f;

    [Header("Movement Detection")]
    [SerializeField, Min(0f)] private float minimumGroundSpeed = 0.05f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer bodyVisual;
    [SerializeField, Min(0.01f)] private float recoverySpeed = 12f;
    [SerializeField] private TrailRenderer trail;

    [Header("Water Size")]
    [Tooltip("Visual and collider size near empty water. 0.45 means 45% of the full size.")]
    [SerializeField, Range(0.25f, 1f)] private float minimumScale = 0.45f;

    [Header("Refill and Respawn")]
    [SerializeField] private LayerMask waterSourceLayer;
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
    private bool reloadingScene;
    private static string respawnScenePath;
    private static Vector2 respawnCheckpoint;
    private readonly Dictionary<UnityEngine.Object, float> drainMultipliers = new Dictionary<UnityEngine.Object, float>();

    public float CurrentWater => currentWater;
    public float NormalizedWater => currentWater / Capacity;
    public bool IsEmpty => currentWater <= 0f;
    public bool IsRespawning { get; private set; }
    public Vector2 Checkpoint => checkpoint;
    public float DrainMultiplier
    {
        get
        {
            float multiplier = 1f;
            foreach (var entry in drainMultipliers)
            {
                if (entry.Key != null)
                    multiplier = Mathf.Max(multiplier, entry.Value);
            }
            return multiplier;
        }
    }
    public event Action<float> WaterChanged;
    public event Action Depleted;
    public event Action RefilledFromSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRespawn()
    {
        respawnScenePath = null;
        respawnCheckpoint = default;
    }

    private void Awake()
    {
        player = GetComponent<Player>();
        physics = GetComponent<PhysicsControl>();
        body = GetComponent<Rigidbody2D>();
        checkpoint = body.position;
        if (respawnScenePath != null && respawnScenePath == gameObject.scene.path)
        {
            checkpoint = respawnCheckpoint;
            currentWater = Capacity;
            body.position = checkpoint;
            transform.position = new Vector3(checkpoint.x, checkpoint.y, transform.position.z);
            physics.ResetMotion();
            ResetRespawn();
        }
        currentWater = Mathf.Clamp(currentWater, 0f, Capacity);
        if (bodyVisual == null)
            bodyVisual = GetComponentInChildren<SpriteRenderer>();
        if (bodyVisual != null)
        {
            visualScale = bodyVisual.transform.localScale;
            visualPosition = bodyVisual.transform.localPosition;
            spriteBottom = bodyVisual.sprite.bounds.min.y * visualScale.y;
        }
        UpdateBodySize();

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
        drainMultipliers.Clear();
        StopAllCoroutines();
        if (IsRespawning && !reloadingScene)
            RestoreControls();
        ClearTrail();
    }

    private void FixedUpdate()
    {
        UpdateBodySize();
        float speed = Mathf.Abs(body.position.x - previousPosition.x) / Time.fixedDeltaTime;
        previousPosition = body.position;
        if (IsRespawning || !player.isActiveAndEnabled || !body.simulated)
            return;
        if (IsEmpty)
        {
            RespawnAtCheckpoint();
            return;
        }
        float multiplier = DrainMultiplier;
        // Air always uses the normal rate, without the previous ground's material.
        float rate = dryDrainPerSecond;
        if (physics.grounded)
        {
            bool idle = speed <= minimumGroundSpeed;
            rate = physics.wetGrounded ? (idle ? wetIdleDrainPerSecond : wetDrainPerSecond)
                : physics.extraDryGrounded ? (idle ? extraDryIdleDrainPerSecond : extraDryDrainPerSecond)
                : (idle ? dryIdleDrainPerSecond : dryDrainPerSecond);
            rate *= GetGroundMaterialMultiplier();
        }
        Consume(rate * multiplier * Time.fixedDeltaTime);
    }

    private float GetGroundMaterialMultiplier()
    {
        switch (physics.GroundMaterial)
        {
            case GroundSurface.Material.Sand: return Mathf.Max(0.01f, sandDrainMultiplier);
            case GroundSurface.Material.Grass: return Mathf.Max(0.01f, grassDrainMultiplier);
            case GroundSurface.Material.BlackStone: return Mathf.Max(0.01f, blackStoneDrainMultiplier);
            default: return 1f;
        }
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
        if (IsRespawning)
            return;
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

        float size = physics.BodyScale;
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

    public void Refill()
    {
        if (!IsRespawning)
            SetWater(Capacity);
    }

    // Sources own their entry; overlapping heat areas use the strongest multiplier.
    public void SetDrainMultiplier(UnityEngine.Object source, float multiplier)
    {
        if (source == null || !isActiveAndEnabled || IsRespawning)
            return;
        if (float.IsNaN(multiplier) || float.IsInfinity(multiplier))
            return;
        drainMultipliers[source] = Mathf.Max(1f, multiplier);
    }

    public void RemoveDrainMultiplier(UnityEngine.Object source)
    {
        if (!ReferenceEquals(source, null))
            drainMultipliers.Remove(source);
    }

    public void SetCheckpoint(Vector2 position)
    {
        if (!IsRespawning)
            checkpoint = position;
    }
    public void ResetPositionTracking() => previousPosition = body.position;

    private void UpdateBodySize()
    {
        physics.SetBodyScale(Mathf.Lerp(minimumScale, 1f, NormalizedWater));
    }

    private void SetWater(float value)
    {
        float previous = currentWater;
        currentWater = Mathf.Clamp(value, 0f, Capacity);
        UpdateBodySize();
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
        IsRespawning = true;
        StartCoroutine(Respawn());
        if (IsEmpty && !depletionNotified)
        {
            depletionNotified = true;
            Depleted?.Invoke();
        }
    }

    private IEnumerator Respawn()
    {
        drainMultipliers.Clear();
        restorePlayerEnabled = player.enabled;
        restoreSimulated = body.simulated;
        DeathAbility death = player.GetComponent<DeathAbility>();
        bool playingDeath = player.isActiveAndEnabled && death != null &&
            player.stateMachine.ChangeState(PlayerStates.State.Death);
        if (!playingDeath)
            player.StopGameplay();
        physics.ResetMotion();
        body.simulated = false;
        ClearTrail();
        // WaterMeter remains the single owner of death/respawn; the ability only plays the animation.
        // Keep the guard set through the depletion event, including when no animation is available.
        yield return null;
        while (playingDeath && player.isActiveAndEnabled && !death.AnimationFinished)
            yield return null;

        Scene scene = gameObject.scene;
        bool canReload = !string.IsNullOrEmpty(scene.path);
#if !UNITY_EDITOR
        canReload &= Application.CanStreamedLevelBeLoaded(scene.path);
#endif
        if (canReload)
        {
            respawnScenePath = scene.path;
            respawnCheckpoint = checkpoint;
            reloadingScene = true;
            // Reloading recreates all map objects, including destroyed traps and spawned hazards.
#if UNITY_EDITOR
            AsyncOperation load = Application.CanStreamedLevelBeLoaded(scene.path)
                ? SceneManager.LoadSceneAsync(scene.path, LoadSceneMode.Single)
                : UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                    scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
            AsyncOperation load = SceneManager.LoadSceneAsync(scene.path, LoadSceneMode.Single);
#endif
            if (load != null)
            {
                yield return load;
                yield break;
            }
            reloadingScene = false;
            ResetRespawn();
        }
        // Unsaved test scenes have no scene asset to reload.
        body.position = checkpoint;
        transform.position = new Vector3(checkpoint.x, checkpoint.y, transform.position.z);
        physics.ResetMotion();
        SetWater(Capacity);
        ResetPositionTracking();
        deformation = Vector3.one;
        landingTimer = 0f;
        wasGrounded = false;
        ClearTrail();
        RestoreControls();
        UpdateVisual(0f);
    }

    private void RestoreControls()
    {
        player.stateMachine.ExitCurrentState();
        player.stateMachine.ChangeState(PlayerStates.State.Idle);
        body.simulated = restoreSimulated;
        player.enabled = restorePlayerEnabled;
        IsRespawning = false;
        if (player.isActiveAndEnabled)
            player.gatherInput.EnablePlayerMap();
    }
}
