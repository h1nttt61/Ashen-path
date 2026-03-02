using UnityEngine;

public class ActiveWeapon : MonoBehaviour
{
    public static ActiveWeapon Instance { get; private set; }

    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Sword sword;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Player.Instance.IsAlive())
            FollowMousePosition();
    }

    public Sword GetActiveWeapon()
    {
        return sword;
    }

    private void FollowMousePosition()
    {
        Vector2 vector2 = GameInput.Instance.GetMovementVector();

        if (vector2.x < 0.1f)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            if (spriteRenderer != null) spriteRenderer.flipY = false;
        }
        else if (vector2.x > 0.1f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            if (spriteRenderer != null) spriteRenderer.flipY = true;
        }
    }
}
