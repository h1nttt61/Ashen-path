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
    [SerializeField] private float wallCheckDistatnce = 0.15f;
    [SerializeField] private LayerMask walllayer;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpForceX = 12f;
    [SerializeField] private float wallJumpForceY = 18f;
    [SerializeField] private float wallPushForce = 10f;

    [Header("Wall Stick Settings")]
    [SerializeField] private float wallStickTime = 0.3f;
    [SerializeField] private float wallStickGravity = 0f;
    [SerializeField] private float wallSlideGravity = 0.3f;
    [SerializeField] private float wallStickDelay = 0.5f;

    [Header("Collision Settings")]
    [SerializeField] private float skinWidth = 0.05f;
    [SerializeField] private int horizontalRays = 3;
    [SerializeField] private int verticalRays = 3;

    [Header("Wall Jump Colldown Settings")]
    [SerializeField] private float wallJumpCooldown = 0.5f;
    [SerializeField] private float weakWallJumpForceX = 8f;
    [SerializeField] private float weakWallJumpForceY = 12f;

    private bool isDashing;

    private bool isAlive;

    private bool isGrounded;

    private float initialSpeed;

    private bool isWallSliding;

    private bool isTouchingWall;

    private bool isFacingRight = true;

    private int wallDirection;

    private BoxCollider2D boxCollider;

    private float lastWallActionTime;

    private float wallActionCooldown = 0.3f;

    private float wallStickTimer = 0f;

    private bool isWallSticking = false;

    private bool wasTouchingWall = false;

    private Coroutine wallStickCoroutine;

    private float lastWallJumpTime = 0f;

    private float wallTouchStartTime = 0f;

    private bool wallTouchRegistered = false;

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
        HandleWallStickTimer();
        HandleWallSliding();
        Debug.Log($"Grounded: {isGrounded}, Wall: {isTouchingWall}, Sticking: {isWallSticking}, Sliding: {isWallSliding}, Input: {inputVector.x}");
        if (GameInput.Instance != null)
        {
            inputVector = new Vector2(GameInput.Instance.GetMovementVector().x, 0f);

            if (inputVector.x > 0.1f) isFacingRight = true;

            else if (inputVector.x < -0.1f) isFacingRight = false;

            if (isTouchingWall && !isGrounded && Mathf.Abs(inputVector.x) > 0.05f)
                if (Mathf.Sign(inputVector.x) != wallDirection)
                    TryWallPushOff();

            if (GameInput.Instance.WasJumpPressedThisFrame())
            {
                bool canWallJump = (isWallSliding || isWallSticking) && (Time.time >= lastWallJumpTime + wallJumpCooldown);

                if (isGrounded)
                    Jump();
                else if (canWallJump)
                    Jump();
            }
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

        int currentIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

        if (PlayerPositionStorage.TargetSceneIndex == currentIndex)
        {
            SpawnPoint[] allSpawns = FindObjectsOfType<SpawnPoint>();
            SpawnPoint spawn = null;

            foreach (var s in allSpawns)
            {
                if (s.spawnId == PlayerPositionStorage.TargetSpawnId)
                {
                    spawn = s;
                    break;
                }
            }

            if (spawn != null)
            {
                Vector3 pos = spawn.transform.position;
                transform.position = pos;
                rb.position = pos;
            }

            PlayerPositionStorage.TargetSceneIndex = -1;
        }
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
        if (isWallSticking)
        {
            rb.gravityScale = wallStickGravity;
        }
        else if (isWallSliding)
        {
            rb.gravityScale = wallSlideGravity;
        }
        else if (rb.linearVelocity.y < 0)
            rb.gravityScale = fallMultiplier;
        else if (rb.linearVelocity.y > 0 && !GameInput.Instance.IsJumpPressed())
            rb.gravityScale = lowJumpMultiplier;
        else
            rb.gravityScale = 1f;
    }




    private void StartWallStick()
    {
        if (wallStickCoroutine != null)
            StopCoroutine(WallStickCoroutine());
        isWallSticking = true;
        isWallSliding = false;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        wallStickTimer = wallStickTime;

        wallStickCoroutine = StartCoroutine(WallStickCoroutine());
    }

    private IEnumerator WallStickCoroutine()
    {
        float timer = 0f;

        while (timer < wallStickTime && isTouchingWall && !isGrounded)
        {
            timer += Time.deltaTime;

            bool stillHolding = Mathf.Abs(inputVector.x) > 0.05f && Mathf.Sign(inputVector.x) == Mathf.Sign(wallDirection);

            if (!stillHolding)
            {
                EndWallStick();
                yield break;
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

            yield return null;
        }

        EndWallStick();

        if (isTouchingWall && !isGrounded && Mathf.Abs(inputVector.x) > 0.05f && Mathf.Sign(inputVector.x) == Mathf.Sign(wallDirection))
        {
            yield return new WaitForSeconds(0.1f);
            isWallSliding = true;
        }
    }

    private void EndWallStick()
    {
        isWallSticking = false;
        wallStickTimer = 0f;
    }

    private void ResetWallStick()
    {
        isWallSticking = false;
        wallStickTimer = 0f;

        if (wallStickCoroutine != null)
        {
            StopCoroutine(WallStickCoroutine());
            wallStickCoroutine = null;
        }
    }

    private void TryWallPushOff()
    {
        if (Time.time < lastWallActionTime + wallActionCooldown)
            return;

        var pushX = -wallDirection * wallPushForce;
        var pushY = Mathf.Max(rb.linearVelocity.y, 3f);

        if (Mathf.Abs(inputVector.x) > 0.1f && Mathf.Sign(inputVector.x) != wallDirection)
        {
            pushX *= 1.3f;
            pushY = Mathf.Max(pushY, 5f);
        }

        Vector2 pushForce = new Vector2(pushX, pushY);
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, pushForce, 0.6f);

        rb.AddForce(pushForce * 0.2f, ForceMode2D.Impulse);

        
        isWallSliding = false;
        isWallSticking = false;
        wallStickTimer = 0f;
        wallTouchRegistered = false;

        lastWallActionTime = Time.time;
        lastWallJumpTime = Time.time;
    }
    private void HandleWallStickTimer()
    {
        if (isTouchingWall && !isGrounded && !isWallSticking && Mathf.Abs(inputVector.x) > 0.1f &&
            Mathf.Sign(inputVector.x) == Mathf.Sign(wallDirection))
        {
            if (!wallTouchRegistered)
            {
                wallTouchStartTime = Time.time;
                wallTouchRegistered = true;
            }
            else if (wallTouchRegistered && Time.time >= wallTouchStartTime + wallStickDelay)
                StartWallStick();
        }

        if (isWallSticking && (Mathf.Abs(inputVector.x) < 0.05f || Mathf.Sign(inputVector.x) != Mathf.Sign(wallDirection)))
            EndWallStick();

        /* if (isWallSticking)
         {
             wallStickTimer -= Time.deltaTime;
             if (wallStickTimer <= 0 || !isTouchingWall || isGrounded ||
                 Mathf.Abs(inputVector.x) < 0.05f ||
                 Mathf.Sign(inputVector.x) != Mathf.Sign(wallDirection))
                 EndWallStick();
         }*/
    }
    private void HandleMovement()
    {
        if (isWallSticking)
        {
            rb.linearVelocity = new Vector2(0, 0);
        }
        else if (!isWallSliding)
        {
            float currentSpeed = speed;
            if (!isGrounded)
                currentSpeed = speed * 0.8f;

            float targetVelocityX = inputVector.x * currentSpeed;
            float currentVelocityX = rb.linearVelocity.x;

            float newVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, Time.fixedDeltaTime * 10f);

            rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }


        isRunning = Mathf.Abs(inputVector.x) > minSpeed;
    }

    private void Jump()
    {
        if (isWallSliding || isWallSticking)
        {

            EndWallStick();

            float jumpDirection = -wallDirection;
            float jumpX = jumpDirection * wallJumpForceX;
            float jumpY = wallJumpForceY;
            if (Mathf.Abs(inputVector.x) > 0.1f)
            {
                if (Mathf.Abs(inputVector.x) > 0.1f && Mathf.Sign(inputVector.x) != wallDirection)
                {
                    jumpX *= 1.5f;
                    jumpY *= 1.5f;
                }
                else jumpX *= 2.5f;
            }
            Vector2 currentVelocity = rb.linearVelocity;
            Vector2 targetVelocity = new Vector2(jumpX, jumpY);
            rb.linearVelocity = Vector2.Lerp(currentVelocity, targetVelocity, 0.7f);

            rb.AddForce(new Vector2(jumpX * 0.3f, jumpY * 0.3f), ForceMode2D.Impulse);

            isWallSliding = false;
            isWallSticking = false;
            wallTouchRegistered = false;

            lastWallActionTime = Time.time;
        }
        else if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void CheckGrounded()
    {

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            ResetWallStick();
            isWallSliding = false;
        }
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

        wasTouchingWall = isTouchingWall;

        isTouchingWall = hit.collider != null;

        if (isTouchingWall && !wasTouchingWall && !isGrounded)
        {
            wallTouchStartTime = Time.time;
            wallTouchRegistered = true;
        }
        else if (!isTouchingWall && wasTouchingWall)
        {
            ResetWallStick();
            isWallSliding = false;
            isWallSticking = false;
            wallTouchRegistered = false;
        }
    }

    private void HandleWallSliding()
    {

        if (isWallSticking) { isWallSliding = true; return; }

        bool shouldSlide = isTouchingWall && !isGrounded &&
            Mathf.Abs(inputVector.x) > 0.05f && Mathf.Sign(inputVector.x) == Mathf.Sign(wallDirection) &&
            rb.linearVelocity.y <= 0;
        if (shouldSlide && canWallClimb)
        {
            isWallSliding = true;

            var targetSpeed = -wallSlideSpeed;
            var currentSpeed = rb.linearVelocity.y;

            if (currentSpeed < targetSpeed)
            {
                var newSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 3f);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, newSpeed);
            }

            else if (currentSpeed > -0.5f)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, targetSpeed);
        }
        else if (!isWallSticking)
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