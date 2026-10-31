using UnityEngine;
using UnityEngine.InputSystem;

// Used only by KozOcagiDemo; the production enemy has no input or UI dependency.
public sealed class EmberSuctionDemoControls : MonoBehaviour
{
    [SerializeField] private WaterMeter waterMeter;
    private Vector2 startPosition;

    private void Start() => startPosition = waterMeter.Checkpoint;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            ReturnToStart();
    }

    private void ReturnToStart()
    {
        if (waterMeter == null || waterMeter.IsRespawning)
            return;
        waterMeter.SetCheckpoint(startPosition);
        waterMeter.RespawnAtCheckpoint();
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(16f, 110f, 620f, 30f), "Koz Ocagi | A/D or arrows: move | Space: jump | R: return to start");
        GUI.Label(new Rect(16f, 135f, 620f, 30f), "Start is safe. Approach the furnace; walk away to escape. Water is shown above.");
        if (GUI.Button(new Rect(16f, 165f, 160f, 28f), "Return to start (R)"))
            ReturnToStart();
    }
}
