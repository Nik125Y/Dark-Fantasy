using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class HeroController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public float wallSlideSpeed = -2.5f;
    public float wallJumpForce = 12f;
    public float wallJumpPush = 6f;

    [Header("Check Settings")]
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;
    public LayerMask platformLayer;
    public LayerMask wallLayer;
    public float checkRadius = 0.2f;

    [Header("References")]
    public Animator animator;
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;

    private PlayerControls controls;
    private float horizontal;
    private bool jumpPressed;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isOnPlatform;
    private bool isWallSliding;
    private bool isFallingThroughPlatform;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => horizontal = ctx.ReadValue<Vector2>().x;
        controls.Player.Move.canceled += _ => horizontal = 0f;

        controls.Player.Jump.started += _ =>
        {
            jumpPressed = true;
            //Debug.Log("Jump pressed");
        };
        controls.Player.Jump.canceled += _ => jumpPressed = false;

        controls.Player.Down.performed += _ =>
        {
            //Debug.Log("Down pressed");
            TryFallThroughPlatform();
        };
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        // --- Проверка земли, платформы и стены ---
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        isOnPlatform = Physics2D.OverlapCircle(groundCheck.position, checkRadius, platformLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, checkRadius, wallLayer);

        //Debug.Log($"Ground: {isGrounded}, Platform: {isOnPlatform}, Wall: {isTouchingWall}");

        // --- Wall Slide ---
        bool isPushingAgainstWall =
            (horizontal > 0 && isTouchingWall && !spriteRenderer.flipX) ||
            (horizontal < 0 && isTouchingWall && spriteRenderer.flipX);

        bool wasWallSliding = isWallSliding;
        isWallSliding = !isGrounded && rb.linearVelocity.y < 0f && isPushingAgainstWall;

        //if (isWallSliding && !wasWallSliding)
            //Debug.Log("WallSlide started");
        //else if (!isWallSliding && wasWallSliding)
            //Debug.Log("WallSlide ended");

        // --- Jump ---
        if (jumpPressed)
        {
            if (isGrounded || isOnPlatform)
            {
                Jump(Vector2.up);
            }
            else if (isWallSliding)
            {
                WallJump();
            }
        }

        // --- Flip sprite ---
        if (horizontal > 0.01f) spriteRenderer.flipX = false;
        else if (horizontal < -0.01f) spriteRenderer.flipX = true;

        // --- Анимации ---
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, wallSlideSpeed));
        }
        else
        {
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void Jump(Vector2 direction)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(direction * jumpForce, ForceMode2D.Impulse);
        animator.SetTrigger("jump");
        jumpPressed = false;
        //Debug.Log("Jump executed");
    }

    private void WallJump()
    {
        float pushDir = spriteRenderer.flipX ? 1 : -1;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(pushDir * wallJumpPush, wallJumpForce), ForceMode2D.Impulse);
        isWallSliding = false;
        animator.SetTrigger("jump");
        jumpPressed = false;
        //Debug.Log("WallJump executed");
    }

    private void TryFallThroughPlatform()
    {
        if ((isOnPlatform || isGrounded) && !isFallingThroughPlatform)
        {
            StartCoroutine(FallThroughPlatform());
        }
    }

    private IEnumerator FallThroughPlatform()
    {
        isFallingThroughPlatform = true;

        Collider2D platform = Physics2D.OverlapCircle(groundCheck.position, checkRadius, platformLayer);
        Collider2D playerCol = GetComponent<Collider2D>();

        if (platform != null)
        {
            Physics2D.IgnoreCollision(playerCol, platform, true);
            //Debug.Log("Ignoring collision with platform");
        }

        yield return new WaitForSeconds(0.4f);

        if (platform != null)
        {
            Physics2D.IgnoreCollision(playerCol, platform, false);
            //Debug.Log("Restored collision with platform");
        }

        isFallingThroughPlatform = false;
    }

    private void UpdateAnimations()
    {
        animator.SetFloat("speed", Mathf.Abs(horizontal));

        // WallSlide превалирует над jump
        if (isWallSliding)
        {
            animator.SetBool("isWallSliding", true);
            animator.SetBool("isGrounded", false);
        }
        else
        {
            animator.SetBool("isWallSliding", false);
            animator.SetBool("isGrounded", isGrounded || isOnPlatform);
        }

        animator.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        if (wallCheck != null)
            Gizmos.DrawWireSphere(wallCheck.position, checkRadius);
    }
}
