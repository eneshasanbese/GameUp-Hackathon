using UnityEngine;

public abstract class BaseAbility : MonoBehaviour
{
    protected Player player;
    protected GatherInput linkedInput;
    protected StateMachine linkedStateMachine;
    protected PhysicsControl linkedPhysicsControl;
    private WaterMeter water;

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
        water = player.GetComponent<WaterMeter>();
    }

    protected float GetLowWaterMultiplier(float maximumMultiplier)
    {
        return water != null
            ? Mathf.Lerp(1f, maximumMultiplier, 1f - water.NormalizedWater)
            : 1f;
    }

    public virtual void EnterAbility() { }
    public virtual void ExitAbility() { }
    public virtual void OnPlayerDisabled() { }
    public virtual void ProcessAbility() { }
    public virtual void ProcessFixedAbility() { }
    public virtual void UpdateAnimator() { }
}
