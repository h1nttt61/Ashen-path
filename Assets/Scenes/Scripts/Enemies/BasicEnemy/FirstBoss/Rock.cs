using UnityEngine;

public class Rock : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    private Rigidbody2D rb;
    private bool hasHit = false;
    private Collider2D myCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        transform.rotation = Quaternion.Euler(0, 180, 0);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        GameObject boss = GameObject.Find("Enemy(Clone)");
        if (boss != null)
        {
            Collider2D[] bossCols = boss.GetComponentsInChildren<Collider2D>();
            foreach (var bc in bossCols)
            {
                Physics2D.IgnoreCollision(myCollider, bc, true);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(damage, transform);

            Collider2D[] playerCols = collision.gameObject.GetComponentsInChildren<Collider2D>();
            foreach (var pc in playerCols)
            {
                Physics2D.IgnoreCollision(myCollider, pc, true);
            }
            return;
        }

        if (collision.gameObject.CompareTag("Ground") && !hasHit)
        {
            hasHit = true;
            rb.bodyType = RigidbodyType2D.Static;
            ShakeAndDestroy();
        }
    }

    private void ShakeAndDestroy()
    {
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.2f, 0.6f);
        Destroy(gameObject, 0.5f);
    }
}