using UnityEngine;

public class FighterHealth : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damagePercent;

    private FighterController fighterController;

    public float DamagePercent => damagePercent;

    private void Awake()
    {
        fighterController = GetComponent<FighterController>();
    }

    public void TakeDamage(float damage, Vector2 knockback)
    {
        damagePercent += damage;

        float knockbackMultiplier = 1f + damagePercent / 100f;
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