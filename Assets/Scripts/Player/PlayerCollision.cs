using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Main Body")]
    [SerializeField] private BoxCollider2D bodyCollider;

    [Header("Detection Colliders")]
    [SerializeField] private CapsuleCollider2D  groundCheck;
    [SerializeField] private BoxCollider2D rightWallCheck;
    [SerializeField] private BoxCollider2D leftWallCheck;

    public bool IsGrounded { get; private set; }
    public bool IsTouchingWall { get; private set; }
    public int WallDirection { get; private set; }

    private void FixedUpdate()
    {
        IsGrounded = groundCheck.IsTouchingLayers(groundLayer);

        bool isRightWall = rightWallCheck.IsTouchingLayers(wallLayer);
        bool isLeftWall = leftWallCheck.IsTouchingLayers(wallLayer);

        IsTouchingWall = isRightWall || isLeftWall;

        if (isRightWall) WallDirection = 1;
        else if (isLeftWall) WallDirection = -1;
        else WallDirection = 0;
    }
}