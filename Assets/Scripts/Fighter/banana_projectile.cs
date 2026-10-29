using UnityEngine;

public class BananaProjectile : MonoBehaviour
{
    // Controls the banana's movement, damage, collision, and lifetime.
    [Header("Damage")]
    [SerializeField] private float damage = 8f;
    [SerializeField] private float horizontalKnockback = 8f;
    [SerializeField] private float verticalKnockback = 4f;

    [Header("Projectile")]
    [SerializeField] private float lifetime = 4f;

    [Header("Sound")]
    [SerializeField] private AudioClip bananaHitSound;
    [SerializeField] private float bananaHitVolume = 1f;

    private Rigidbody2D rb;
    private GameObject owner;
    private bool hasHit;

    private void Awake()
    {
        // Cache physics and remove the projectile after its lifetime.
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    public void Launch(GameObject projectileOwner, float direction, float speed)
    {
        // Set the owner, movement, hitbox values, and facing direction.
        owner = projectileOwner;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * speed, 0f);
        }

        Hitbox hitbox = GetComponent<Hitbox>();
        if (hitbox != null)
        {
            hitbox.Configure(
                owner,
                damage,
                horizontalKnockback,
                verticalKnockback,
                direction
            );
        }

        IgnoreOwnerCollisions();

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    private void IgnoreOwnerCollisions()
    {
        // Prevent the banana from colliding with the fighter who threw it.
        if (owner == null)
        {
            return;
        }

        Collider2D[] ownerColliders =
            owner.GetComponentsInChildren<Collider2D>();
        Collider2D[] projectileColliders =
            GetComponentsInChildren<Collider2D>();

        foreach (Collider2D ownerCollider in ownerColliders)
        {
            foreach (Collider2D projectileCollider in projectileColliders)
            {
                if (ownerCollider != null && projectileCollider != null)
                {
                    Physics2D.IgnoreCollision(
                        ownerCollider,
                        projectileCollider
                    );
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Destroy the banana after it hits another fighter.
        if (hasHit)
        {
            return;
        }

        FighterHealth target =
            other.GetComponentInParent<FighterHealth>();

        if (target != null && target.gameObject != owner)
        {
            hasHit = true;

            if (bananaHitSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    bananaHitSound,
                    transform.position,
                    bananaHitVolume
                );
            }

            Destroy(gameObject);
        }
    }
}
