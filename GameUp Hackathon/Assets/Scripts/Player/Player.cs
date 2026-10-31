using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(GatherInput), typeof(PhysicsControl))]
public class Player : MonoBehaviour
{
    public GatherInput gatherInput;
    public PhysicsControl physicsControl;
    public StateMachine stateMachine;

    private BaseAbility[] playerAbilities;
    private SpriteRenderer bodyVisual;

    public PlayerStates.State CurrentState => stateMachine.currentState;
    public bool IsGrounded => physicsControl.grounded;
    public bool FacingRight => bodyVisual == null || !bodyVisual.flipX;
    public event System.Action GameplayStopped;

    private void Awake()
    {
        gatherInput = GetComponent<GatherInput>();
        physicsControl = GetComponent<PhysicsControl>();
        bodyVisual = GetComponentInChildren<SpriteRenderer>();
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
        GameplayStopped?.Invoke();
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
        if (CurrentState != PlayerStates.State.Knockback)
            Flip();

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

    public void Flip()
    {
        if (bodyVisual != null && gatherInput.horizontalInput != 0f)
            bodyVisual.flipX = gatherInput.horizontalInput < 0f;
    }
}
