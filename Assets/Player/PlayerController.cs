using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerControls controls;

    [Header("Movement Settings")]
    public float speed = 5f;
    public float jumpForce = 5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Wall Settings")]
    public Transform wallCheck;
    public LayerMask wallLayer;
    private bool isTouchingWall;
    private bool isWallSliding;
    public float wallSlideSpeed = 2f;

    public Vector2 wallJumpForce = new Vector2(8f, 12f); // X - push, Y - up
    private bool isWallJumping;
    public float wallJumpDuration = 0.2f;
    private float wallJumpTimer;

    [Header("Attack Settings")]
    public GameObject swordHitbox;

    [SerializeField] private Collider2D shieldHitbox;
    private bool isBlocking = false;

    private float moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        controls = new PlayerControls();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        // Movement input
        controls.Player.Move.performed += ctx => OnMove(ctx);
        controls.Player.Move.canceled += ctx => moveInput = 0f;

        // Jump input
        controls.Player.Jump.performed += ctx => Jump(ctx);

        // Attack input
        controls.Player.Attack.performed += ctx => Attack(ctx);

        // Block input
        controls.Player.Block.started += ctx => SetBlock(true);
        controls.Player.Block.canceled += ctx => SetBlock(false);
    }

    private void Update()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

        // Wall check
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, 0.1f, wallLayer);

        // Wall slide logic
        if (isTouchingWall && !isGrounded && moveInput != 0)
        {
            isWallSliding = true;
            animator.SetBool("isWallSliding", true);

            // Limit fall speed while sliding
            if (rb.linearVelocity.y < -wallSlideSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            }
        }
        else
        {
            isWallSliding = false;
            animator.SetBool("isWallSliding", false);
        }

        // Animator update

        // Horizontal movement
        animator.SetFloat("Speed", Mathf.Abs(moveInput));

        // Grounded state
        animator.SetBool("isGrounded", isGrounded);

        // Vertical velocity 
        animator.SetFloat("yVelocity", rb.linearVelocity.y);

        animator.SetBool("isWallSliding", isWallSliding);
    }

    private void FixedUpdate()
    {
        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        }
    }

    // ===================== Input Callbacks =====================

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>().x;

        // Flip character only if not wall jumping
        if (!isWallJumping)
        {
            if (moveInput > 0.01f)
                transform.localScale = new Vector3(1, 1, 1);
            else if (moveInput < -0.01f)
                transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                animator.SetTrigger("Jump");
                Debug.Log("[Player] Ground jump");
            }
            else if (isWallSliding) // Wall jump
            {
                isWallJumping = true;
                wallJumpTimer = Time.time + wallJumpDuration;

                // Push away from wall
                int wallDirection = transform.localScale.x > 0 ? 1 : -1;
                rb.linearVelocity = new Vector2(-wallDirection * wallJumpForce.x, wallJumpForce.y);

                animator.SetTrigger("Jump");
                Debug.Log("[Player] Wall jump");
            }
        }
    }

    private void Attack(InputAction.CallbackContext context)
    {
        if (!isGrounded) return;

        animator.SetTrigger("Attack");
        Debug.Log("[Player] Attack triggered");
    }

    private void SetBlock(bool state)
    {
        isBlocking = state;
        if (shieldHitbox != null)
            shieldHitbox.enabled = state;

        animator.SetBool("isBlocking", state);
        Debug.Log("[Player] Block state: " + state);
    }

    // ===================== Animation Events =====================

    public void EnableSwordHitbox()
    {
        if (swordHitbox != null)
        {
            swordHitbox.SetActive(true);
            Debug.Log("[Player] Sword hitbox enabled");
        }
    }

    public void DisableSwordHitbox()
    {
        if (swordHitbox != null)
        {
            swordHitbox.SetActive(false);
            Debug.Log("[Player] Sword hitbox disabled");
        }
    }

    public void AttackHit()
    {
        Debug.Log("[Player] Attack hit frame reached");
    }
}
