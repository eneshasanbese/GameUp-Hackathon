using UnityEngine;

[DisallowMultipleComponent]
public sealed class HapsirikKozSneeze : MonoBehaviour
{
    [SerializeField] private Animator visualAnimator;
    [SerializeField] private EnemySmokeCloud smokeCloudPrefab;
    [SerializeField] private bool automaticSneezing = true;
    [Tooltip("Random idle wait before each sneeze, in seconds (minimum, maximum). Starts after cooldown.")]
    [SerializeField] private Vector2 sneezeDelayRange = new Vector2(1f, 5f);
    [Tooltip("Random cooldown after the complete sneeze animation, in seconds (minimum, maximum).")]
    [SerializeField] private Vector2 cooldownRange = new Vector2(2f, 3f);

    private static readonly int IdleState = Animator.StringToHash("Base Layer.Idle");
    private static readonly int SneezeState = Animator.StringToHash("Base Layer.Sneeze");
    private static readonly int SneezeTrigger = Animator.StringToHash("Sneeze");
    private enum Phase { Waiting, Sneezing, Cooldown }
    private Phase phase;
    private float timeRemaining;
    private bool sneezeStarted;
    private bool smokeReleased;

    private void OnEnable() => ScheduleWait();

    private void Update()
    {
        if (!AnimatorReady())
            return;

        AnimatorStateInfo state = visualAnimator.GetCurrentAnimatorStateInfo(0);
        if (state.fullPathHash == SneezeState)
        {
            if (!sneezeStarted)
                smokeReleased = false;
            phase = Phase.Sneezing;
            sneezeStarted = true;
            // The source sheet marks frame 05 (zero-based) as the sneeze impact.
            if (!smokeReleased && state.normalizedTime >= 5f / 12f)
            {
                smokeReleased = true;
                if (smokeCloudPrefab != null)
                    Instantiate(smokeCloudPrefab, transform.position, Quaternion.identity);
            }
            return;
        }
        if (!IsIdle())
            return;

        if (phase == Phase.Sneezing)
        {
            // Wait for the Animator to consume the trigger and finish all twelve frames.
            if (sneezeStarted)
            {
                phase = Phase.Cooldown;
                timeRemaining = RandomSeconds(cooldownRange);
            }
            return;
        }
        if (phase == Phase.Cooldown)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
                ScheduleWait();
            return;
        }
        if (!automaticSneezing)
            return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
            Sneeze();
    }

    [ContextMenu("Sneeze")]
    public void Sneeze()
    {
        if (!isActiveAndEnabled || phase != Phase.Waiting || !IsIdle())
            return;

        phase = Phase.Sneezing;
        sneezeStarted = false;
        visualAnimator.SetTrigger(SneezeTrigger);
    }

    private bool AnimatorReady()
    {
        return visualAnimator != null && visualAnimator.isActiveAndEnabled &&
            visualAnimator.runtimeAnimatorController != null;
    }

    private bool IsIdle()
    {
        return AnimatorReady() && !visualAnimator.IsInTransition(0) &&
            visualAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash == IdleState;
    }

    private void ScheduleWait()
    {
        phase = Phase.Waiting;
        sneezeStarted = false;
        timeRemaining = RandomSeconds(sneezeDelayRange);
    }

    private static float RandomSeconds(Vector2 range)
    {
        float minimum = Mathf.Max(0f, range.x);
        return Random.Range(minimum, Mathf.Max(minimum, range.y));
    }

    private void OnValidate()
    {
        sneezeDelayRange = new Vector2(Mathf.Max(0f, sneezeDelayRange.x), Mathf.Max(0f, sneezeDelayRange.x, sneezeDelayRange.y));
        cooldownRange = new Vector2(Mathf.Max(0f, cooldownRange.x), Mathf.Max(0f, cooldownRange.x, cooldownRange.y));
    }

    private void OnDisable()
    {
        if (visualAnimator != null && visualAnimator.runtimeAnimatorController != null)
            visualAnimator.ResetTrigger(SneezeTrigger);
    }
}
