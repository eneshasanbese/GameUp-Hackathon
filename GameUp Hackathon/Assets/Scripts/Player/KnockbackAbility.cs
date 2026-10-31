using UnityEngine;

[DefaultExecutionOrder(50)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
public sealed class KnockbackAbility : BaseAbility
{
    [SerializeField] private Vector2 force = new Vector2(6f, 5f);
    [SerializeField, Min(0f)] private float controlLockDuration = 0.2f;
    [SerializeField, Min(0.01f)] private float recoveryDuration = 0.3f;
    [SerializeField, Min(0f)] private float recoveryAirspeed = 4f;
    [SerializeField, Min(0f)] private float contactWaterLoss = 10f;
    [SerializeField, Min(0f)] private float flashDuration = 2f;
    [SerializeField, Min(0.01f)] private float flashInterval = 0.1f;
    [SerializeField] private SpriteRenderer bodyVisual;

    private WaterMeter water;
    private Vector2 pendingForce;
    private bool impulsePending;
    private float elapsed;
    private float flashRemaining;
    private bool ownsVisibility;
    private bool originalVisibility;

    public override PlayerStates.State thisAbilityState => PlayerStates.State.Knockback;
    public bool IsFlashing => flashRemaining > 0f;

    protected override void Initialization()
    {
        base.Initialization();
        water = player.GetComponent<WaterMeter>();
        if (bodyVisual == null)
            bodyVisual = player.GetComponentInChildren<SpriteRenderer>();
    }

    public bool TryKnockback(Vector2 sourcePosition)
    {
        if (player == null || !isActiveAndEnabled || !isPermitted || !player.isActiveAndEnabled ||
            !linkedPhysicsControl.rb.simulated || IsFlashing || player.CurrentState == thisAbilityState ||
            (water != null && (!water.isActiveAndEnabled || water.IsEmpty || water.IsRespawning)))
            return false;

        float difference = linkedPhysicsControl.rb.position.x - sourcePosition.x;
        // Equal centres still produce a deterministic horizontal escape direction.
        float direction = Mathf.Abs(difference) > 0.001f ? Mathf.Sign(difference)
            : linkedInput.horizontalInput != 0f ? -Mathf.Sign(linkedInput.horizontalInput) : 1f;
        pendingForce = new Vector2(Mathf.Abs(force.x) * direction, Mathf.Max(0f, force.y));
        if (!linkedStateMachine.ChangeState(thisAbilityState))
            return false;

        BeginFlash();
        // A lethal hit enters the existing respawn flow, which cancels the pending impulse/flash.
        if (water != null)
            water.Consume(contactWaterLoss);
        return true;
    }

    // Projectiles share contact-hit immunity and flashing without adding a second knockback impulse.
    public bool TryWaterHit(float waterPoints)
    {
        if (waterPoints <= 0f || float.IsNaN(waterPoints) || float.IsInfinity(waterPoints) ||
            player == null || !isActiveAndEnabled || !isPermitted || !player.isActiveAndEnabled ||
            !linkedPhysicsControl.rb.simulated || IsFlashing || player.CurrentState == thisAbilityState ||
            water == null || !water.isActiveAndEnabled || water.IsEmpty || water.IsRespawning)
            return false;
        BeginFlash();
        water.Consume(waterPoints);
        return true;
    }

    private void BeginFlash()
    {
        flashRemaining = flashDuration;
        if (IsFlashing && bodyVisual != null)
        {
            originalVisibility = bodyVisual.enabled;
            ownsVisibility = true;
        }
    }

    public override void EnterAbility()
    {
        elapsed = 0f;
        impulsePending = true;
    }

    public override void ProcessFixedAbility()
    {
        if (impulsePending)
        {
            impulsePending = false;
            linkedPhysicsControl.ResetMotion();
            linkedPhysicsControl.Jump(pendingForce.y);
            linkedPhysicsControl.SetHorizontalSpeed(pendingForce.x);
        }
        elapsed += Time.fixedDeltaTime;
        if (elapsed < controlLockDuration)
            return;

        float recovery = Mathf.Clamp01((elapsed - controlLockDuration) / Mathf.Max(0.01f, recoveryDuration));
        float control = recovery * recovery * recovery;
        float target = linkedInput.horizontalInput * recoveryAirspeed;
        linkedPhysicsControl.SetHorizontalSpeed(Mathf.Lerp(linkedPhysicsControl.rb.linearVelocity.x, target, control));
        if (recovery >= 1f)
            ReturnToMovement();
    }

    private void ReturnToMovement()
    {
        linkedStateMachine.ChangeState(!linkedPhysicsControl.grounded ? PlayerStates.State.Jump
            : linkedInput.horizontalInput == 0f ? PlayerStates.State.Idle : PlayerStates.State.Move);
    }

    private void Update()
    {
        if (player == null || !player.isActiveAndEnabled)
        {
            ClearFlash();
            return;
        }
        flashRemaining = Mathf.Max(0f, flashRemaining - Time.deltaTime);
        if (!IsFlashing)
            ClearFlash();
    }

    private void LateUpdate()
    {
        if (IsFlashing && ownsVisibility && bodyVisual != null)
            bodyVisual.enabled = originalVisibility &&
                Mathf.FloorToInt((flashDuration - flashRemaining) / Mathf.Max(0.01f, flashInterval)) % 2 == 0;
    }

    private void ClearFlash()
    {
        flashRemaining = 0f;
        if (ownsVisibility && bodyVisual != null)
            bodyVisual.enabled = originalVisibility;
        ownsVisibility = false;
    }

    public override void ExitAbility()
    {
        impulsePending = false;
        if (!player.isActiveAndEnabled)
            linkedPhysicsControl.ResetMotion();
    }

    public override void OnPlayerDisabled()
    {
        ClearFlash();
        impulsePending = false;
        if (linkedStateMachine.currentState == thisAbilityState)
            linkedStateMachine.currentState = PlayerStates.State.Idle;
    }

    private void OnDisable()
    {
        ClearFlash();
        if (player != null && player.isActiveAndEnabled && linkedStateMachine.CurrentAbility == this)
            ReturnToMovement();
    }
}
