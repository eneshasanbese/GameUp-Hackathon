using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class EnemyHeatArea : MonoBehaviour
{
    [SerializeField, Min(1f)] private float drainMultiplier = 2f;

    private readonly Dictionary<Collider2D, WaterMeter> contacts = new Dictionary<Collider2D, WaterMeter>();
    private readonly List<Collider2D> staleContacts = new List<Collider2D>();
    private CircleCollider2D area;

    private void Awake() => area = GetComponent<CircleCollider2D>();

    private void OnTriggerEnter2D(Collider2D other) => ApplyHeat(other);
    private void OnTriggerStay2D(Collider2D other) => ApplyHeat(other);

    private void ApplyHeat(Collider2D other)
    {
        // Unity can deliver trigger messages to disabled behaviours.
        if (!isActiveAndEnabled || !area.enabled || !area.isTrigger || other.isTrigger ||
            other.attachedRigidbody == null)
            return;
        WaterMeter water = other.attachedRigidbody.GetComponent<WaterMeter>();
        Player player = other.attachedRigidbody.GetComponent<Player>();
        if (water == null || !water.isActiveAndEnabled || water.IsRespawning ||
            player == null || !player.isActiveAndEnabled)
            return;

        contacts[other] = water;
        water.SetDrainMultiplier(this, drainMultiplier);
    }

    private void OnTriggerExit2D(Collider2D other) => RemoveContact(other);

    private void FixedUpdate()
    {
        staleContacts.Clear();
        foreach (var entry in contacts)
        {
            Collider2D other = entry.Key;
            WaterMeter water = entry.Value;
            if (!area.enabled || !area.isTrigger || other == null || !other.enabled ||
                !other.gameObject.activeInHierarchy || other.attachedRigidbody == null ||
                !other.attachedRigidbody.simulated || water == null || !water.isActiveAndEnabled ||
                water.IsRespawning || !area.IsTouching(other))
                staleContacts.Add(other);
        }
        foreach (Collider2D other in staleContacts)
            RemoveContact(other);
    }

    private void RemoveContact(Collider2D other)
    {
        if (!contacts.TryGetValue(other, out WaterMeter water))
            return;
        contacts.Remove(other);
        if (water == null)
            return;
        foreach (WaterMeter remaining in contacts.Values)
        {
            if (remaining == water)
                return;
        }
        water.RemoveDrainMultiplier(this);
    }

    private void OnDisable()
    {
        foreach (WaterMeter water in contacts.Values)
        {
            if (water != null)
                water.RemoveDrainMultiplier(this);
        }
        contacts.Clear();
        staleContacts.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        Gizmos.color = new Color(1f, 0.35f, 0f, 0.8f);
        Matrix4x4 previous = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(circle.offset, circle.radius);
        Gizmos.matrix = previous;
    }
}
