using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Kameranın altındaki bir arka plan katmanını yatayda kameradan daha yavaş kaydırır.
/// Follow Amount: 1 = ekranla birlikte sabit durur (çok uzak), 0 = dünyaya sabittir (zemin gibi).
/// Katman her Tile Width biriminde başa sarılır; bu yüzden sprite genişliği
/// (Draw Mode: Tiled) ekran genişliği + Tile Width'ten küçük olmamalıdır.
/// Konum, kamera son konumunu aldıktan sonra (çizimden hemen önce) hesaplanır;
/// Cinemachine kamerayı hangi adımda hareket ettirirse ettirsin titreme olmaz.
/// </summary>
[DisallowMultipleComponent]
public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField, Range(0f, 1f)] private float followAmount = 0.8f;
    [SerializeField, Min(0.01f)] private float tileWidth = 4f;

    private float baseLocalX;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = GetComponentInParent<Camera>();
        if (targetCamera == null || !transform.IsChildOf(targetCamera.transform))
        {
            Debug.LogWarning($"{name}: ParallaxLayer, Main Camera'nın altında olmalı.", this);
            enabled = false;
            return;
        }
        baseLocalX = transform.localPosition.x;
    }

    private void OnEnable() => RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    private void OnDisable() => RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
    {
        if (renderingCamera != targetCamera)
            return;

        float offset = -renderingCamera.transform.position.x * (1f - followAmount);
        float halfTile = tileWidth * 0.5f;
        Vector3 position = transform.localPosition;
        position.x = baseLocalX + Mathf.Repeat(offset + halfTile, tileWidth) - halfTile;
        transform.localPosition = position;
    }
}
