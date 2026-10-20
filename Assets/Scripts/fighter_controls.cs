using UnityEngine;
using UnityEngine.InputSystem;

public class FighterController : MonoBehaviour
{
    public enum ControlMode
    {
        Keyboard,
        Controller
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 13f;
    [SerializeField] private float downForce = 10f;
    [SerializeField] private float normalGravity = 3f;

    [Header("Controls")]
    [SerializeField] private ControlMode controlMode = ControlMode.Keyboard;

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 0.25f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private float knockbackTimer;
    private bool isGrounded;
    private bool downPressed;
    private bool facingRight = true;

    public float HorizontalInput => horizontalInput;
    public bool IsGrounded => isGrounded;
    public bool IsDownPressed => downPressed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        horizontalInput = 0f;
        downPressed = false;

        if (controlMode == ControlMode.Keyboard)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed)
                {
                    horizontalInput = -1f;
                }

                if (Keyboard.current.dKey.isPressed)
                {
                    horizontalInput = 1f;
                }

                downPressed = Keyboard.current.sKey.isPressed;

                if (Keyboard.current.spaceKey.wasPressedThisFrame &&
                    isGrounded &&
                    knockbackTimer <= 0f)
                {
                    rb.linearVelocity = new Vector2(
                        rb.linearVelocity.x,
                        jumpForce
                    );
                }
            }
        }
        else if (controlMode == ControlMode.Controller)
        {
            if (Gamepad.current != null)
            {
                Vector2 stickInput = Gamepad.current.leftStick.ReadValue();
                horizontalInput = stickInput.x;
                downPressed = stickInput.y < -0.5f;

                // Xbox A button
                if (Gamepad.current.buttonSouth.wasPressedThisFrame &&
                    isGrounded &&
                    knockbackTimer <= 0f)
                {
                    rb.linearVelocity = new Vector2(
                        rb.linearVelocity.x,
                        jumpForce
                    );
                }
            }
        }

        if (horizontalInput > 0f)
        {
            facingRight = true;
        }
        else if (horizontalInput < 0f)
        {
            facingRight = false;
        }
    }

    private void FixedUpdate()
    {
        // Preserve knockback momentum while knockback is active.
        if (knockbackTimer <= 0f)
        {
            rb.linearVelocity = new Vector2(
                horizontalInput * moveSpeed,
                rb.linearVelocity.y
            );
        }

        if (downPressed && !isGrounded)
        {
            rb.gravityScale = normalGravity - downForce;
        }
        else
        {
            rb.gravityScale = normalGravity;
        }
    }

    public void ApplyKnockback(Vector2 knockbackForce)
    {
        rb.linearVelocity = knockbackForce;
        knockbackTimer = knockbackDuration;
    }
}