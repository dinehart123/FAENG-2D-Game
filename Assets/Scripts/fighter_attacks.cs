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
    [SerializeField] private FighterController.ControlMode controlMode =
        FighterController.ControlMode.Keyboard;

    [Header("Attack Hitbox")]
    [SerializeField] private GameObject attackHitbox;

    [Header("Ground Light Attacks")]
    [SerializeField] private AttackSettings neutralLight = new AttackSettings();
    [SerializeField] private AttackSettings sideLight = new AttackSettings();
    [SerializeField] private AttackSettings downLight = new AttackSettings();

    [Header("Ground Heavy Attacks")]
    [SerializeField] private AttackSettings neutralHeavy = new AttackSettings();
    [SerializeField] private AttackSettings sideHeavy = new AttackSettings();
    [SerializeField] private AttackSettings downHeavy = new AttackSettings();

    [Header("Air Light Attacks")]
    [SerializeField] private AttackSettings neutralAirLight = new AttackSettings();
    [SerializeField] private AttackSettings sideAirLight = new AttackSettings();
    [SerializeField] private AttackSettings downAirLight = new AttackSettings();

    [Header("Air Heavy Attacks")]
    [SerializeField] private AttackSettings neutralAirHeavy = new AttackSettings();
    [SerializeField] private AttackSettings sideAirHeavy = new AttackSettings();
    [SerializeField] private AttackSettings downAirHeavy = new AttackSettings();

    private FighterController fighterController;
    private Hitbox hitbox;
    private bool canAttack = true;

    private void Awake()
    {
        fighterController = GetComponent<FighterController>();

        if (attackHitbox != null)
        {
            hitbox = attackHitbox.GetComponent<Hitbox>();
            attackHitbox.SetActive(false);
        }
    }

    private void Update()
    {
        if (canAttack && AttackPressed(out AttackType attackType))
        {
            AttackDirection direction = GetAttackDirection();
            bool airAttack = !fighterController.IsGrounded;
            AttackSettings settings = GetAttackSettings(
                attackType,
                direction,
                airAttack
            );

            StartCoroutine(PerformAttack(settings));
        }
    }

    private bool AttackPressed(out AttackType attackType)
    {
        attackType = AttackType.Light;

        if (controlMode == FighterController.ControlMode.Keyboard)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.jKey.wasPressedThisFrame)
                {
                    attackType = AttackType.Light;
                    return true;
                }

                if (Keyboard.current.kKey.wasPressedThisFrame)
                {
                    attackType = AttackType.Heavy;
                    return true;
                }
            }

            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    attackType = AttackType.Light;
                    return true;
                }

                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    attackType = AttackType.Heavy;
                    return true;
                }
            }

            return false;
        }

        if (Gamepad.current == null)
        {
            return false;
        }

        // Xbox X button = light attack; Xbox Y button = heavy attack.
        if (Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            attackType = AttackType.Light;
            return true;
        }

        if (Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            attackType = AttackType.Heavy;
            return true;
        }

        return false;
    }

    private AttackDirection GetAttackDirection()
    {
        // Down takes priority over sideways movement.
        if (fighterController.IsDownPressed)
        {
            return AttackDirection.Down;
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
                    return sideAirLight;
                }

                if (direction == AttackDirection.Down)
                {
                    return downAirLight;
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

        return neutralHeavy;
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