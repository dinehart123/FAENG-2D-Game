using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    // Stores attack data and applies it once to each fighter hit.
    private GameObject owner;
    private float damage;
    private float horizontalKnockback;
    private float verticalKnockback;
    private float attackDirection;
    private readonly HashSet<FighterHealth> hitTargets =
        new HashSet<FighterHealth>();

    public void Configure(
        GameObject attackOwner,
        float attackDamage,
        float horizontalForce,
        float verticalForce,
        float direction)
    {
        // Set the attack owner, damage, and knockback values.
        owner = attackOwner;
        damage = attackDamage;
        horizontalKnockback = horizontalForce;
        verticalKnockback = verticalForce;
        attackDirection = Mathf.Sign(direction);
        hitTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore invalid or repeated targets, then apply knockback and damage.
        FighterHealth target = other.GetComponentInParent<FighterHealth>();

        if (target == null || owner == null || hitTargets.Contains(target))
        {
            return;
        }

        if (target.transform.root == owner.transform.root)
        {
            return;
        }

        Vector2 knockback = new Vector2(
            attackDirection * horizontalKnockback,
            verticalKnockback
        );

        hitTargets.Add(target);
        target.TakeDamage(damage, knockback);
    }
}
