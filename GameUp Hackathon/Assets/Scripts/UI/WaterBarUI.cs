using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the player's water as a horizontal bar with a portrait (T19).
/// Reads WaterMeter only and keeps no water value of its own.
/// </summary>
[DisallowMultipleComponent]
public class WaterBarUI : MonoBehaviour
{
    [Tooltip("Optional. Found automatically when left empty.")]
    [SerializeField] private WaterMeter waterMeter;
    [Tooltip("Blue fill. Its anchors follow the water amount; Left/Top/Right/Bottom are kept at 0.")]
    [SerializeField] private RectTransform fill;

    [Header("Portrait")]
    [SerializeField] private Image portrait;
    [Tooltip("Optional. Shown at or below the threshold, so low water is not shown by colour alone.")]
    [SerializeField] private Sprite lowWaterPortrait;
    [SerializeField, Range(0f, 1f)] private float lowWaterThreshold = 0.25f;

    private Sprite normalPortrait;

    private void Awake()
    {
        if (portrait != null)
            normalPortrait = portrait.sprite;
    }

    private void OnEnable()
    {
        if (waterMeter == null)
            waterMeter = FindAnyObjectByType<WaterMeter>();
        if (waterMeter == null)
        {
            Debug.LogWarning($"{name}: WaterBarUI could not find a WaterMeter.", this);
            return;
        }
        waterMeter.WaterChanged += OnWaterChanged;
        Show(waterMeter.NormalizedWater);
    }

    private void OnDisable()
    {
        if (waterMeter != null)
            waterMeter.WaterChanged -= OnWaterChanged;
    }

    // WaterChanged carries the absolute amount; the bar needs 0–1.
    private void OnWaterChanged(float water) => Show(water / WaterMeter.Capacity);

    private void Show(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        if (fill != null)
        {
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(normalized, 1f);
            fill.offsetMin = fill.offsetMax = Vector2.zero;
        }
        if (portrait != null && lowWaterPortrait != null && normalPortrait != null)
            portrait.sprite = normalized <= lowWaterThreshold ? lowWaterPortrait : normalPortrait;
    }
}
