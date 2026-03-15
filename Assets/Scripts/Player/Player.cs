using UnityEngine;
using System;
using System.Collections;
using System.ComponentModel.Design;



[SelectionBase]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public event EventHandler OnPlayerDash;

    public static event Action<int> OnHealthChanged;

    private Rigidbody2D rb;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float damageRecoveryTime = 10f;

    [Header("Health & Respawn")]
    [SerializeField] public int maxHealth = 10;
    public int Health { get; private set; }

    private Vector3 lastCheckpointPos;
    [SerializeField] private float spikesDamageCooldown = 2f;
    [SerializeField] private bool canTakeDamage = true;

    Vector2 inputVector;
    private Camera camera;
    private readonly float minSpeed = 0.1f;
    private bool isRunning = false;

    [Header("Jump settings")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float fallMultiplier = 4f;
    [SerializeField] private float lowJumpMultiplier = 2.5f;
    private bool canJump = true;
    private bool isJumping = false;

    [Header("Dash settings")]
    [SerializeField] private int dashSpeed = 4;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float dashCooldown = 2f;

    [Header("Wall Climb Settings")]
    [SerializeField] private bool canWallClimb = true;
    [SerializeField] private float wallCheckDistatnce = 0.15f;
    [SerializeField] private LayerMask walllayer;
    [SerializeField] private float wallSlideSpeed = 0.8f;
    [SerializeField] private float wallJumpForceX = 12f;
    [SerializeField] private float wallJumpForceY = 18f;
    [SerializeField] private float wallPushForce = 10f;

    [Header("Wall Stick Settings")]
    [SerializeField] private float wallStickTime = 1f;
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

    [Header("Skills Unlocked")]
    public bool isDashUnlocked = false;

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

    private float wallJumpControlWait = 0f;

    private bool hasFinishedSticking = false;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        if (rb != null)
            rb.freezeRotation = true;
        camera = Camera.main;
        initialSpeed = speed;
        Health = maxHealth;
        lastCheckpointPos = transform.position;
    }


private void Update()
    {
        CheckGrounded();
        CheckWall();
        bool strictlyAtWall = isTouchingWall && !isGrounded;
        bool wallLogicActive = isTouchingWall;

        if (GameInput.Instance != null)
        {
            inputVector = new Vector2(GameInput.Instance.GetMovementVector().x, 0f);

            if (inputVector.x > 0.1f) isFacingRight = true;
            else if (inputVector.x < -0.1f) isFacingRight = false;

            if (GameInput.Instance.WasJumpPressedThisFrame())
            {
                if (isTouchingWall)
                {
                    Jump(); 
                }
                else if (isGrounded)
                {
                    Jump(); 
                }
            }
            else if (wallLogicActive)
            {
                float moveInput = inputVector.x;
                if (Mathf.Abs(moveInput) > 0.1f && Mathf.Sign(moveInput) != Mathf.Sign(wallDirection))
                {
                    TryWallPushOff();
                }
                else if (Mathf.Abs(moveInput) > 0.1f && Mathf.Sign(moveInput) == Mathf.Sign(wallDirection))
                {
                    HandleWallStickTimer();
                }
            }
        }

        if (!isWallSticking) HandleWallSliding();
    }

    private void FixedUpdate()
    {

        HandleWallStickTimer();

        if (!isWallSticking) HandleWallSliding();

        HandleMovement();
        ApplyGravity();
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

    public Vector3 GetScreenPlayerPosition()
    {
        Vector3 playerScreenPos = camera.WorldToScreenPoint(transform.position);
        return playerScreenPos;
    }

    public void TakeDamage(int damageAmount, Transform damageSource)
    {
        if (canTakeDamage && Health > 0)
        {
            Health -= damageAmount;
            // needs knockback i think
            //KnockBack.Instance.GetKnockedBack(damageSource);
            if (Health <= 0)
            {
                Health = 0;
                Debug.Log("player is dead no waay");
                Die();
            }

            StartCoroutine(DamageCooldownRoutine(spikesDamageCooldown));
        }
    }

    public bool IsAlive() => isAlive;

    public bool IsRunning() => isRunning;

    public void Respawn()
    {
        Health = maxHealth;
        transform.position = lastCheckpointPos;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = lastCheckpointPos;
        }
    }

    public void UpdateCheckpoint(Vector3 newPos)
    {
        lastCheckpointPos = newPos;
        Debug.Log("Checkpoint updated");
    }


    private IEnumerator DamageCooldownRoutine(float cooldown)
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(cooldown);
        canTakeDamage = true;
    }
    private void Die()
    {
        Debug.Log("Player died!");
        Respawn();
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
        if (!isDashing && isDashUnlocked)
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

    

    private void ApplyGravity()
    {
        if (isWallSticking)
        {

            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocityX, 0f);
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
        float timer = wallStickTime;
        isWallSticking = true;

        while (timer > 0 && isTouchingWall && !isGrounded)
        {

            rb.linearVelocity = new Vector2(rb.linearVelocityX, 0);

            bool stillHolding = Mathf.Abs(inputVector.x) > 0.05f && Mathf.Sign(inputVector.x) == Mathf.Sign(wallDirection);

            if (!stillHolding)
            {
                break;
            }

            timer -= Time.deltaTime;

            yield return null;
        }

        EndWallStick();

    }

    private void EndWallStick()
    {
        isWallSticking = false;
        wallStickTimer = 0f;
        hasFinishedSticking = true;
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
        if (Time.time < lastWallActionTime + wallActionCooldown) return;

        float pushX = -wallDirection * wallPushForce;
        float pushY = weakWallJumpForceY; 

        rb.linearVelocity = new Vector2(pushX, pushY);

        wallJumpControlWait = 0.15f;

        isWallSliding = false;
        isWallSticking = false;
        ResetWallStick();

        lastWallActionTime = Time.time;
    }
    private void HandleWallStickTimer()
    {
        bool pressingTowardsWall = Mathf.Abs(inputVector.x) > 0.1f && Mathf.Sign(inputVector.x) == Mathf.Sign(wallDirection);

        if (isTouchingWall && !isGrounded && pressingTowardsWall && !hasFinishedSticking)
        {
            if (!isWallSticking)
            {
                StartWallStick();
            }
        }
    }
    private void HandleMovement()
    {
        if (isWallSticking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (wallJumpControlWait > 0)
        {
            wallJumpControlWait -= Time.fixedDeltaTime;
            return;
        }

        float targetVelocityX = inputVector.x * speed;

        float acceliration = isGrounded ? 100f : 50f;

        float newX = Mathf.MoveTowards(rb.linearVelocityX, targetVelocityX, acceliration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newX, rb.linearVelocityY);

        isRunning = Mathf.Abs(inputVector.x) > minSpeed;
    }

    private void Jump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
        }
        else 
        {
            ResetWallStick();

            float jumpDir = -wallDirection;
            rb.linearVelocity = new Vector2(jumpDir * wallJumpForceX, wallJumpForceY);

            // Блокируем управление чуть дольше для сочного прыжка
            wallJumpControlWait = 0.2f;

            isWallSliding = false;
            isWallSticking = false;
            hasFinishedSticking = true; 

            lastWallJumpTime = Time.time;
            isJumping = true;
        }
    }

    private void CheckGrounded()
    {

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            canJump = true;
            isJumping = false;
            ResetWallStick();
            isWallSliding = false;
            hasFinishedSticking = false;
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
        wasTouchingWall = isTouchingWall;
        float rayDistance = wallCheckDistatnce + (boxCollider.size.x / 2);
        LayerMask combinedLayer = walllayer | groundLayer;

        RaycastHit2D rightHit = Physics2D.Raycast(transform.position, Vector2.right, rayDistance, combinedLayer);
        RaycastHit2D leftHit = Physics2D.Raycast(transform.position, Vector2.left, rayDistance, combinedLayer);

        isTouchingWall = rightHit.collider != null || leftHit.collider != null;

        if (rightHit.collider != null) wallDirection = 1;
        else if (leftHit.collider != null) wallDirection = -1;
    }

    private void HandleWallSliding()
    {

        if (isWallSticking)
        {
            isWallSliding = false;
            return;
        }

        bool shouldSlide = Mathf.Abs(inputVector.x) > 0.05f && Mathf.Sign(inputVector.x) == Mathf.Sign(wallDirection);
        if (isTouchingWall && !isGrounded && shouldSlide && rb.linearVelocity.y <= 0)
        {
            if (Time.time > wallTouchStartTime + wallStickDelay)
            {
                isWallSliding = true;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            }
        }
        else
            isWallSliding = false;
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