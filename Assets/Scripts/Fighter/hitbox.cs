using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
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
        owner = attackOwner;
        damage = attackDamage;
        horizontalKnockback = horizontalForce;
        verticalKnockback = verticalForce;
        attackDirection = Mathf.Sign(direction);
        hitTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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
