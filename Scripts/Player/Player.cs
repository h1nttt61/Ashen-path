using UnityEngine;
using System;
using System.Collections;
using System.ComponentModel.Design;

[SelectionBase]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public event EventHandler OnPlayerDash;

    private Rigidbody2D rb;
    [SerializeField] private float speed = 5f;
    [SerializeField] public int maxHealth = 10;
    [SerializeField] private float damageRecoveryTime = 10f;

    Vector2 inputVector;
    private Camera camera;
    private readonly float minSpeed = 0.1f;
    private bool isRunning = false;

    [Header("Jump settings")]
    [SerializeField] private float jumpForce = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float fallMultiplier = 4f;
    [SerializeField] private float lowJumpMultiplier = 2.5f;

    [Header("Dash settings")]
    [SerializeField] private int dashSpeed = 4;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float dashCooldown = 2f;

    [Header("Wall Climb Settings")]
    [SerializeField] private bool canWallClimb = true;
    [SerializeField] private float wallCheckDistatnce = 0.6f;
    [SerializeField] private LayerMask walllayer;
    [SerializeField] private float wallSlidingSpace = 2f;
    [SerializeField] private float wallJumpForceX = 10f;
    [SerializeField] private float wallJumpForceY = 15f;

    [Header("Collision Settings")]
    [SerializeField] private float skinWidth = 0.05f;
    [SerializeField] private int horizontalRays = 3;
    [SerializeField] private int verticalRays = 3;

    private bool isDashing;

    private bool isAlive;

    private bool isGrounded;

    private float initialSpeed;

    private bool isWallSliding;

    private bool isTouchingWall;

    private bool isFacingRight = true;

    private int wallDirection;

    private BoxCollider2D boxCollider;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        if (rb != null)
            rb.freezeRotation = true;
        camera = Camera.main;
        initialSpeed = speed;
    }

    private void Update()
    {
        CheckGrounded();
        CheckWall();
        HandleWallSliding();
        if (GameInput.Instance != null)
        {
            inputVector = new Vector2(GameInput.Instance.GetMovementVector().x, 0f);

            if (inputVector.x > 0.1f) isFacingRight = true;

            else if (inputVector.x < -0.1f) isFacingRight = false;

            if (GameInput.Instance.WasJumpPressedThisFrame() && (isGrounded || isWallSliding))
                Jump();
        }
    }

    public Vector3 GetScreenPlayerPosition()
    {
        Vector3 playerScreenPos = camera.WorldToScreenPoint(transform.position);
        return playerScreenPos;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        ApplyGravity();
        ImprovedCollisionHandling();
    }

    public void Start()
    {
        GameInput.Instance.OnPlayerDash += OnPlayerDashh;
        GameInput.Instance.OnPlayerAttack += Player_OnPlayerAttack;
        isAlive = true;
    }
    private void Player_OnPlayerAttack(object sender, System.EventArgs e)
    {
        ActiveWeapon.Instance.GetActiveWeapon().Attack();
    }
    private void OnPlayerDashh(object sender, EventArgs e)
    {
        Dash();
    }

    private void Dash()
    {
        if (!isDashing)
            StartCoroutine(DashRutine());
    }

    private IEnumerator DashRutine()
    {
        isDashing = true;
        speed *= dashSpeed;
        trailRenderer.emitting = true;
        yield return new WaitForSeconds(dashTime);
        trailRenderer.emitting = false;
        speed = initialSpeed;
        yield return new WaitForSeconds(dashCooldown);
        isDashing = false;
    }

    public bool IsAlive() => isAlive;

    public bool IsRunning() => isRunning;

    private void ApplyGravity()
    {
        if (isWallSliding)
        {
            rb.gravityScale = 0.5f;
        }
        else if (rb.linearVelocity.y < 0)
            rb.gravityScale = fallMultiplier;
        else if (rb.linearVelocity.y > 0 && !GameInput.Instance.IsJumpPressed())
            rb.gravityScale = lowJumpMultiplier;
        else
            rb.gravityScale = 1f;
    }

    private void HandleMovement()
    {
        if (!isWallSliding)
        {
            rb.linearVelocity = new Vector2(inputVector.x * speed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    

        if (Math.Abs(inputVector.x) > minSpeed)
            isRunning = true;
        else
            isRunning = false;
    }

    private void Jump()
    {
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(-wallDirection * wallJumpForceX, wallJumpForceY);
            isWallSliding = false;

            StartCoroutine(WallJumpCooldown());
        }
        else if (isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private IEnumerator WallJumpCooldown()
    {
        canWallClimb = false;
        yield return new WaitForSeconds(0.3f);
        canWallClimb = true;
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }


    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    private void CheckWall()
    {
        wallDirection = isFacingRight ? 1 : -1;
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.right * (boxCollider.size.x / 2 * wallDirection);

        LayerMask L = groundLayer | walllayer;

        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin,
            Vector2.right * wallDirection,
            wallCheckDistatnce,
            L
            );

        isTouchingWall = hit.collider != null;
    }

    private void HandleWallSliding()
    {

        bool shouldSlide = isTouchingWall && !isGrounded &&
            Mathf.Sign(inputVector.x) == Mathf.Sign(wallDirection);
        if (shouldSlide && canWallClimb)
        {
            isWallSliding = true;
            if (rb.linearVelocity.y < -wallSlidingSpace)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlidingSpace);
        }
        else
            isWallSliding = false;
    }

    private void ImprovedCollisionHandling()
    {
        var collider = GetComponent<Collider2D>();
        if (collider == null) return;
        CheckHorizontalCollisions();
        CheckVerticalCollisions();
    }

    private void CheckHorizontalCollisions()
    {
        Bounds bounds = GetComponent<Collider2D>().bounds;

        bounds.Expand(skinWidth * -2);

        float raySpacing = bounds.size.y / (horizontalRays - 1);

        for (int i = 0; i < horizontalRays; i++)
        {
            Vector2 rayOrigin = new Vector2(
                isFacingRight ? bounds.max.x : bounds.min.x,
                bounds.min.y + raySpacing * i
                );

            RaycastHit2D hit = Physics2D.Raycast(
                rayOrigin,
                Vector2.right * (isFacingRight ? 1 : -1),
                skinWidth,
                groundLayer | walllayer);

            if (hit.collider != null)
            {
                float pushDistance = skinWidth - hit.distance;
                transform.position += new Vector3(
                    (isFacingRight ? -pushDistance : pushDistance),
                    0,
                    0);
            }
        }
    }


    private void CheckVerticalCollisions()
    {
        var collider = GetComponent<Collider2D>();
        if (collider == null) return;

        Bounds bounds = collider.bounds;

        bounds.Expand(skinWidth * -2);

        float raySpacing = bounds.size.y / (verticalRays - 1);

        for (int i = 0; i < verticalRays; i++)
        {
            Vector2 rayOrigin = new Vector2(
                bounds.min.x + raySpacing * i,
                bounds.max.y
                );

            RaycastHit2D hit = Physics2D.Raycast(
                rayOrigin,
                Vector2.up,
                skinWidth,
                groundLayer | walllayer);

            if (hit.collider != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                break;
            }
        }


        for (int i = 0; i < verticalRays; i++)
        {
            Vector2 rayOrigin = new Vector2(
                bounds.min.x + raySpacing * i,
                bounds.min.y);

            RaycastHit2D hit = Physics2D.Raycast(
                rayOrigin,
                Vector2.down,
                skinWidth,
                groundLayer | walllayer);

            if (hit.collider != null && rb.linearVelocity.y <= 0)
            {
                float pushDistance = skinWidth - hit.distance;
                transform.position += new Vector3(0, pushDistance, 0);
                break;
            }
        }
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPlayerDash -= OnPlayerDashh;
            GameInput.Instance.OnPlayerAttack -= Player_OnPlayerAttack;
        }
    }
}