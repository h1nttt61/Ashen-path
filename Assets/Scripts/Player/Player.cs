using UnityEngine;
using System;

[SelectionBase]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    private Rigidbody2D rb;
    [SerializeField] private float speed = 5f;
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float damageRecoveryTime = 0.5f;

    Vector2 inputVector;
    private Camera camera;
    private readonly float minSpeed = 0.1f;
    private bool isRunning = false;


    [SerializeField] private float jumpForce = 2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private bool isAlive;

    private bool isGrounded;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        camera = Camera.main;
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
    }

    public bool IsAlive() => isAlive;

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