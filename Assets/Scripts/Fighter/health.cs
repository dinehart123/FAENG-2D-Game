
using UnityEngine;

public class FighterHealth : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damagePercent;

    [Header("Knockback Scaling")]
    [SerializeField] private float maxKnockbackMultiplier = 4f;
    [SerializeField] private float knockbackGrowth = 3f;

    private FighterController fighterController;

    public float DamagePercent => damagePercent;

    private void Awake()
    {
        fighterController = GetComponent<FighterController>();
    }

    public void TakeDamage(float damage, Vector2 knockback)
    {
        // Dashing grants invulnerability: no damage and no knockback.
        if (fighterController != null &&
            (fighterController.IsDashing || fighterController.IsRespawning))
        {
            return;
        }

        damagePercent += damage;

        // Saturating exponential scaling:
        // knockback increases quickly, then approaches a maximum plateau.
        float normalizedDamage = damagePercent / 200f;
        float plateauProgress =
            1f - Mathf.Exp(-knockbackGrowth * normalizedDamage);

        float knockbackMultiplier = Mathf.Lerp(
            1f,
            maxKnockbackMultiplier,
            plateauProgress
        );

        Vector2 finalKnockback = knockback * knockbackMultiplier;

        if (fighterController != null)
        {
            fighterController.ApplyKnockback(finalKnockback);
        }

        Debug.Log(
            gameObject.name + " took " + damage +
            " damage. Total: " + damagePercent + "%"
        );
    }

    public void ResetDamage()
    {
        damagePercent = 0f;
    }
}

