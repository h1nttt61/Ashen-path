using UnityEngine;

public class Rock : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    private Rigidbody2D rb;
    private bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.rotation = Quaternion.Euler(0, 180, 0);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            hasHit = true;
            Player.Instance.TakeDamage(damage, transform);
            ShakeAndDestroy();
            return;
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            hasHit = true;

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;

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