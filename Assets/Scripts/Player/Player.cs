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
    [SerializeField] private int maxHealth = 10;
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
    private bool isDashing;

    private bool isAlive;

    private bool isGrounded;

    private float initialSpeed;


    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.freezeRotation = true;
        camera = Camera.main;
        initialSpeed = speed;
    }

    private void Update()
    {
        CheckGrounded();
        if (GameInput.Instance != null)
        {
            inputVector = new Vector2(GameInput.Instance.GetMovementVector().x, 0f);

            if (GameInput.Instance.WasJumpPressedThisFrame() && isGrounded)
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
    }

    public void Start()
    {
        GameInput.Instance.OnPlayerDash += OnPlayerDashh;
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

    private void ApplyGravity()
    {
        if (rb.linearVelocity.y < 0)
            rb.gravityScale = fallMultiplier;
        else if (rb.linearVelocity.y > 0 && !GameInput.Instance.IsJumpPressed())
            rb.gravityScale = lowJumpMultiplier;
        else
            rb.gravityScale = 1f;
    }

    private void HandleMovement()
    {
        rb.linearVelocity = new Vector2(inputVector.x * speed, rb.linearVelocity.y);

        if (Math.Abs(inputVector.x) > minSpeed)
            isRunning = true;
        else
            isRunning = false;
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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
}