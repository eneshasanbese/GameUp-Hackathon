using UnityEngine;
using UnityEngine.InputSystem;

// Isolated demo UI; production prefabs have no input or UI dependency.
public sealed class EmberSpitterDemoControls : MonoBehaviour
{
    [SerializeField] private WaterMeter waterMeter;
    private Vector2 startPosition;

    private void Start() => startPosition = waterMeter.Checkpoint;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) Restart();
    }

    private void Restart()
    {
        if (waterMeter == null || waterMeter.IsRespawning) return;
        waterMeter.SetCheckpoint(startPosition);
        waterMeter.RespawnAtCheckpoint();
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(16f, 110f, 850f, 28f), "Ember Spitter | A/D or arrows: move | Space: jump | R: restart");
        GUI.Label(new Rect(16f, 135f, 850f, 28f), "Approach behind the wall, then jump over it. Orange windup warns of one aimed shot.");
        GUI.Label(new Rect(16f, 160f, 850f, 28f), "Only fireballs deal damage (10 water). Wall and thin platform block shots. Water drains normally.");
        if (GUI.Button(new Rect(16f, 195f, 140f, 28f), "Restart (R)")) Restart();
    }
}
