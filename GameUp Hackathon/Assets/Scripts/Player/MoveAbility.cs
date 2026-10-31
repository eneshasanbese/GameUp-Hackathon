using UnityEngine;

[DisallowMultipleComponent]
public sealed class MoveAbility : BaseAbility
{
    [SerializeField, Min(0f)] private float speed = 6f;
    [SerializeField, Min(0.01f)] private float acceleration = 60f;

    [Header("Low Water Boost")]
    [Tooltip("Maximum walking speed multiplier near empty water. 1 disables the boost.")]
    [SerializeField, Min(1f)] private float lowWaterSpeedMultiplier = 1.4f;

    public override PlayerStates.State thisAbilityState => PlayerStates.State.Move;

    public override void ProcessAbility()
    {
        if (!linkedPhysicsControl.grounded)
        {
            linkedStateMachine.ChangeState(PlayerStates.State.Jump);
            return;
        }

        if (linkedInput.horizontalInput == 0f)
            linkedStateMachine.ChangeState(PlayerStates.State.Idle);
    }

    public override void ProcessFixedAbility()
    {
        float currentSpeed = speed * GetLowWaterMultiplier(lowWaterSpeedMultiplier);
        linkedPhysicsControl.ApproachHorizontalSpeed(linkedInput.horizontalInput * currentSpeed, acceleration);
    }

    public override void ExitAbility()
    {
        if (!player.isActiveAndEnabled)
            linkedPhysicsControl.SetHorizontalSpeed(0f);
    }
}
