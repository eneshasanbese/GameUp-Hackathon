using UnityEngine;

[DisallowMultipleComponent]
public sealed class IdleAbility : BaseAbility
{
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

    public override void EnterAbility()
    {
        linkedPhysicsControl.SetHorizontalSpeed(0f);
    }

    public override void ProcessFixedAbility()
    {
        linkedPhysicsControl.SetHorizontalSpeed(0f);
    }
}
