using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class EnemyHeatAuraVisual : MonoBehaviour
{
    [SerializeField] private CircleCollider2D heatCollider;
    [SerializeField] private EnemyHeatArea heatArea;
    [Tooltip("Visible diameter of Heat_00 in world units before scaling; excludes its transparent margin.")]
    [SerializeField, Min(0.001f)] private float referenceSpriteDiameter = 202f / 256f;

    private SpriteRenderer auraRenderer;

    private void OnEnable() => RefreshVisual();
    private void LateUpdate() => RefreshVisual();
    private void OnValidate() => RefreshVisual();

    private void RefreshVisual()
    {
        if (auraRenderer == null)
            auraRenderer = GetComponent<SpriteRenderer>();
        if (heatCollider == null)
            heatCollider = GetComponentInParent<CircleCollider2D>();
        if (heatArea == null)
            heatArea = GetComponentInParent<EnemyHeatArea>();
        if (auraRenderer == null)
            return;

        auraRenderer.enabled = heatCollider != null && heatCollider.enabled && heatCollider.isTrigger &&
            heatArea != null && heatArea.enabled;
        if (heatCollider == null || transform.parent != heatCollider.transform)
            return;

        transform.localPosition = heatCollider.offset;
        // Keep the authored breathing in the frames; never resize the damage collider to match it.
        float scale = 2f * heatCollider.radius / Mathf.Max(0.001f, referenceSpriteDiameter);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
