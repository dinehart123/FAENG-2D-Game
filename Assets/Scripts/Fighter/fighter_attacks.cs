using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FighterAttack : MonoBehaviour
{
    // Reads attack input, plays animations, and enables attack hitboxes.
    private enum AttackType
    {
        Light,
        Heavy
    }

    private enum AttackDirection
    {
        Neutral,
        Side,
        Up,
        Down
    }

    [System.Serializable]
    private class AttackSettings
    {
        public float damage = 10f;
        public float horizontalKnockback = 8f;
        public float verticalKnockback = 4f;
        public float duration = 0.15f;
        public float cooldown = 0.35f;
    }

    [Header("Controls")]
    [SerializeField] private float bananaCooldown = 0.5f;

    [Header("Banana Projectile")]
    [SerializeField] private GameObject bananaProjectilePrefab;
    [SerializeField] private Transform bananaSpawnPoint;
    [SerializeField] private float bananaLaunchSpeed = 12f;

    [Header("Attack Knockback")]
    [SerializeField] private float lightKnockbackMultiplier = 0.65f;
    [SerializeField] private float heavyKnockbackMultiplier = 1f;

    [Header("Heavy Attack Movement")]
    [SerializeField] private float heavyAttackLungeSpeed = 6f;
    [SerializeField] private float heavyAttackLungeDuration = 0.12f;

    [Header("Air Heavy Cooldown")]
    [SerializeField] private float airHeavyCooldown = 0.8f;

    [Header("Air Light Cooldown")]
    [SerializeField] private float airLightCooldown = 0.35f;

    [Header("Attack Sounds")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] lightAttackSounds = new AudioClip[5];
    [SerializeField] private AudioClip[] heavyAttackSounds = new AudioClip[5];

    [Header("Banana Sounds")]
    [SerializeField] private AudioClip bananaThrowSound;
    [SerializeField] private float bananaThrowVolume = 1f;

    [Header("Ground Light Hitboxes")]
    [SerializeField] private GameObject neutralLightHitbox;
    [SerializeField] private GameObject sideLightHitbox;
    [SerializeField] private GameObject upLightHitbox;
    [SerializeField] private GameObject downLightHitbox;

    [Header("Ground Heavy Hitboxes")]
    [SerializeField] private GameObject neutralHeavyHitbox;
    [SerializeField] private GameObject sideHeavyHitbox;
    [SerializeField] private GameObject upHeavyHitbox;
    [SerializeField] private GameObject downHeavyHitbox;

    [Header("Air Light Hitboxes")]
    [SerializeField] private GameObject neutralAirLightHitbox;
    [SerializeField] private GameObject sideAirLightHitbox;
    [SerializeField] private GameObject upAirLightHitbox;
    [SerializeField] private GameObject downAirLightHitbox;

    [Header("Air Heavy Hitboxes")]
    [SerializeField] private GameObject neutralAirHeavyHitbox;
    [SerializeField] private GameObject sideAirHeavyHitbox;
    [SerializeField] private GameObject upAirHeavyHitbox;
    [SerializeField] private GameObject downAirHeavyHitbox;

    [Header("Ground Light Attacks")]
    [SerializeField] private AttackSettings neutralLight = new AttackSettings();
    [SerializeField] private AttackSettings sideLight = new AttackSettings();
    [SerializeField] private AttackSettings upLight = new AttackSettings();
    [SerializeField] private AttackSettings downLight = new AttackSettings();

    [Header("Ground Heavy Attacks")]
    [SerializeField] private AttackSettings neutralHeavy = new AttackSettings();
    [SerializeField] private AttackSettings sideHeavy = new AttackSettings();
    [SerializeField] private AttackSettings upHeavy = new AttackSettings();
    [SerializeField] private AttackSettings downHeavy = new AttackSettings();

    [Header("Air Light Attacks")]
    [SerializeField] private AttackSettings neutralAirLight = new AttackSettings();
    [SerializeField] private AttackSettings sideAirLight = new AttackSettings();
    [SerializeField] private AttackSettings upAirLight = new AttackSettings();
    [SerializeField] private AttackSettings downAirLight = new AttackSettings();

    [Header("Air Heavy Attacks")]
    [SerializeField] private AttackSettings neutralAirHeavy = new AttackSettings();
    [SerializeField] private AttackSettings sideAirHeavy = new AttackSettings();
    [SerializeField] private AttackSettings upAirHeavy = new AttackSettings();
    [SerializeField] private AttackSettings downAirHeavy = new AttackSettings();

    private FighterController fighterController;
    private Animator animator;
    private bool canAttack = true;
    private int attackSequence = 0;

    private GameObject[] AllHitboxes => new[]
    {
        neutralLightHitbox,
        sideLightHitbox,
        upLightHitbox,
        downLightHitbox,
        neutralHeavyHitbox,
        sideHeavyHitbox,
        upHeavyHitbox,
        downHeavyHitbox,
        neutralAirLightHitbox,
        sideAirLightHitbox,
        upAirLightHitbox,
        downAirLightHitbox,
        neutralAirHeavyHitbox,
        sideAirHeavyHitbox,
        upAirHeavyHitbox,
        downAirHeavyHitbox
    };

    private void Awake()
    {
        // Cache fighter components and disable hitboxes at startup.
        fighterController = GetComponent<FighterController>();
        animator = GetComponent<Animator>();

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

        foreach (GameObject hitboxObject in AllHitboxes)
        {
            if (hitboxObject != null)
            {
                hitboxObject.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        // Stop attacks when the fighter is disabled or destroyed.
        ResetAttackState();
    }

    /// <summary>
    /// Clears attack state when the fighter dies or is disabled. Unity stops
    /// coroutines on disabled objects, so a cooldown coroutine may otherwise
    /// leave canAttack false forever after respawning.
    /// </summary>
    public void ResetAttackState()
    {
        // Cancel attack coroutines, triggers, and active hitboxes.
        StopAllCoroutines();
        canAttack = true;
        attackSequence++;

        DisableAllHitboxes();

        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
            ResetAttackTriggers();
        }
    }

    private void Update()
    {
        // Check for projectile and normal attack input each frame.
        if (fighterController != null && fighterController.IsRespawning)
        {
            return;
        }

        if (canAttack && BananaPressed())
        {
            PlayBananaAnimation();
            StartCoroutine(BananaCooldown());
            return;
        }

        if (canAttack && AttackPressed(out AttackType attackType))
        {
            AttackDirection direction = GetAttackDirection();
            bool airAttack = !fighterController.IsGrounded;
            AttackSettings settings = GetAttackSettings(
                attackType,
                direction,
                airAttack
            );

            PlayAttackAnimation(attackType, direction, airAttack);

            StartCoroutine(PerformAttack(
                settings,
                attackType,
                direction,
                airAttack
            ));
        }
    }

    private bool BananaPressed()
    {
        // Return true when the current control scheme throws a banana.
        if (fighterController.CurrentControlMode ==
            FighterController.ControlMode.Keyboard)
        {
            return Keyboard.current != null &&
                (Keyboard.current.leftAltKey.wasPressedThisFrame ||
                 Keyboard.current.rightAltKey.wasPressedThisFrame);
        }

        Gamepad gamepad = fighterController.AssignedGamepad;
        return gamepad != null && gamepad.buttonEast.wasPressedThisFrame;
    }

    private void PlayBananaAnimation()
    {
        // Trigger the throw animation, sound, and projectile spawn.
        if (animator != null)
        {
            ResetAttackTriggers();
            animator.SetTrigger("Special");
        }

        if (audioSource != null && bananaThrowSound != null)
        {
            audioSource.PlayOneShot(bananaThrowSound, bananaThrowVolume);
        }

        SpawnBananaProjectile();
    }

    private void SpawnBananaProjectile()
    {
        // Create a banana and launch it in the fighter's facing direction.
        if (bananaProjectilePrefab == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " has no Banana Projectile Prefab assigned."
            );
            return;
        }

        float direction = fighterController.FacingRight ? 1f : -1f;
        Transform spawnTransform = bananaSpawnPoint != null
            ? bananaSpawnPoint
            : transform;

        GameObject banana = Instantiate(
            bananaProjectilePrefab,
            spawnTransform.position,
            Quaternion.identity
        );

        BananaProjectile projectile = banana.GetComponent<BananaProjectile>();
        if (projectile == null)
        {
            Debug.LogError(
                "Banana Projectile Prefab needs a BananaProjectile component."
            );
            Destroy(banana);
            return;
        }

        projectile.Launch(
            gameObject,
            direction,
            bananaLaunchSpeed
        );
    }

    private IEnumerator BananaCooldown()
    {
        // Prevent another banana throw until the cooldown expires.
        canAttack = false;
        yield return new WaitForSeconds(bananaCooldown);
        canAttack = true;
    }

    private bool AttackPressed(out AttackType attackType)
    {
        // Detect light or heavy attack input for the active control scheme.
        attackType = AttackType.Light;

        if (fighterController.CurrentControlMode ==
            FighterController.ControlMode.Keyboard)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.jKey.wasPressedThisFrame ||
                    (Mouse.current != null &&
                    Mouse.current.leftButton.wasPressedThisFrame))
                {
                    attackType = AttackType.Light;
                    return true;
                }

                if (Keyboard.current.kKey.wasPressedThisFrame ||
                    (Mouse.current != null &&
                    Mouse.current.rightButton.wasPressedThisFrame))
                {
                    attackType = AttackType.Heavy;
                    return true;
                }
            }

            return false;
        }

        Gamepad gamepad = fighterController.AssignedGamepad;

        if (gamepad == null)
        {
            return false;
        }

        if (gamepad.buttonWest.wasPressedThisFrame)
        {
            attackType = AttackType.Light;
            return true;
        }

        if (gamepad.buttonNorth.wasPressedThisFrame)
        {
            attackType = AttackType.Heavy;
            return true;
        }

        return false;
    }

    private AttackDirection GetAttackDirection()
    {
        // Convert movement input into neutral, side, up, or down.
        // Vertical input takes priority over sideways movement.
        if (fighterController.IsDownPressed)
        {
            return AttackDirection.Down;
        }

        if (fighterController.IsUpPressed)
        {
            return AttackDirection.Up;
        }

        if (Mathf.Abs(fighterController.HorizontalInput) > 0.1f)
        {
            return AttackDirection.Side;
        }

        return AttackDirection.Neutral;
    }

    private AttackSettings GetAttackSettings(
        AttackType attackType,
        AttackDirection direction,
        bool airAttack)
    {
        // Select the configured attack values for direction and air state.
        if (airAttack)
        {
            if (attackType == AttackType.Light)
            {
                if (direction == AttackDirection.Side)
                {
                    // The controller uses the neutral air animation, but the
                    // side-air attack still gets its own editable hitbox and
                    // attack settings.
                    return sideAirLight;
                }

                if (direction == AttackDirection.Down)
                {
                    return downAirLight;
                }

                if (direction == AttackDirection.Up)
                {
                    return upAirLight;
                }

                return neutralAirLight;
            }

            if (direction == AttackDirection.Side)
            {
                return sideAirHeavy;
            }

            if (direction == AttackDirection.Down)
            {
                return downAirHeavy;
            }

            if (direction == AttackDirection.Up)
            {
                return upAirHeavy;
            }

            return neutralAirHeavy;
        }

        if (attackType == AttackType.Light)
        {
            if (direction == AttackDirection.Side)
            {
                return sideLight;
            }

            if (direction == AttackDirection.Down)
            {
                return downLight;
            }

            if (direction == AttackDirection.Up)
            {
                return upLight;
            }

            return neutralLight;
        }

        if (direction == AttackDirection.Side)
        {
            return sideHeavy;
        }

        if (direction == AttackDirection.Down)
        {
            return downHeavy;
        }

        if (direction == AttackDirection.Up)
        {
            return upHeavy;
        }

        return neutralHeavy;
    }

    private void PlayAttackAnimation(
        AttackType attackType,
        AttackDirection direction,
        bool airAttack)
    {
        // Reset old triggers and activate the matching attack animation.
        if (animator == null)
        {
            return;
        }

        // There are no Air Forward animations in the Monkey controller.
        // While airborne, pressing A/D should use the air-neutral attack
        // instead of trying to play a ground Forward attack.
        if (airAttack && direction == AttackDirection.Side)
        {
            direction = AttackDirection.Neutral;
        }

        string directionName;

        if (direction == AttackDirection.Down)
        {
            directionName = "Down";
        }
        else if (direction == AttackDirection.Up)
        {
            directionName = "Up";
        }
        else if (direction == AttackDirection.Side)
        {
            directionName = "Forward";
        }
        else
        {
            directionName = "Neutral";
        }

        string strengthName = attackType == AttackType.Light
            ? "Light"
            : "Heavy";

        // This controller uses separate direction and strength triggers.
        // Example: Forward + Light selects the Forward Light animation.
        ResetAttackTriggers();
        animator.SetTrigger(directionName);
        animator.SetTrigger(strengthName);

        // Play the attack sound when the attack is triggered.
        // This does not require Animation Events.
        if (attackType == AttackType.Light)
        {
            PlayRandomAttackSound(lightAttackSounds);
        }
        else
        {
            PlayRandomAttackSound(heavyAttackSounds);
        }
    }

    private void ResetAttackTriggers()
    {
        animator.ResetTrigger("Neutral");
        animator.ResetTrigger("Forward");
        animator.ResetTrigger("Up");
        animator.ResetTrigger("Down");
        animator.ResetTrigger("Light");
        animator.ResetTrigger("Heavy");
        animator.ResetTrigger("Special");
    }

    // Add this as an Animation Event on the impact frame of every light attack.
    public void PlayLightAttackSound()
    {
        PlayRandomAttackSound(lightAttackSounds);
    }

    // Add this as an Animation Event on the impact frame of every heavy attack.
    public void PlayHeavyAttackSound()
    {
        PlayRandomAttackSound(heavyAttackSounds);
    }

    private void PlayRandomAttackSound(AudioClip[] sounds)
    {
        // Play one valid sound chosen randomly from the supplied array.
        if (audioSource == null || sounds == null || sounds.Length == 0)
        {
            return;
        }

        int validSoundCount = 0;

        foreach (AudioClip sound in sounds)
        {
            if (sound != null)
            {
                validSoundCount++;
            }
        }

        if (validSoundCount == 0)
        {
            return;
        }

        int selectedSound = Random.Range(0, validSoundCount);
        int validIndex = 0;

        foreach (AudioClip sound in sounds)
        {
            if (sound == null)
            {
                continue;
            }

            if (validIndex == selectedSound)
            {
                audioSource.PlayOneShot(sound);
                return;
            }

            validIndex++;
        }
    }

    private GameObject GetAttackHitbox(
        AttackType attackType,
        AttackDirection direction,
        bool airAttack)
    {
        // Return the hitbox matching the attack type and direction.
        if (airAttack)
        {
            if (attackType == AttackType.Light)
            {
                switch (direction)
                {
                    case AttackDirection.Side:
                        return sideAirLightHitbox;
                    case AttackDirection.Up:
                        return upAirLightHitbox;
                    case AttackDirection.Down:
                        return downAirLightHitbox;
                    default:
                        return neutralAirLightHitbox;
                }
            }

            switch (direction)
            {
                case AttackDirection.Side:
                    return sideAirHeavyHitbox;
                case AttackDirection.Up:
                    return upAirHeavyHitbox;
                case AttackDirection.Down:
                    return downAirHeavyHitbox;
                default:
                    return neutralAirHeavyHitbox;
            }
        }

        if (attackType == AttackType.Light)
        {
            switch (direction)
            {
                case AttackDirection.Side:
                    return sideLightHitbox;
                case AttackDirection.Up:
                    return upLightHitbox;
                case AttackDirection.Down:
                    return downLightHitbox;
                default:
                    return neutralLightHitbox;
            }
        }

        switch (direction)
        {
            case AttackDirection.Side:
                return sideHeavyHitbox;
            case AttackDirection.Up:
                return upHeavyHitbox;
            case AttackDirection.Down:
                return downHeavyHitbox;
            default:
                return neutralHeavyHitbox;
        }
    }

    private void DisableAllHitboxes()
    {
        // Turn off every attack hitbox before enabling a new one.
        foreach (GameObject hitboxObject in AllHitboxes)
        {
            if (hitboxObject != null)
            {
                hitboxObject.SetActive(false);
            }
        }
    }

    private IEnumerator PerformAttack(
        AttackSettings settings,
        AttackType attackType,
        AttackDirection direction,
        bool airAttack)
    {
        // Enable the attack hitbox, wait for its active time, and cool down.
        int myAttackSequence = ++attackSequence;
        canAttack = false;

        if (attackType == AttackType.Heavy)
        {
            StartHeavyAttackMovement(direction);
        }

        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
        }

        GameObject activeHitbox = GetAttackHitbox(
            attackType,
            direction,
            airAttack
        );
        Hitbox hitbox = activeHitbox == null
            ? null
            : activeHitbox.GetComponent<Hitbox>();

        if (activeHitbox != null && hitbox != null)
        {
            float knockbackMultiplier = attackType == AttackType.Light
                ? lightKnockbackMultiplier
                : heavyKnockbackMultiplier;

            float attackDirection = fighterController.FacingRight ? 1f : -1f;

            hitbox.Configure(
                gameObject,
                settings.damage,
                settings.horizontalKnockback * knockbackMultiplier,
                settings.verticalKnockback
                    * knockbackMultiplier,
                attackDirection
            );

            MirrorHitboxToFacingDirection(activeHitbox, attackDirection);
            DisableAllHitboxes();
            activeHitbox.SetActive(true);
            yield return new WaitForSeconds(settings.duration);
            activeHitbox.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(settings.duration);
        }

        // Ground lights can chain after their active hitbox window.
        // Air lights and all heavies must wait for their cooldown.
        if (attackType == AttackType.Light &&
            !airAttack &&
            myAttackSequence == attackSequence)
        {
            canAttack = true;
        }

        float cooldown = settings.cooldown;

        if (airAttack && attackType == AttackType.Heavy)
        {
            cooldown = Mathf.Max(cooldown, airHeavyCooldown);
        }
        else if (airAttack && attackType == AttackType.Light)
        {
            cooldown = Mathf.Max(cooldown, airLightCooldown);
        }

        yield return new WaitForSeconds(cooldown);

        // A newer light may already have started. Do not let an older
        // coroutine unlock the Animator in the middle of that attack.
        if (myAttackSequence == attackSequence)
        {
            canAttack = true;

            if (animator != null)
            {
                animator.SetBool("IsAttacking", false);
            }
        }
    }

    private void MirrorHitboxToFacingDirection(
        GameObject hitboxObject,
        float facingDirection)
    {
        // Flip side-attack hitboxes to match the fighter's facing direction.
        Transform hitboxTransform = hitboxObject.transform;
        Vector3 localPosition = hitboxTransform.localPosition;
        localPosition.x = Mathf.Abs(localPosition.x) * facingDirection;
        hitboxTransform.localPosition = localPosition;

        BoxCollider2D collider = hitboxObject.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Vector2 colliderOffset = collider.offset;
            colliderOffset.x = Mathf.Abs(colliderOffset.x) * facingDirection;
            collider.offset = colliderOffset;
        }
    }

    private void StartHeavyAttackMovement(AttackDirection direction)
    {
        // Apply forward or directional movement during a heavy attack.
        float facingDirection = fighterController.FacingRight ? 1f : -1f;
        Vector2 movement;

        switch (direction)
        {
            case AttackDirection.Up:
                movement = Vector2.up * heavyAttackLungeSpeed;
                break;

            case AttackDirection.Down:
                movement = Vector2.down * heavyAttackLungeSpeed;
                break;

            case AttackDirection.Side:
            case AttackDirection.Neutral:
            default:
                movement = Vector2.right * facingDirection * heavyAttackLungeSpeed;
                break;
        }

        fighterController.StartAttackLunge(
            movement,
            heavyAttackLungeDuration
        );
    }
}
