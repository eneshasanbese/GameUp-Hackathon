using UnityEngine;

[DisallowMultipleComponent]
public sealed class MoveAbility : BaseAbility
{
    [SerializeField, Min(0f)] private float speed = 6f;

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
        linkedPhysicsControl.SetHorizontalSpeed(linkedInput.horizontalInput * speed);
    }

    public override void ExitAbility()
    {
        linkedPhysicsControl.SetHorizontalSpeed(0f);
    }
}
