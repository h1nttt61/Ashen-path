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
        if (EscMenu.Instance == null || EscMenu.Instance.isPause) return;

        Vector2 movement = GameInput.Instance.GetMovementVector();

        if (Mathf.Abs(movement.x) > 0.1f)
        {
            bool isLeft = movement.x < 0;
            spriteRenderer.flipX = isLeft;
            Transform handContainer = Player.Instance.transform.Find("HandCombatContainer");
            if (handContainer != null)
            {
                Vector3 scale = handContainer.localScale;
                scale.x = isLeft ? -1 : 1;
                handContainer.localScale = scale;
            }
        }
    }

    public void EnableLeftHand() => Player.Instance.leftHand.EnableAttack();
    public void DisableLeftHand() => Player.Instance.leftHand.DisableAttack();

    public void EnableRightHand() => Player.Instance.rightHand.EnableAttack();
    public void DisableRightHand() => Player.Instance.rightHand.DisableAttack();
}
