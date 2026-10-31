using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
public sealed class DeathAbility : BaseAbility
{
    private PlayerAnimation animation;

    public override PlayerStates.State thisAbilityState => PlayerStates.State.Death;
    public bool AnimationFinished => animation == null || animation.DeathFinished;

    protected override void Initialization()
    {
        base.Initialization();
        animation = player.GetComponentInChildren<PlayerAnimation>();
    }

    public override void EnterAbility()
    {
        player.StopGameplay();
        linkedPhysicsControl.ResetMotion();
        if (animation != null)
            animation.PlayDeath();
    }
}
