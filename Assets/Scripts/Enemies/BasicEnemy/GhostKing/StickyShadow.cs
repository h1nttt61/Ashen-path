using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody2D))]
public class StickyShadow : MonoBehaviour
{
    [Header("Settings")]
    public float health = 7f;
    public int damage = 1;
    public float speed = 5f;
    public float stickySpeed = 8f;
    public float stickDistance = 0.5f;
    public float damageCooldown = 0.8f;

    [Header("Components")]
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Animator anim;

    [Header("Attack Settings")]
    public float attackCooldown = 2f; 
    private bool isRecoiling = false;

    private Rigidbody2D rb;
    private Transform player;
    private float lastDamageTime;
    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        if (anim == null) anim = GetComponent<Animator>();
        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (Player.Instance != null)
            player = Player.Instance.transform;
    }

    void FixedUpdate()
    {
        if (isDead || player == null || isRecoiling) return;

        float dist = Vector2.Distance(transform.position, player.position);

        float effectiveSpeed = speed;
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;

        if (dist < 1.5f)
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 10f);
        }
        else
        {
            rb.linearVelocity = dir * speed;
        }

        sprite.flipX = rb.linearVelocity.x > 0;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0) Die();
    }

    private void ApplyDamage()
    {
        if (Time.time >= lastDamageTime + attackCooldown)
        {
            Player.Instance.TakeDamage(damage, transform);
            lastDamageTime = Time.time;
            StartCoroutine(RecoilRoutine()); 
        }
    }

    IEnumerator RecoilRoutine()
    {
        isRecoiling = true;

        Vector2 currentPos = transform.position;
        Vector2 pPos = player.position;

        Vector2 escapeDir = (currentPos - pPos).normalized + Random.insideUnitCircle * 0.5f;
        rb.linearVelocity = escapeDir.normalized * speed;

        yield return new WaitForSeconds(1.5f);
        isRecoiling = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null) anim.SetTrigger("Die");

        Destroy(gameObject, 0.1f); 
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player")) ApplyDamage();
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Player")) ApplyDamage();
    }
}