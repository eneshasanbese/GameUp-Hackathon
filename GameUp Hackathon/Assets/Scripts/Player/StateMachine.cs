using System;

[Serializable]
public sealed class StateMachine
{
    public PlayerStates.State previousState;
    public PlayerStates.State currentState;
    public BaseAbility[] arrayOfAbilities = Array.Empty<BaseAbility>();

    public BaseAbility CurrentAbility { get; private set; }

    public bool ChangeState(PlayerStates.State state)
    {
        foreach (BaseAbility ability in arrayOfAbilities)
        {
            // During Player.OnEnable a sibling ability's OnEnable may not have run yet.
            if (ability.thisAbilityState != state || !ability.enabled ||
                !ability.gameObject.activeInHierarchy || !ability.isPermitted)
                continue;

            if (CurrentAbility == ability)
                return true;

            if (CurrentAbility != null)
                CurrentAbility.ExitAbility();
            previousState = currentState;
            currentState = state;
            CurrentAbility = ability;
            CurrentAbility.EnterAbility();
            return true;
        }

        return false;
    }

    public void ExitCurrentState()
    {
        if (CurrentAbility != null)
            CurrentAbility.ExitAbility();
        CurrentAbility = null;
    }

    public void ProcessAbility()
    {
        if (CurrentAbility != null && CurrentAbility.isActiveAndEnabled)
            CurrentAbility.ProcessAbility();
    }

    public void ProcessFixedAbility()
    {
        if (CurrentAbility != null && CurrentAbility.isActiveAndEnabled)
            CurrentAbility.ProcessFixedAbility();
    }
}
