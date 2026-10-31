using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class EnemyContactKnockback : MonoBehaviour
{
    private Collider2D contact;

    private void Awake() => contact = GetComponent<Collider2D>();
    private void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    private void OnTriggerStay2D(Collider2D other) => TryHit(other);

    private void TryHit(Collider2D other)
    {
        // Root Rigidbody callbacks also receive events from child heat areas. Only the body may hit.
        if (!isActiveAndEnabled || !contact.enabled || !contact.isTrigger || other.isTrigger ||
            other.attachedRigidbody == null || !contact.IsTouching(other))
            return;
        KnockbackAbility knockback = other.attachedRigidbody.GetComponent<KnockbackAbility>();
        if (knockback != null)
            knockback.TryKnockback(contact.bounds.center);
    }
}
