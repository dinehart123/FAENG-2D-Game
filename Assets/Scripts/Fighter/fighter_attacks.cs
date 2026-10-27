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

            StartCoroutine(PerformAttack(settings));
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

    private IEnumerator PerformAttack(AttackSettings settings)
    {
        canAttack = false;

        if (attackHitbox != null && hitbox != null)
        {
            hitbox.Configure(
                gameObject,
                settings.damage,
                settings.horizontalKnockback,
                settings.verticalKnockback
            );

            attackHitbox.SetActive(true);
            yield return new WaitForSeconds(settings.duration);
            attackHitbox.SetActive(false);
        }

        yield return new WaitForSeconds(settings.cooldown);
        canAttack = true;
    }
}
