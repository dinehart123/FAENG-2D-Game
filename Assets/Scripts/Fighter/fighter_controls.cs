using UnityEngine;
using UnityEngine.InputSystem;

public class FighterController : MonoBehaviour
{
    // Handles movement, jumping, dashing, knockback, and fighter animation.
    public enum ControlMode
    {
        Keyboard,
        Controller
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 13f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float downForce = 10f;
    [SerializeField] private float normalGravity = 3f;

    [Header("Controls")]
    [SerializeField] private ControlMode controlMode = ControlMode.Keyboard;
    [SerializeField] private bool startsFacingRight = true;

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 0.25f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float verticalDashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.15f;

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

    [Header("Dash Sound")]
    [SerializeField] private AudioClip dashSound;

    private Rigidbody2D rb;
    private Gamepad assignedGamepad;

    private float horizontalInput;
    private float knockbackTimer;

    private float dashTimer;
    private Vector2 dashVelocity;
    private bool isDashing;
    private bool dashAvailable = true;

    private float attackLungeTimer;
    private Vector2 attackLungeVelocity;

    private bool respawnFrozen;
    private float respawnTimer;
    private Color originalSpriteColor = Color.white;

    private bool isGrounded;
    private bool wasGrounded;
    private int jumpsRemaining;
    private bool downPressed;
    private bool upPressed;
    private bool facingRight = true;

    public float HorizontalInput => horizontalInput;
    public bool IsGrounded => isGrounded;
    public bool IsFalling => !isGrounded && rb != null && rb.linearVelocity.y < -0.1f;
    public bool IsDownPressed => downPressed;
    public bool IsUpPressed => upPressed;
    public bool FacingRight => facingRight;
    public bool IsDashing => isDashing;
    public bool IsRespawning => respawnFrozen;
    public int JumpsRemaining => jumpsRemaining;

    public ControlMode CurrentControlMode => controlMode;
    public Gamepad AssignedGamepad => assignedGamepad;

    private void Awake()
    {
        // Cache components and initialize movement and visual state.
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = Mathf.Max(1, maxJumps);
        facingRight = startsFacingRight;

        // Facing is controlled with SpriteRenderer.flipX. Do not let the
        // spawned prefab or spawn point leave the fighter rotated at 180/-180.
        transform.rotation = Quaternion.identity;

        // If this fighter is configured for controller input directly in the
        // Arena scene, use the first connected gamepad. Character selection
        // can still replace this through SetControls().
        if (controlMode == ControlMode.Controller &&
            assignedGamepad == null &&
            Gamepad.all.Count > 0)
        {
            assignedGamepad = Gamepad.all[0];
        }

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

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }

        if (spriteRenderer != null)
        {
            originalSpriteColor = spriteRenderer.color;
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
        // Process timers, grounded state, input, facing, and animation.
        if (respawnFrozen)
        {
            UpdateRespawnState();
            return;
        }

        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
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

        // Grounded fighters can dash again. An aerial dash stays unavailable
        // until the fighter touches the ground.
        if (isGrounded)
        {
            dashAvailable = true;
        }

        // Reset jumps only when landing. Resetting every grounded frame can
        // restore the counter after the first jump and allow one extra jump.
        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = Mathf.Max(1, maxJumps);
        }

        wasGrounded = isGrounded;

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
        // Read movement, dash, and jump input from the keyboard.
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

        if ((Keyboard.current.leftShiftKey.wasPressedThisFrame ||
             Keyboard.current.rightShiftKey.wasPressedThisFrame) &&
            dashAvailable &&
            knockbackTimer <= 0f &&
            !isDashing)
        {
            StartDash();
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame &&
            jumpsRemaining > 0 &&
            knockbackTimer <= 0f &&
            !isDashing)
        {
            Jump();
        }
    }

    private void HandleControllerInput()
    {
        // Read movement, dash, and jump input from the assigned gamepad.
        if (assignedGamepad == null)
        {
            return;
        }

        Vector2 stickInput = assignedGamepad.leftStick.ReadValue();

        horizontalInput = stickInput.x;
        downPressed = stickInput.y < -0.5f;
        upPressed = stickInput.y > 0.5f;

        if (assignedGamepad.leftTrigger.wasPressedThisFrame &&
            dashAvailable &&
            knockbackTimer <= 0f &&
            !isDashing)
        {
            StartDash();
        }

        if (assignedGamepad.buttonSouth.wasPressedThisFrame &&
            jumpsRemaining > 0 &&
            knockbackTimer <= 0f &&
            !isDashing)
        {
            Jump();
        }
    }

    private void StartDash()
    {
        // Begin a directional dash when the dash is available.
        if (!dashAvailable || rb == null || isDashing)
        {
            return;
        }

        // Vertical input takes priority for up and down dashes.
        if (upPressed)
        {
            dashVelocity = Vector2.up * verticalDashSpeed;
        }
        else if (downPressed)
        {
            dashVelocity = Vector2.down * verticalDashSpeed;
        }
        else if (horizontalInput > 0.1f)
        {
            dashVelocity = Vector2.right * dashSpeed;
        }
        else if (horizontalInput < -0.1f)
        {
            dashVelocity = Vector2.left * dashSpeed;
        }
        else
        {
            dashVelocity = Vector2.right * (facingRight ? dashSpeed : -dashSpeed);
        }

        isDashing = true;
        dashAvailable = false;
        dashTimer = dashDuration;

        rb.linearVelocity = dashVelocity;

        if (audioSource != null && dashSound != null)
        {
            audioSource.PlayOneShot(dashSound);
        }
    }

    private void Jump()
    {
        // Use one remaining jump and apply upward velocity.
        if (rb == null || jumpsRemaining <= 0)
        {
            return;
        }

        jumpsRemaining--;

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

    public void BeginRespawn(float duration)
    {
        // Freeze the fighter and make it flash during respawn protection.
        respawnFrozen = true;
        respawnTimer = duration;
        jumpsRemaining = Mathf.Max(1, maxJumps);
        isDashing = false;
        attackLungeTimer = 0f;
        horizontalInput = 0f;
        downPressed = false;
        upPressed = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
        }
    }

    private void UpdateRespawnState()
    {
        // Keep the fighter frozen and end protection after input or time.
        respawnTimer -= Time.deltaTime;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        if (spriteRenderer != null)
        {
            Color color = originalSpriteColor;
            color.a = Mathf.Lerp(
                0.2f,
                0.8f,
                (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f
            );
            spriteRenderer.color = color;
        }

        if (HasResumeInput() || respawnTimer <= 0f)
        {
            EndRespawn();
        }
    }

    private bool HasResumeInput()
    {
        // Check whether the player presses movement or attack input.
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame ||
                Keyboard.current.dKey.wasPressedThisFrame ||
                Keyboard.current.wKey.wasPressedThisFrame ||
                Keyboard.current.sKey.wasPressedThisFrame ||
                Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.leftShiftKey.wasPressedThisFrame ||
                Keyboard.current.rightShiftKey.wasPressedThisFrame ||
                Keyboard.current.jKey.wasPressedThisFrame ||
                Keyboard.current.kKey.wasPressedThisFrame ||
                Keyboard.current.lKey.wasPressedThisFrame)
            {
                return true;
            }
        }

        if (Mouse.current != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame))
        {
            return true;
        }

        if (assignedGamepad != null &&
            (assignedGamepad.leftStick.ReadValue().sqrMagnitude > 0.04f ||
             assignedGamepad.buttonSouth.wasPressedThisFrame ||
             assignedGamepad.leftTrigger.wasPressedThisFrame ||
             assignedGamepad.buttonWest.wasPressedThisFrame ||
             assignedGamepad.buttonNorth.wasPressedThisFrame ||
             assignedGamepad.buttonEast.wasPressedThisFrame))
        {
            return true;
        }

        return false;
    }

    private void EndRespawn()
    {
        // Restore movement and the fighter's original color.
        respawnFrozen = false;
        respawnTimer = 0f;

        if (rb != null)
        {
            rb.gravityScale = normalGravity;
            rb.linearVelocity = Vector2.zero;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalSpriteColor;
        }
    }

    private void UpdateFacingDirection()
    {
        // Face the fighter toward its current horizontal movement.
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
        // Apply physics movement, knockback, dashes, and fast-fall input.
        if (respawnFrozen)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            return;
        }

        if (isDashing)
        {
            rb.linearVelocity = dashVelocity;

            // Prevent gravity from bending the dash downward.
            rb.gravityScale = 0f;
            return;
        }

        if (attackLungeTimer > 0f)
        {
            attackLungeTimer -= Time.fixedDeltaTime;

            if (knockbackTimer <= 0f)
            {
                rb.linearVelocity = attackLungeVelocity;
            }

            if (downPressed && !isGrounded)
            {
                rb.gravityScale = normalGravity + downForce;
            }
            else
            {
                rb.gravityScale = normalGravity;
            }

            return;
        }

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
        // Assign the input mode and gamepad used by this fighter.
        controlMode = mode;
        assignedGamepad = gamepad;
    }

    public void SetFacingDirection(bool shouldFaceRight)
    {
        // Set the fighter's initial facing direction.
        facingRight = shouldFaceRight;
        transform.rotation = Quaternion.identity;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }
    }

    public void ApplyKnockback(Vector2 knockbackForce)
    {
        // Replace current velocity with attack knockback.
        if (isDashing)
        {
            return;
        }

        rb.linearVelocity = knockbackForce;
        rb.angularVelocity = 0f;
        knockbackTimer = knockbackDuration;
    }

    public void StartAttackLunge(Vector2 velocity, float duration)
    {
        // Temporarily move the fighter during a lunge attack.
        if (rb == null || isDashing || knockbackTimer > 0f)
        {
            return;
        }

        attackLungeVelocity = velocity;
        attackLungeTimer = Mathf.Max(0f, duration);
    }

    private void UpdateAnimation()
    {
        // Send movement and grounded values to the Animator.
        if (animator == null)
        {
            return;
        }

        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsFalling", IsFalling);
    }
}
