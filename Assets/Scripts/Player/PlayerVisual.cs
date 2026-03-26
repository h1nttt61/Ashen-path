using Unity.VisualScripting;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D playerRb;

    private const string IS_RUNNING = "isRunning";
    private const string IS_JUMP = "isJump";
    private const string Y_VELOCITY = "yVelocity";
    private const string IS_GROUND = "isGd";
    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (Player.Instance != null)
        {
            playerRb = Player.Instance.GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        if (Player.Instance == null || playerRb == null) return;
        if (animator == null) return;

        animator.SetBool(IS_RUNNING, Player.Instance.IsRunning());
        animator.SetBool(IS_JUMP, Player.Instance.isJump());
        animator.SetFloat(Y_VELOCITY, playerRb.linearVelocity.y);
        animator.SetBool(IS_GROUND, Player.Instance.isGrouned());
        if (Player.Instance.IsAlive())
            AdjustPlayerFacingDirection();
        
    }

    private void AdjustPlayerFacingDirection()
    {
        Vector2 movement = GameInput.Instance.GetMovementVector();

        if (Mathf.Abs(movement.x) > 0.1f)
        {
            spriteRenderer.flipX = movement.x < 0;
        }
    }
}
