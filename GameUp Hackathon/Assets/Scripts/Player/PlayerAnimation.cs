using UnityEngine;

[DefaultExecutionOrder(150)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public sealed class PlayerAnimation : MonoBehaviour
{
    private static readonly int StateParameter = Animator.StringToHash("State");
    private static readonly int HealParameter = Animator.StringToHash("Heal");
    private static readonly int IdleState = Animator.StringToHash("Base Layer.Idle");
    private static readonly int DeathState = Animator.StringToHash("Base Layer.Death");

    private Animator animator;
    private Player player;
    private WaterMeter water;
    private KnockbackAbility knockback;
    private bool wasActive;

    public bool DeathFinished
    {
        get
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            return !isActiveAndEnabled || !animator.isActiveAndEnabled ||
                animator.runtimeAnimatorController == null || !animator.HasState(0, DeathState) ||
                (animator.GetCurrentAnimatorStateInfo(0).fullPathHash == DeathState &&
                 animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        }
    }

    public void PlayDeath()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (!isActiveAndEnabled || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
            return;
        animator.ResetTrigger(HealParameter);
        animator.SetInteger(StateParameter, (int)PlayerStates.State.Death);
        if (animator.HasState(0, DeathState))
            animator.Play(DeathState, 0, 0f);
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        player = GetComponentInParent<Player>();
        water = GetComponentInParent<WaterMeter>();
        knockback = GetComponentInParent<KnockbackAbility>();
    }

    private void OnEnable()
    {
        if (water != null)
            water.RefilledFromSource += PlayHeal;
    }

    private void OnDisable()
    {
        if (water != null)
            water.RefilledFromSource -= PlayHeal;
        animator.ResetTrigger(HealParameter);
        wasActive = false;
    }

    private void Update()
    {
        if (player != null && player.isActiveAndEnabled && player.CurrentState == PlayerStates.State.Death)
        {
            animator.ResetTrigger(HealParameter);
            animator.SetInteger(StateParameter, (int)PlayerStates.State.Death);
            wasActive = true;
            return;
        }
        bool active = player != null && player.isActiveAndEnabled &&
            (water == null || !water.IsRespawning);
        if (!active)
        {
            if (wasActive)
            {
                animator.ResetTrigger(HealParameter);
                animator.SetInteger(StateParameter, (int)PlayerStates.State.Idle);
                animator.Play(IdleState, 0, 0f);
            }
            wasActive = false;
            return;
        }

        wasActive = true;
        bool hitVisual = knockback != null && knockback.IsFlashing;
        if (hitVisual)
            animator.ResetTrigger(HealParameter);
        animator.SetInteger(StateParameter, (int)(hitVisual ? PlayerStates.State.Knockback : player.CurrentState));
    }

    private void PlayHeal()
    {
        if (player != null && player.isActiveAndEnabled && animator.isActiveAndEnabled &&
            !water.IsRespawning && (knockback == null || !knockback.IsFlashing) &&
            player.CurrentState != PlayerStates.State.Knockback)
            animator.SetTrigger(HealParameter);
    }
}
