using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private const string IS_RUNNING = "isRunning";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        animator.SetBool(IS_RUNNING, Player.Instance.IsRunning());
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
