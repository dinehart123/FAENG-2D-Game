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
    private Gamepad assignedGamepad;

    private float horizontalInput;
    private float knockbackTimer;

    private bool isGrounded;
    private bool downPressed;
    private bool facingRight = true;

    public float HorizontalInput => horizontalInput;
    public bool IsGrounded => isGrounded;
    public bool IsDownPressed => downPressed;
    public bool FacingRight => facingRight;

    public ControlMode CurrentControlMode => controlMode;
    public Gamepad AssignedGamepad => assignedGamepad;

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
            HandleKeyboardInput();
        }
        else
        {
            HandleControllerInput();
        }

        UpdateFacingDirection();
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            horizontalInput = -1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            horizontalInput = 1f;
        }

        downPressed = Keyboard.current.sKey.isPressed;

        if ((Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame) &&
            isGrounded &&
            knockbackTimer <= 0f)
        {
            Jump();
        }
    }

    private void HandleControllerInput()
    {
        if (assignedGamepad == null)
        {
            return;
        }

        Vector2 stickInput = assignedGamepad.leftStick.ReadValue();

        horizontalInput = stickInput.x;
        downPressed = stickInput.y < -0.5f;

        if (assignedGamepad.buttonSouth.wasPressedThisFrame &&
            isGrounded &&
            knockbackTimer <= 0f)
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );
    }

    private void UpdateFacingDirection()
    {
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

    public void SetControls(
        ControlMode mode,
        Gamepad gamepad = null)
    {
        controlMode = mode;
        assignedGamepad = gamepad;
    }

    public void ApplyKnockback(Vector2 knockbackForce)
    {
        rb.linearVelocity = knockbackForce;
        knockbackTimer = knockbackDuration;
    }
}