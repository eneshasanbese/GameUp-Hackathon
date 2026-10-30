using UnityEngine;

public abstract class BaseAbility : MonoBehaviour
{
    protected Player player;
    protected GatherInput linkedInput;
    protected StateMachine linkedStateMachine;
    protected PhysicsControl linkedPhysicsControl;

    public abstract PlayerStates.State thisAbilityState { get; }
    public bool isPermitted = true;

    public void Initialize(Player owner)
    {
        player = owner;
        Initialization();
    }

    protected virtual void Initialization()
    {
        linkedInput = player.gatherInput;
        linkedStateMachine = player.stateMachine;
        linkedPhysicsControl = player.physicsControl;
    }

    public virtual void EnterAbility() { }
    public virtual void ExitAbility() { }
    public virtual void ProcessAbility() { }
    public virtual void ProcessFixedAbility() { }
    public virtual void UpdateAnimator() { }
}
