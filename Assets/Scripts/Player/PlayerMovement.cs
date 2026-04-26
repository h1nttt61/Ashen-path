using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Player core;
    private Vector2 inputVector;

    public bool IsRunning => Mathf.Abs(inputVector.x) > 0.1f;
    public bool IsJumping { get; private set; }
    private bool isFacingRight = true;
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 60f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 30f;
    [SerializeField] private float wallJumpForceX = 15f;
    [SerializeField] private float wallJumpForceY = 25f;

    [Header("Wall Settings")]
    [SerializeField] private float wallSlideSpeed = 4f;
    [SerializeField] private float wallStickTime = 0.2f;
    private float stickTimer;
    private bool canDash = true;

    [Header("Dash")]
    public bool isDashUnlocked = true;
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private float dashSpeed = 35f;
    [SerializeField] private float dashTime = 0.2f;
    private bool isDashing;

    [Header("Ghost Trail")]
    [SerializeField] private Color ghostColor = new Color(0.5f, 0.5f, 1f, 0.5f);
    [SerializeField] private float ghostDelay = 0.03f;
    [SerializeField] private float ghostFadeSpeed = 3f;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    [Header("Jump Buffer")]
    [SerializeField] private float jumpBufferTime = 0.15f;
    private float jumpBufferTimeCounter;

    [Header("Super Dash Settings")]
    [SerializeField] private float superDashSpeed = 30f;
    [SerializeField] private float superDashCooldown = 30f;
    public bool isSuperDashing = false;
    private bool canSuperDash = true;
    private float superDashTimer = 0f;

    public float GetSuperDashTimer() => superDashTimer;

    private void Start()
    {
        core = Player.Instance;
        GameInput.Instance.OnPlayerDash += OnDashInput;
    }

    private void Update()
    {
        if (GameInput.Instance == null) return;
        if (superDashTimer > 0) superDashTimer -= Time.deltaTime;
        inputVector = GameInput.Instance.GetMovementVector();

        if (inputVector.x > 0.1f) isFacingRight = true;
        else if (inputVector.x < -0.1f) isFacingRight = false;

        if (core.collision.IsGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (GameInput.Instance.WasJumpPressedThisFrame())
        {
            jumpBufferTimeCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferTimeCounter -= Time.deltaTime;
        }
        HandleJumpInput();
        if (Input.GetKeyDown(KeyCode.W))
        {
            AttemptSuperDash();
        }
    }

    private void FixedUpdate()
    {
        if (core.rb.bodyType == RigidbodyType2D.Static) return;
        if (isDashing) return;

        ApplyMovementLogic();
    }

    public void AttemptSuperDash()
    {
        float halfCharge = core.maxHealth * 0.5f;

        if (core.isSuperDashUnlocked && canSuperDash && core.combat.CurrentHealCharge >= halfCharge)
        {
            StartCoroutine(SuperDashRoutine());
        }
    }

    private void HandleJumpInput()
    {
        if (jumpBufferTimeCounter > 0f)
        {
            if (core.collision.IsTouchingWall && core.isWallJumpUnlocked && !core.collision.IsGrounded)
            {
                core.rb.linearVelocity = new Vector2(-core.collision.WallDirection * wallJumpForceX, wallJumpForceY);
                stickTimer = 0;
                IsJumping = true;
                jumpBufferTimeCounter = 0;
            }
            else if (coyoteTimeCounter > 0f)
            {
                core.rb.linearVelocity = new Vector2(core.rb.linearVelocity.x, jumpForce);
                IsJumping = true;
                jumpBufferTimeCounter = 0;
                coyoteTimeCounter = 0;
            }
        }

        if (Input.GetButtonUp("Jump") && core.rb.linearVelocity.y > 0f)
        {
            core.rb.linearVelocity = new Vector2(core.rb.linearVelocity.x, core.rb.linearVelocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }

        if (core.collision.IsGrounded) IsJumping = false;
    }

    private void ApplyMovementLogic()
    {
        float targetX = inputVector.x * speed;
        float currentAccel = core.collision.IsGrounded ? acceleration : acceleration * 0.5f;
        float newX = Mathf.MoveTowards(core.rb.linearVelocity.x, targetX, currentAccel * Time.fixedDeltaTime);

        float newY = core.rb.linearVelocity.y;

        if (core.collision.IsGrounded)
        {
            stickTimer = wallStickTime;
        }
        else if (core.collision.IsTouchingWall && newY <= 0.1f)
        {
            if (stickTimer > 0)
            {
                newY = 0;
                stickTimer -= Time.fixedDeltaTime;
            }
            else
            {
                newY = -wallSlideSpeed;
            }
        }
        core.rb.linearVelocity = new Vector2(newX, newY);
    }

    private void OnDashInput(object sender, EventArgs e)
    {
        if (!isDashing && core.isDashUnlocked && canDash && IsRunning)
        {
            StartCoroutine(DashRoutine());
        }
    }


    private IEnumerator SuperDashRoutine()
    {
        isSuperDashing = true;
        canSuperDash = false; 

        PlayerVisual visual = core.GetComponentInChildren<PlayerVisual>();
        float dashDir = (visual != null && visual.GetSpriteRenderer().flipX) ? -1f : 1f;

        float originalGravity = core.rb.gravityScale;
        core.rb.gravityScale = 0;

        while (core.combat.CurrentHealCharge > 0)
        {
            core.rb.linearVelocity = new Vector2(dashDir * superDashSpeed, 0);
            core.combat.SpendCharge(core.maxHealth * 0.2f * Time.deltaTime);

            if (core.collision.IsTouchingWall)
            {
                ApplySuperDashPenalty(dashDir, originalGravity, 30f);
                yield break;
            }

            if (!Input.GetKey(KeyCode.W)) break;
            yield return null;
        }

        isSuperDashing = false;
        core.rb.gravityScale = originalGravity;
        StartCoroutine(WaitCooldown(5f));
    }


    private void ApplySuperDashPenalty(float dashDir, float grav, float penaltyCD)
    {
        isSuperDashing = false;
        core.rb.gravityScale = grav;
        core.rb.linearVelocity = new Vector2(-dashDir * 7f, 6f);

        if (StatusEffectsUI.Instance != null)
        {
            StatusEffectsUI.Instance.ShowNausea(5f);
        }

        if (NauseaEffect.Instance != null)
        {
            NauseaEffect.Instance.StartNausea(5f);
        }

        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.5f, 0.5f);

        StartCoroutine(WaitCooldown(penaltyCD));
    }

    private IEnumerator WaitCooldown(float duration)
    {
        canSuperDash = false;
        superDashTimer = duration;

        while (superDashTimer > 0)
        {
            superDashTimer -= Time.deltaTime;
            yield return null;
        }

        superDashTimer = 0;
        canSuperDash = true;
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        GameObject bossObj = GameObject.Find("Enemy(Clone)");
        Collider2D[] bossCols = bossObj != null ? bossObj.GetComponentsInChildren<Collider2D>() : new Collider2D[0];
        Collider2D[] playerCols = GetComponentsInChildren<Collider2D>();

        if (bossCols.Length > 0)
        {
            ToggleCollisions(playerCols, bossCols, true);
        }

        float dashDir = inputVector.x != 0 ? Mathf.Sign(inputVector.x) : (isFacingRight ? 1 : -1);

        core.rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);

        StartCoroutine(CreateGhostTrail());

        yield return new WaitForSeconds(dashTime);

        yield return new WaitForSeconds(0.15f);

        if (bossCols.Length > 0)
        {
            ToggleCollisions(playerCols, bossCols, false);
        }

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    private void ToggleCollisions(Collider2D[] groupA, Collider2D[] groupB, bool ignore)
    {
        foreach (var a in groupA)
        {
            foreach (var b in groupB)
            {
                if (a != null && b != null)
                {
                    Physics2D.IgnoreCollision(a, b, ignore);
                }
            }
        }
    }

    private IEnumerator CreateGhostTrail()
    {
        PlayerVisual visual = core.GetComponentInChildren<PlayerVisual>();
        SpriteRenderer playerSR = visual != null ? visual.GetSpriteRenderer() : null;

        while (isDashing)
        {
            if (playerSR != null)
            {
                GameObject ghostObj = new GameObject("ShadowStep");
                ShadowPlayer shadow = ghostObj.AddComponent<ShadowPlayer>();

                Vector3 fixedScale = new Vector3(0.0054f, 0.0054f, 1f);

                shadow.Init(
                    playerSR.sprite,
                    transform.position,
                    transform.rotation,
                    fixedScale,
                    ghostColor,
                    ghostFadeSpeed
                );

                SpriteRenderer shadowSR = ghostObj.GetComponent<SpriteRenderer>();
                if (shadowSR != null)
                {
                    shadowSR.flipX = playerSR.flipX;
                }
            }
            yield return new WaitForSeconds(ghostDelay);
        }
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null) GameInput.Instance.OnPlayerDash -= OnDashInput;
    }
}