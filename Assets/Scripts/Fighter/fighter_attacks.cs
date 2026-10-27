using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FighterAttack : MonoBehaviour
{
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

    [Header("Attack Hitbox")]
    [SerializeField] private GameObject attackHitbox;

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
    private Hitbox hitbox;
    private bool canAttack = true;
    private int attackSequence = 0;

    private void Awake()
    {
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

        if (attackHitbox != null)
        {
            hitbox = attackHitbox.GetComponent<Hitbox>();
            attackHitbox.SetActive(false);
        }
    }

    private void Update()
    {
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
        if (fighterController.CurrentControlMode ==
            FighterController.ControlMode.Keyboard)
        {
            return Keyboard.current != null &&
                Keyboard.current.lKey.wasPressedThisFrame;
        }

        Gamepad gamepad = fighterController.AssignedGamepad;
        return gamepad != null && gamepad.buttonEast.wasPressedThisFrame;
    }

    private void PlayBananaAnimation()
    {
        if (animator == null)
        {
            return;
        }

        ResetAttackTriggers();
        animator.SetTrigger("Special");
    }

    private IEnumerator BananaCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(bananaCooldown);
        canAttack = true;
    }

    private bool AttackPressed(out AttackType attackType)
    {
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
        if (airAttack)
        {
            if (attackType == AttackType.Light)
            {
                if (direction == AttackDirection.Side)
                {
                    // The current Monkey controller has no Air Forward state.
                    return neutralAirLight;
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
                // The current Monkey controller has no Air Forward state.
                return neutralAirHeavy;
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

    private IEnumerator PerformAttack(
        AttackSettings settings,
        AttackType attackType,
        AttackDirection direction,
        bool airAttack)
    {
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

        if (attackHitbox != null && hitbox != null)
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

            attackHitbox.SetActive(true);
            yield return new WaitForSeconds(settings.duration);
            attackHitbox.SetActive(false);
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

    private void StartHeavyAttackMovement(AttackDirection direction)
    {
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
