using UnityEngine;

/// <summary>
/// Player sound effects. Reads the existing state machine, ground check and WaterMeter,
/// so no player code needs to change. Leave a clip empty to skip that sound.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
public class PlayerSounds : MonoBehaviour
{
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip land;
    [Tooltip("Enemy, trap or projectile hit (knockback).")]
    [SerializeField] private AudioClip hurt;
    [Tooltip("Any death: water ran out, lava, or falling out of the level.")]
    [SerializeField] private AudioClip death;
    [Tooltip("Entering a water puddle (checkpoint) with less than full water.")]
    [SerializeField] private AudioClip refill;
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float landVolume = 0.4f;
    [Tooltip("Landing is silent after shorter falls, so small steps do not click.")]
    [SerializeField, Min(0f)] private float minAirTimeForLand = 0.25f;

    // A real jump leaves the ground faster than this; walking off an edge does not.
    private const float JumpSpeed = 2f;

    private Player player;
    private WaterMeter water;
    private PlayerStates.State lastState;
    private float airTime;
    private bool airborne;
    private bool jumpPlayed;
    private bool wasRespawning;

    private void Awake()
    {
        player = GetComponent<Player>();
        water = GetComponent<WaterMeter>();
        lastState = player.stateMachine.currentState;
    }

    private void OnEnable()
    {
        if (water != null)
            water.RefilledFromSource += OnRefilled;
    }

    private void OnDisable()
    {
        if (water != null)
            water.RefilledFromSource -= OnRefilled;
    }

    private void Update()
    {
        bool respawning = water != null && water.IsRespawning;
        if (respawning && !wasRespawning)
            Sfx.Play(death, volume);
        wasRespawning = respawning;
        if (respawning || !player.isActiveAndEnabled)
        {
            airborne = false;
            airTime = 0f;
            lastState = player.stateMachine.currentState;
            return;
        }

        PlayerStates.State state = player.stateMachine.currentState;
        if (state != lastState && state == PlayerStates.State.Knockback)
            Sfx.Play(hurt, volume);
        lastState = state;

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
}
