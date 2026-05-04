using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class MosquitoAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement (Physics-based)")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float acceleration = 40f;
    
    [Header("Damage Settings")]
    [SerializeField] private GameObject mosquitoPrefab;
    [SerializeField] private float damageCooldown = 0.5f;
    [SerializeField] private float dashCooldown = 2.5f;
    [SerializeField] private float dashDistance = 4.5f;
    [SerializeField] private float dashDuration = 0.3f;

    private float lastDamageTime;
    private Rigidbody2D rb;
    private Collider2D col;
    private float currentHealth;
    private float dashTimer;
    private float lastDashTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        rb.freezeRotation = true;
        dashTimer = Time.time;
    }

    private void Start()
    {
        if (data != null) currentHealth = data.enemyHealth;

    }

    void Update()
    {
        Transform playerTransform = Player.Instance.transform;
        float distToPlayer = Vector2.Distance(playerTransform.position, transform.position);

        Move(Player.Instance.transform.position);

        if (Time.time >= dashTimer && distToPlayer <= data.attackRange)
        {
            StartCoroutine(Dash());
            dashTimer = Time.time + dashCooldown;
        }
    }

    private void Move(Vector2 moveTo)
    {
        Vector2 offset = moveTo - (Vector2)transform.position;
        float distance = offset.magnitude;

        if (distance < 3f) 
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, acceleration * Time.fixedDeltaTime);
            return;
        }

        Vector2 moveDir = offset.normalized;

        float targetVelX = moveDir.x * moveSpeed;
        float targetVelY = moveDir.y * moveSpeed;

        float newVelX = Mathf.MoveTowards(rb.linearVelocity.x, targetVelX, acceleration * Time.fixedDeltaTime);
        float newVelY = Mathf.MoveTowards(rb.linearVelocity.y, targetVelY, acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newVelX, newVelY);

        spriteRenderer.flipX = moveTo.x < transform.position.x;
    }

    private IEnumerator Dash()
    {
        LayerMask wallsLayerMask = LayerMask.GetMask("Wall", "Ground");
        lastDashTime = Time.time; 

        float gravityBefore = rb.gravityScale;
        rb.gravityScale = 0; 

        Vector2 startPos = transform.position;
        Vector2 dashDir = (Player.Instance.transform.position - transform.position).normalized;
        Vector2 castSize = new Vector2(col.bounds.size.x, col.bounds.size.y * 0.7f - col.bounds.size.y / 2);

        RaycastHit2D hit = Physics2D.BoxCast(
            startPos,
            castSize,
            0f,
            dashDir,
            dashDistance,
            wallsLayerMask
        );

        float actualDistance = hit ? hit.distance - 0.25f : dashDistance;
        actualDistance = Mathf.Max(actualDistance, 0f);
        Vector2 dashTarget = startPos + dashDir * actualDistance;

        float elapsed = 0;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;

            rb.MovePosition(Vector2.Lerp(startPos, dashTarget, elapsed / dashDuration));

            yield return null;
        }

        rb.gravityScale = gravityBefore;
        rb.linearVelocity = Vector2.zero;

        spriteRenderer.flipX = dashTarget.x < transform.position.x;

        yield return new WaitForSeconds(0.5f); 
    }

    private void ApplyDamageToPlayer()
    {
        if (Time.time >= lastDamageTime + damageCooldown)
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
            lastDamageTime = Time.time;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
            if (collision.gameObject.TryGetComponent(out KnockBack kb))
                kb.GetKnockedBack(transform);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) ApplyDamageToPlayer();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (SpiritDIalogManager.Instance != null) SpiritDIalogManager.Instance.RegistrKills();
        Destroy(gameObject);
    }
}
