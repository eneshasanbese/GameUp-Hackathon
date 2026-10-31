using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(GatherInput), typeof(PhysicsControl))]
public class Player : MonoBehaviour
{
    public GatherInput gatherInput;
    public PhysicsControl physicsControl;
    public StateMachine stateMachine;

    private BaseAbility[] playerAbilities;

    public PlayerStates.State CurrentState => stateMachine.currentState;
    public bool IsGrounded => physicsControl.grounded;

    private void Awake()
    {
        gatherInput = GetComponent<GatherInput>();
        physicsControl = GetComponent<PhysicsControl>();
        playerAbilities = GetComponents<BaseAbility>();
        stateMachine = new StateMachine { arrayOfAbilities = playerAbilities };

        foreach (BaseAbility ability in playerAbilities)
            ability.Initialize(this);
    }

    private void OnEnable()
    {
        gatherInput.EnablePlayerMap();
        stateMachine.ChangeState(stateMachine.currentState);
    }

    private void OnDisable()
    {
        physicsControl.ClearExternalHorizontalSpeeds();
        gatherInput.DisablePlayerMap();
        stateMachine.ExitCurrentState();
        foreach (BaseAbility ability in playerAbilities)
            ability.OnPlayerDisabled();
    }

    private void Update()
    {
        Debug.Log("Current State: " + CurrentState);
        stateMachine.ProcessAbility();

        foreach (BaseAbility ability in playerAbilities)
        {
            if (ability.isActiveAndEnabled)
                ability.UpdateAnimator();
        }
    }

    private void FixedUpdate()
    {
        stateMachine.ProcessFixedAbility();
    }
}
