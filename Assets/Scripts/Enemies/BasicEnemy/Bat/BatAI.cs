using UnityEngine;

public class BatAI : MonoBehaviour
{
    private static bool isFlockAggressed = false;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float smoothTime = 0.6f;
    [SerializeField] private float wobbleAmount = 0.3f;
    [SerializeField] private float wobbleSpeed = 1.5f;

    [Header("Combat")]
    [SerializeField] private int health = 1;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 2.0f;
    private float lastAttackTime;

    private Vector3 currentVelocity;
    private Vector3 randomOffset;
    private bool isFollowingFront;
    private Animator anim;
    private float initialScaleX;

    void Start()
    {
        anim = GetComponent<Animator>();
        initialScaleX = transform.localScale.x;
        isFollowingFront = Random.value > 0.5f;

        randomOffset = new Vector3(Random.Range(-0.7f, 0.7f), Random.Range(0.2f, 1.2f), 0);

    }

    void Update()
    {
        if (Player.Instance == null) return;

        if (isFlockAggressed)
        {
            MoveToPlayer();
        }
    }

    private void MoveToPlayer()
    {
        float side = isFollowingFront ? 0.8f : -0.8f; 
        float lookDir = Player.Instance.transform.localScale.x > 0 ? 1 : -1;

        Vector3 targetPos = Player.Instance.transform.position + new Vector3(side * lookDir, 0, 0) + randomOffset;
        targetPos.y += Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime, moveSpeed);

        FlipSprite();
    }

    private void FlipSprite()
    {
        if (transform.position.x < Player.Instance.transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(initialScaleX), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(initialScaleX), transform.localScale.y, transform.localScale.z);
    }

    public void TakeDamage(int amount)
    {
        Debug.Log("Мышь получила урон!");
        isFlockAggressed = true;
        health -= amount;

        if (health <= 0) Die();
    }

    private void Die()
    {
        if (SpiritDIalogManager.Instance != null)
            SpiritDIalogManager.Instance.RegistrKills();

        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isFlockAggressed)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Player.Instance.TakeDamage(damage, transform);
                lastAttackTime = Time.time;
            }
        }
    }
}