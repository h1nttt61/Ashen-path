using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Player player;

    private const string IS_RUNNING = "isRunning";
    private const string IS_GROUNDED = "isGrounded";
    private const string VERTICAL_SPEED = "verticalSpeed";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GetComponentInParent<Player>();
    }

    private void Update()
    {
        if (player != null)
        {

            animator.SetBool(IS_RUNNING, player.IsRunning());
            animator.SetBool(IS_GROUNDED, player.IsGrounded());
            animator.SetFloat(VERTICAL_SPEED, player.GetVerticalSpeed());

            if (player.IsAlive())
                AdjustPlayerFacingDirection();
        }
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