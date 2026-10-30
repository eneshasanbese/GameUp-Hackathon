using UnityEngine;

[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
public sealed class JumpAbility : BaseAbility
{
    [SerializeField, Min(0f)] private float airspeed = 6f;
    [SerializeField, Min(0f)] private float jumpForce = 10f;

    public override PlayerStates.State thisAbilityState => PlayerStates.State.Jump;

    private void FixedUpdate()
    {
        // Jump input must be checked while Idle and Move are active as well.
        bool requested = linkedInput.ConsumeJump();
        if (!requested || !isPermitted || !player.isActiveAndEnabled || !linkedPhysicsControl.grounded)
            return;

        if (linkedStateMachine.ChangeState(thisAbilityState))
            linkedPhysicsControl.Jump(jumpForce);
    }

    public override void ProcessAbility()
    {
        if (linkedPhysicsControl.grounded)
        {
            linkedStateMachine.ChangeState(linkedInput.horizontalInput == 0f
                ? PlayerStates.State.Idle
                : PlayerStates.State.Move);
        }
    }

    public override void ProcessFixedAbility()
    {
        linkedPhysicsControl.SetHorizontalSpeed(linkedInput.horizontalInput * airspeed);
    }

    public override void ExitAbility()
    {
        linkedPhysicsControl.SetHorizontalSpeed(0f);
    }
}
