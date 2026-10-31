using UnityEngine;

[DefaultExecutionOrder(150)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public sealed class PlayerAnimation : MonoBehaviour
{
    private static readonly int StateParameter = Animator.StringToHash("State");
    private static readonly int HealParameter = Animator.StringToHash("Heal");
    private static readonly int IdleState = Animator.StringToHash("Base Layer.Idle");

    private Animator animator;
    private Player player;
    private WaterMeter water;
    private KnockbackAbility knockback;
    private bool wasActive;

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
