using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public sealed class EmberSuctionWindVisual : MonoBehaviour
{
    [SerializeField] private EmberSuctionEnemy suction;
    [SerializeField, Min(0.01f)] private float idleScale = 1.5f;
    [SerializeField, Range(0f, 1f)] private float idleAlpha = 0.35f;
    [SerializeField, Min(0.01f)] private float windWidth = 1f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.25f;
    [Tooltip("Authored distance from the mouth pivot to the wind source, in unscaled world units.")]
    [SerializeField, Min(0.001f)] private float referenceWindLength = 0.758f;

    private static readonly int Idle = Animator.StringToHash("IdleWind");
    private static readonly int Pull = Animator.StringToHash("PullWind");
    private static readonly int Strong = Animator.StringToHash("PullWindStrong");
    private SpriteRenderer windRenderer;
    private Animator animator;
    private int currentAnimation;
    private bool pulling, fading, strong;
    private float side = 1f;
    private float fadeElapsed, fadeStartAlpha;

    private void OnEnable()
    {
        windRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        currentAnimation = 0;
        pulling = fading = strong = false;
        SetAlpha(0f);
    }

    private void LateUpdate()
    {
        if (suction == null || !suction.IsWindEnabled)
        {
            windRenderer.enabled = false;
            pulling = fading = strong = false;
            currentAnimation = 0;
            SetAlpha(0f);
            return;
        }
        windRenderer.enabled = true;
        if (Time.deltaTime <= 0f)
            return;

        if (suction.TryGetWindTarget(out Vector2 target, out float proximity, out float ramp))
        {
            bool continuing = pulling || fading;
            side = Mathf.Sign(target.x - suction.MouthPosition.x);
            // Hysteresis avoids alternating clips while hovering near the strong-wind threshold.
            strong = proximity >= (strong ? 0.6f : 0.7f);
            Play(strong ? Strong : Pull, continuing);
            animator.speed = Mathf.Lerp(0.85f, 1.15f, proximity);
            float length = Mathf.Abs(target.x - suction.MouthPosition.x);
            SetGeometry(Mathf.Max(0.1f, length) / referenceWindLength, windWidth * Mathf.Lerp(0.7f, 1f, proximity));
            SetAlpha(Mathf.MoveTowards(windRenderer.color.a, Mathf.Lerp(0.5f, 0.9f, proximity) * ramp, Time.deltaTime / fadeDuration));
            pulling = true;
            fading = false;
            return;
        }

        if (pulling)
        {
            pulling = false;
            fading = true;
            fadeElapsed = 0f;
            fadeStartAlpha = windRenderer.color.a;
        }
        if (fading)
        {
            // Keep the current frame phase and geometry, rather than jumping to FadeOut_00.
            fadeElapsed += Time.deltaTime;
            SetAlpha(fadeStartAlpha * Mathf.Clamp01(1f - fadeElapsed / fadeDuration));
            if (fadeElapsed < fadeDuration)
                return;
            fading = false;
        }

        strong = false;
        Play(Idle, false);
        animator.speed = 1f;
        SetGeometry(idleScale, idleScale);
        SetAlpha(Mathf.MoveTowards(windRenderer.color.a, idleAlpha, Time.deltaTime * idleAlpha / fadeDuration));
    }

    private void Play(int animation, bool preservePhase)
    {
        if (currentAnimation == animation)
            return;
        float phase = preservePhase && animator.isInitialized ? Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f) : 0f;
        animator.Play(animation, 0, phase);
        animator.Update(0f);
        currentAnimation = animation;
    }

    private void SetGeometry(float lengthScale, float widthScale)
    {
        transform.position = suction.MouthPosition;
        // Suction is horizontal. Rotate about the shared mouth pivot instead of flipping the sprite.
        transform.rotation = Quaternion.Euler(0f, 0f, side < 0f ? 180f : 0f);
        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(lengthScale / Mathf.Max(0.0001f, Mathf.Abs(parentScale.x)),
            widthScale / Mathf.Max(0.0001f, Mathf.Abs(parentScale.y)), 1f);
    }

    private void SetAlpha(float alpha)
    {
        Color color = windRenderer.color;
        color.a = alpha;
        windRenderer.color = color;
    }

    private void OnDisable()
    {
        if (windRenderer != null)
            windRenderer.enabled = false;
    }
}
