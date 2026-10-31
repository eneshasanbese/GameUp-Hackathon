using UnityEngine;

[DisallowMultipleComponent]
public sealed class IdleAbility : BaseAbility
{
    [SerializeField, Min(0.01f)] private float deceleration = 80f;

    public override PlayerStates.State thisAbilityState => PlayerStates.State.Idle;

    public override void ProcessAbility()
    {
        if (!linkedPhysicsControl.grounded)
        {
            linkedStateMachine.ChangeState(PlayerStates.State.Jump);
            return;
        }

        if (linkedInput.horizontalInput != 0f)
            linkedStateMachine.ChangeState(PlayerStates.State.Move);
    }

    public override void ProcessFixedAbility()
    {
        linkedPhysicsControl.ApproachHorizontalSpeed(0f, deceleration);
    }

    public override void ExitAbility()
    {
        if (!player.isActiveAndEnabled)
            linkedPhysicsControl.SetHorizontalSpeed(0f);
    }
}
