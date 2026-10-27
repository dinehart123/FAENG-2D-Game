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

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Jump Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;

    private Rigidbody2D rb;
    private Gamepad assignedGamepad;

    private float horizontalInput;
    private float knockbackTimer;

    private bool isGrounded;
    private bool downPressed;
    private bool upPressed;
    private bool facingRight = true;

    public float HorizontalInput => horizontalInput;
    public bool IsGrounded => isGrounded;
    public bool IsFalling => !isGrounded && rb != null && rb.linearVelocity.y < -0.1f;
    public bool IsDownPressed => downPressed;
    public bool IsUpPressed => upPressed;
    public bool FacingRight => facingRight;

    public ControlMode CurrentControlMode => controlMode;
    public Gamepad AssignedGamepad => assignedGamepad;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

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

    private void Update()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;
        }

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }
        else
        {
            isGrounded = false;
        }

        horizontalInput = 0f;
        downPressed = false;
        upPressed = false;

        if (controlMode == ControlMode.Keyboard)
        {
            HandleKeyboardInput();
        }
        else
        {
            HandleControllerInput();
        }

        UpdateFacingDirection();
        UpdateAnimation();
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
        upPressed = Keyboard.current.wKey.isPressed;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && knockbackTimer <= 0f)
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
        upPressed = stickInput.y > 0.5f;

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

        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }

        if (animator != null)
        {
            animator.ResetTrigger("Jump");
            animator.SetTrigger("Jump");
            
        }
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

        if (spriteRenderer != null)
        {
            // The spritesheet faces right by default.
            spriteRenderer.flipX = !facingRight;
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
            rb.gravityScale = normalGravity + downForce;
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
        rb.angularVelocity = 0f;
        knockbackTimer = knockbackDuration;
    }

    private void UpdateAnimation()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsFalling", IsFalling);
    }
}
