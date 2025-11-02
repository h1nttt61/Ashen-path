using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Wall Jump")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpForce = 10f;
    [SerializeField] private float wallJumpDirectionForce = 5f;
    [SerializeField] private float wallJumpTime = 0.2f;
    [SerializeField] private LayerMask whatIsWall;

    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask whatIsGround;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float moveInput;
    private bool IsGrounded;
    private bool isFacingRight = true;

    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpDirection;
    private float WallJumpCounter;
    private bool isTouchingWall;



    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        moveInput = Input.GetAxisRaw("Horizontal");


    }
}
