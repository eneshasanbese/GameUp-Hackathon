using UnityEngine;

[DisallowMultipleComponent]
public sealed class MoveAbility : BaseAbility
{
    [SerializeField, Min(0f)] private float speed = 6f;
    [SerializeField, Min(0.01f)] private float acceleration = 60f;

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
        linkedPhysicsControl.ApproachHorizontalSpeed(linkedInput.horizontalInput * speed, acceleration);
    }

    public override void ExitAbility()
    {
        if (!player.isActiveAndEnabled)
            linkedPhysicsControl.SetHorizontalSpeed(0f);
    }
}
