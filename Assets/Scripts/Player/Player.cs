using UnityEngine;
using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Diagnostics.Tracing;



[SelectionBase]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public event EventHandler OnPlayerDash;

    public static event Action<int> OnHealthChanged;
    public static event Action<float> OnHealProgressChanged;

    private Rigidbody2D rb;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float damageRecoveryTime = 10f;

    [Header("Health & Respawn")]
    [SerializeField] public int maxHealth = 10;
    public int Health { get; private set; }
    [SerializeField] private int lowHealthOnSpawn = 1;

    [Header("Heal Settings (Charge System)")]
    [SerializeField] private float chargeSpeed = 0.2f; 
    [SerializeField] private float healFillSpeed = 1.5f;
    private float currentHealCharge = 0f;
    private bool isRegenerating = false;

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
    public bool isWallJumpUnlocked = false;

    private bool isDashing;

    private bool isHealButtonHeld = false;

    private bool isAlive;

    public bool isGrounded;

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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
            else if (wallLogicActive && isWallJumpUnlocked)
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
        if (!isRegenerating && currentHealCharge < maxHealth)
        {
            currentHealCharge += Time.deltaTime * chargeSpeed;
            currentHealCharge = Mathf.Min(currentHealCharge, (float)maxHealth);
            OnHealProgressChanged?.Invoke(currentHealCharge / maxHealth);
        }

        if (!isWallSticking) HandleWallSliding();
    }

    private void FixedUpdate()
    {

        if (isWallJumpUnlocked)
        {
            HandleWallStickTimer();
        }

        if (!isWallSticking)
        {
            HandleWallSliding();
        }

        HandleMovement();
        ApplyGravity();
    }

    public void Start()
    {
        SaveManager.LoadGame();



        if (!PlayerPrefs.HasKey("PlayerHealth"))
        {
            string lastCheckpoint = SaveManager.GetLastCheckpointID();
            if (string.IsNullOrEmpty(lastCheckpoint))
            {
                Health = maxHealth;
            }
            else
            {
                Health = lowHealthOnSpawn;
            }
            OnHealthChanged?.Invoke(Health);
        }

        GameInput.Instance.OnPlayerDash += OnPlayerDashh;
        GameInput.Instance.OnPlayerAttack += Player_OnPlayerAttack;

        GameInput.Instance.OnPlayerHealHoldStarted += StartHealingFromInput;
        GameInput.Instance.OnPlayerHealHoldEnded += StopHealingFromInput;

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
    public void InitializeHealth(int value)
    {
        Health = value;
        OnHealthChanged?.Invoke(Health); 
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
            OnHealthChanged?.Invoke(Health);
            
            KnockBack kb = GetComponent<KnockBack>();
            if (kb != null)
            {
                float knockBackChance = 1.0f;
                if (UnityEngine.Random.value <= knockBackChance)
                {
                    kb.GetKnockedBack(damageSource);
                }
            }
            if (Health <= 0)
            {
                Health = 0;
                Die();
            }

            StartCoroutine(DamageCooldownRoutine(spikesDamageCooldown));
        }
    }

    public bool IsAlive() => isAlive;

    public bool IsRunning() => isRunning;

    public bool isJump() => isJumping;

    public bool isGrouned() => isGrounded;

    public void Respawn()
    {
        Health = lowHealthOnSpawn;
        OnHealthChanged?.Invoke(Health);
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
    public void Die()
    {
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
            StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;

        BossAI boss = FindFirstObjectByType<BossAI>();
        PolygonCollider2D bossCol = null;

        if (boss != null)
        {
            bossCol = boss.GetComponent<PolygonCollider2D>();
            if (bossCol != null) bossCol.isTrigger = true; 
        }

        speed *= dashSpeed;
        trailRenderer.emitting = true;

        yield return new WaitForSeconds(dashTime);

        if (bossCol != null) bossCol.isTrigger = false;

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
            rb.linearVelocity = new Vector2(0, 0); 

            bool stillHolding = Mathf.Abs(inputVector.x) > 0.05f && Mathf.Sign(inputVector.x) == Mathf.Sign(wallDirection);

            if (!stillHolding) break;

            timer -= Time.deltaTime;
            yield return null;
        }

        isWallSticking = false;
        wallStickCoroutine = null;

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

        if (isTouchingWall && !isGrounded && pressingTowardsWall && !hasFinishedSticking && rb.linearVelocity.y <= 0.01f)
        {
            if (!isWallSticking)
            {
                StartWallStick();
            }
        }
    }
    private void HandleMovement()
    {
        if (isWallSticking || isWallSliding) return;

        if (wallJumpControlWait > 0)
        {
            wallJumpControlWait -= Time.fixedDeltaTime;
            return;
        }

        KnockBack kb = GetComponent<KnockBack>();
        if (kb != null && kb.isGettingKnock) return;

        float targetVelocityX = inputVector.x * speed;
        float acceleration = isGrounded ? 100f : 50f;
        float newX = Mathf.MoveTowards(rb.linearVelocityX, targetVelocityX, acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newX, rb.linearVelocityY);
        isRunning = Mathf.Abs(inputVector.x) > minSpeed;
    }

    private void Jump()
    {
        
        if (isGrounded)
        {
            ResetWallStick();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;

        }
        else if (isTouchingWall && isWallJumpUnlocked)
        {
            ResetWallStick();

            float jumpDir = -wallDirection;
            rb.linearVelocity = new Vector2(jumpDir * wallJumpForceX, wallJumpForceY);

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
        Vector2 boxSize = new Vector2(boxCollider.size.x - 0.04f, 0.05f);

        Vector2 boxCenter = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y);

        isGrounded = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayer);

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
        Vector2 boxSize = new Vector2(boxCollider.size.x, boxCollider.size.y * 0.8f);
        RaycastHit2D rightHit = Physics2D.BoxCast(boxCollider.bounds.center, boxSize, 0f, Vector2.right, wallCheckDistatnce, walllayer);
        RaycastHit2D leftHit = Physics2D.BoxCast(boxCollider.bounds.center, boxSize, 0f, Vector2.left, wallCheckDistatnce, walllayer);

        isTouchingWall = rightHit.collider != null || leftHit.collider != null;
       

        if (rightHit.collider != null) wallDirection = 1;
        else if (leftHit.collider != null) wallDirection = -1;
    }

    private void HandleWallSliding()
    {
        if (isWallSticking || isGrounded || !isTouchingWall)
        {
            isWallSliding = false;
            return;
        }

        bool inputTowardsWall = Mathf.Abs(inputVector.x) > 0.05f && Mathf.Sign(inputVector.x) == Mathf.Sign(wallDirection);

        if (inputTowardsWall && rb.linearVelocity.y <= 0.1f)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
        else
        {
            isWallSliding = false;
        }
    }
    private void StartHealingFromInput(object sender, EventArgs e)
    {
        if (Health >= maxHealth)
        {
            return;
        }

        isHealButtonHeld = true;
        if (currentHealCharge >= 1f && !isRegenerating)
        {
            StartCoroutine(GradualHealRoutine());
        }
    }
    private void StopHealingFromInput(object sender, EventArgs e)
    {
        isHealButtonHeld = false;
    }

    private IEnumerator GradualHealRoutine()
    {
        isRegenerating = true;
        while (isHealButtonHeld && currentHealCharge >= 1f && Health < maxHealth)
        {
            float timer = 0f;
            float duration = 1f / healFillSpeed;

            while (timer < duration && isHealButtonHeld)
            {
                timer += Time.deltaTime;
                float amountToSubtract = Time.deltaTime * healFillSpeed;
                currentHealCharge -= amountToSubtract;
                currentHealCharge = Mathf.Max(0, currentHealCharge);

                OnHealProgressChanged?.Invoke(currentHealCharge / maxHealth);
                yield return null;
            }

            if (isHealButtonHeld && Health < maxHealth)
            {
                Health = Mathf.Min(Health + 1, maxHealth);
                OnHealthChanged?.Invoke(Health);
            }
        }

        isRegenerating = false;
    }
    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPlayerDash -= OnPlayerDashh;
            GameInput.Instance.OnPlayerAttack -= Player_OnPlayerAttack;
            GameInput.Instance.OnPlayerHealHoldStarted -= StartHealingFromInput;
        }
    }

   
}