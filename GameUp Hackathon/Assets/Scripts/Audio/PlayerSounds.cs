using UnityEngine;

/// <summary>
/// Player feedback from accepted hits, source refills and the existing death flow.
/// Leave a clip empty to skip that sound.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
public class PlayerSounds : MonoBehaviour
{
    [Tooltip("Optional takeoff sound. Leave empty when the Jump clip already plays an Animation Event sound.")]
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip land;
    [Tooltip("Accepted enemy, trap or projectile damage. Immune contacts stay silent.")]
    [SerializeField] private AudioClip hurt;
    [Tooltip("Any death: water ran out, lava, or falling out of the level.")]
    [SerializeField] private AudioClip death;
    [Tooltip("Entering a water puddle (checkpoint) with less than full water.")]
    [SerializeField] private AudioClip refill;
    [Tooltip("Played once when water falls below the threshold; rearmed after recovering water.")]
    [SerializeField] private AudioClip lowWaterWarning;
    [SerializeField, Range(0.01f, 0.9f)] private float lowWaterThreshold = 0.25f;
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float landVolume = 0.4f;
    [Tooltip("Landing is silent after shorter falls, so small steps do not click.")]
    [SerializeField, Min(0f)] private float minAirTimeForLand = 0.25f;

    // A real jump leaves the ground faster than this; walking off an edge does not.
    private const float JumpSpeed = 2f;

    private Player player;
    private WaterMeter water;
    private KnockbackAbility knockback;
    private float airTime;
    private bool airborne;
    private bool jumpPlayed;
    private bool lowWaterWarned;

    private void Awake()
    {
        player = GetComponent<Player>();
        water = GetComponent<WaterMeter>();
        knockback = GetComponent<KnockbackAbility>();
    }

    private void OnEnable()
    {
        if (water != null)
        {
            water.RefilledFromSource += OnRefilled;
            water.RespawnStarted += OnDeath;
            water.WaterChanged += OnWaterChanged;
            lowWaterWarned = water.NormalizedWater <= lowWaterThreshold;
        }
        if (knockback != null)
            knockback.HitAccepted += OnHit;
    }

    private void OnDisable()
    {
        if (water != null)
        {
            water.RefilledFromSource -= OnRefilled;
            water.RespawnStarted -= OnDeath;
            water.WaterChanged -= OnWaterChanged;
        }
        if (knockback != null)
            knockback.HitAccepted -= OnHit;
        airborne = false;
        airTime = 0f;
        jumpPlayed = false;
    }

    private void Update()
    {
        bool respawning = water != null && water.IsRespawning;
        if (respawning || !player.isActiveAndEnabled || SceneFader.IsBusy)
        {
            airborne = false;
            airTime = 0f;
            return;
        }

        PlayerStates.State state = player.stateMachine.currentState;

        PhysicsControl physics = player.physicsControl;
        if (physics.grounded)
        {
            if (airborne && airTime >= minAirTimeForLand)
                Sfx.Play(land, landVolume);
            airborne = false;
            airTime = 0f;
            jumpPlayed = false;
            return;
        }

        airborne = true;
        airTime += Time.deltaTime;
        if (!jumpPlayed && state == PlayerStates.State.Jump && airTime < 0.2f &&
            physics.rb.linearVelocityY > JumpSpeed)
        {
            jumpPlayed = true;
            Sfx.Play(jump, volume);
        }
    }

    private void OnRefilled() => Sfx.Play(refill, volume);

    private void OnHit()
    {
        // A lethal hit uses the death cue instead of stacking hurt and death voices.
        if (water == null || (!water.IsEmpty && !water.IsRespawning))
            Sfx.Play(hurt, volume);
    }

    private void OnDeath() => Sfx.Play(death, volume);

    private void OnWaterChanged(float amount)
    {
        float normalized = water.NormalizedWater;
        if (normalized >= Mathf.Min(1f, lowWaterThreshold + 0.1f))
            lowWaterWarned = false;
        if (!lowWaterWarned && normalized > 0f && normalized <= lowWaterThreshold &&
            player.isActiveAndEnabled && !water.IsRespawning && !SceneFader.IsBusy)
        {
            lowWaterWarned = true;
            Sfx.Play(lowWaterWarning, volume);
        }
    }
}
