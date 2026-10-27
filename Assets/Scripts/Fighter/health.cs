using UnityEngine;

public class FighterHealth : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damagePercent;

    [Header("Knockback Scaling")]
    [SerializeField] private float maxKnockbackMultiplier = 4f;
    [SerializeField] private float knockbackGrowth = 3f;

    [Header("Death Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathSound;

    private FighterController fighterController;

    public float DamagePercent => damagePercent;

    private void Awake()
    {
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
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
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
