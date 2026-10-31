using UnityEngine;

[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
public sealed class JumpAbility : BaseAbility
{
    [SerializeField, Min(0f)] private float airspeed = 6f;
    [SerializeField, Min(0f)] private float jumpForce = 10f;
    [SerializeField, Min(0.01f)] private float airAcceleration = 90f;

    public override PlayerStates.State thisAbilityState => PlayerStates.State.Jump;

    private void FixedUpdate()
    {
        // Jump input must be checked while Idle and Move are active as well.
        bool requested = linkedInput.ConsumeJump();
        bool released = linkedInput.ConsumeJumpRelease();
        if (!isPermitted || !player.isActiveAndEnabled)
            return;

        if (requested && linkedPhysicsControl.grounded && linkedStateMachine.ChangeState(thisAbilityState))
            linkedPhysicsControl.Jump(jumpForce);

        if (released && linkedStateMachine.currentState == thisAbilityState && linkedPhysicsControl.rb.linearVelocityY > 0f)
            linkedPhysicsControl.rb.linearVelocityY = 0f;
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
        linkedPhysicsControl.ApproachHorizontalSpeed(linkedInput.horizontalInput * airspeed, airAcceleration);
    }

    public override void ExitAbility()
    {
        if (!player.isActiveAndEnabled)
            linkedPhysicsControl.SetHorizontalSpeed(0f);
    }
}
