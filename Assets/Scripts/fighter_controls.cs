using UnityEngine;
using UnityEngine.InputSystem;

public class FighterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 13f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;

    private float horizontalInput;
    private bool isGrounded;
    private bool facingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Check whether the fighter is touching the ground
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Reset movement every frame
        horizontalInput = 0f;

        // New Input System
        if (Keyboard.current.aKey.isPressed)
        {
            horizontalInput = -1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            horizontalInput = 1f;
        }

        // Jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("SPACE PRESSED");
            Debug.Log("Grounded: " + isGrounded);

            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );
            }
        }
        // if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        // {
        //     rb.linearVelocity = new Vector2(
        //         rb.linearVelocity.x,
        //         jumpForce
        //     );
        // }

        // Determine which direction we're facing
        if (horizontalInput > 0)
        {
            facingRight = true;
        }
        else if (horizontalInput < 0)
        {
            facingRight = false;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            horizontalInput * moveSpeed,
            rb.linearVelocity.y
        );
    }
}