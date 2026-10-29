using UnityEngine;

public class FighterHealth : MonoBehaviour
{
    // Tracks damage, applies knockback, and handles fighter damage state.
    [Header("Damage")]
    [SerializeField] private float damagePercent;
    private float totalDamageTaken;

    [Header("Knockback Scaling")]
    [SerializeField] private float maxKnockbackMultiplier = 4f;
    [SerializeField] private float knockbackGrowth = 3f;

    [Header("Death Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathSound;

    private FighterController fighterController;

    public float DamagePercent => damagePercent;
    public float TotalDamageTaken => totalDamageTaken;

    private void Awake()
    {
        // Cache fighter components and prepare the death audio source.
        fighterController = GetComponent<FighterController>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }

    public void PlayDeathSound()
    {
        // Play the configured sound when this fighter loses a stock.
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }

    public void TakeDamage(float damage, Vector2 knockback)
    {
        // Add damage and apply knockback unless the fighter is respawning.
        // Dashing grants invulnerability: no damage and no knockback.
        if (fighterController != null &&
            (fighterController.IsDashing || fighterController.IsRespawning))
        {
            return;
        }

        damagePercent += damage;
        totalDamageTaken += damage;

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
        // Return the fighter's damage values to zero.
        damagePercent = 0f;
    }
}
