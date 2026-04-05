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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;
        hasHit = true;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static;

        GetComponent<Collider2D>().enabled = false;

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.2f, 0.6f);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(damage, transform);
        }

        Destroy(gameObject, 0.5f);
    }
}