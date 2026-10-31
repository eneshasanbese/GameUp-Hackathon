using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyHeatArea), typeof(Rigidbody2D))]
public sealed class EnemySmokeCloud : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float duration = 3f;
    [SerializeField] private SpriteRenderer smokeVisual;
    [SerializeField, Min(0f)] private float fadeInDuration = 0.25f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.5f;
    private float timeRemaining;
    private float lifetime;

    private void OnEnable()
    {
        lifetime = Mathf.Max(0.01f, duration);
        timeRemaining = lifetime;
        UpdateVisualAlpha();
    }

    private void Update()
    {
        timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
        UpdateVisualAlpha();
        if (timeRemaining > 0f)
            return;

        // Disable the heat immediately so no drain multiplier survives the cloud's lifetime.
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void UpdateVisualAlpha()
    {
        // Short-lived clouds compress both fades proportionally, keeping one full-opacity peak.
        float fadeIn = Mathf.Max(0f, fadeInDuration);
        float fadeOut = Mathf.Max(0f, fadeOutDuration);
        float fadeScale = Mathf.Min(1f, lifetime / Mathf.Max(0.001f, fadeIn + fadeOut));
        fadeIn *= fadeScale;
        fadeOut *= fadeScale;
        float alphaIn = fadeIn > 0f ? Mathf.Clamp01((lifetime - timeRemaining) / fadeIn) : 1f;
        float alphaOut = fadeOut > 0f ? Mathf.Clamp01(timeRemaining / fadeOut) : 1f;
        SetVisualAlpha(Mathf.Min(alphaIn, alphaOut));
    }

    private void SetVisualAlpha(float alpha)
    {
        if (smokeVisual == null)
            return;
        Color color = smokeVisual.color;
        color.a = alpha;
        smokeVisual.color = color;
    }

    private void OnDisable() => SetVisualAlpha(0f);
}
