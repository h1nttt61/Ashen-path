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
    [SerializeField] private float dashTime = 0.15f;
    private bool isDashing;

    [Header("Ghost Trail")]
    [SerializeField] private Color ghostColor = new Color(0.5f, 0.5f, 1f, 0.5f);
    [SerializeField] private float ghostDelay = 0.03f;
    [SerializeField] private float ghostFadeSpeed = 3f;

    private void Start()
    {
        core = Player.Instance;
        GameInput.Instance.OnPlayerDash += OnDashInput;
    }

    private void Update()
    {
        if (GameInput.Instance == null) return;

        inputVector = GameInput.Instance.GetMovementVector();

        if (inputVector.x > 0.1f) isFacingRight = true;
        else if (inputVector.x < -0.1f) isFacingRight = false;

        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        if (core.rb.bodyType == RigidbodyType2D.Static) return;
        if (isDashing) return;

        ApplyMovementLogic();
    }

    private void HandleJumpInput()
    {
        if (GameInput.Instance.WasJumpPressedThisFrame())
        {
            if (core.collision.IsTouchingWall && core.isWallJumpUnlocked && !core.collision.IsGrounded)
            {
                core.rb.linearVelocity = new Vector2(-core.collision.WallDirection * wallJumpForceX, wallJumpForceY);
                stickTimer = 0;
                IsJumping = true;
            }
            else if (core.collision.IsGrounded)
            {
                core.rb.linearVelocity = new Vector2(core.rb.linearVelocity.x, jumpForce);
                IsJumping = true;
            }
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

    private IEnumerator DashRoutine()
    {
        canDash = false; 
        isDashing = true;
        core.InvokeDashEvent();

        float dashDir = isFacingRight ? 1 : -1;
        Coroutine ghostCoroutine = StartCoroutine(CreateGhostTrail());

        core.rb.linearVelocity = new Vector2(dashDir * dashSpeed, core.rb.linearVelocity.y);

        yield return new WaitForSeconds(dashTime);

        StopCoroutine(ghostCoroutine);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
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