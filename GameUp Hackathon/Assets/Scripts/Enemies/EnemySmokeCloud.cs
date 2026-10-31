using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyHeatArea), typeof(Rigidbody2D))]
public sealed class EnemySmokeCloud : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float duration = 3f;
    private float timeRemaining;

    private void OnEnable() => timeRemaining = duration;

    private void Update()
    {
        timeRemaining -= Time.deltaTime;
        if (timeRemaining > 0f)
            return;

        // Disable the heat immediately so no drain multiplier survives the cloud's lifetime.
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
