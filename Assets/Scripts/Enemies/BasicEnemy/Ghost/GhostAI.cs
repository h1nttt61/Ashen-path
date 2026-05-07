using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GhostAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement (Physics-based)")]
    private float moveSpeed = 3f;
    private float acceleration = 40f;
    
    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 0.5f;
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float attackDistance = 4.5f;

    private float lastDamageTime;
    private Rigidbody2D rb;
    private bool isAttacking;
    private float currentHealth;
    private float attackTimer;
    private Vector2 startPos; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        rb.freezeRotation = true;
    }

    private void Start()
    {
        if (data != null) currentHealth = data.enemyHealth;
        attackTimer = Time.time;
        startPos = transform.position;
    }

    void Update()
    {
        if (isAttacking) return;

        Transform playerTransform = Player.Instance.transform;
        float distToPlayer = Vector2.Distance(playerTransform.position, transform.position);
        
        if (distToPlayer > 3f)
        {
            moveSpeed = 3f;
            acceleration = 40f;

            if (distToPlayer <= data.detectionRange)
            {
                Move(playerTransform.position);
            }
            else
            {
                Move(startPos);
            }
        }
        else
        {
            if (Time.time >= attackTimer)
            {
                StartCoroutine(Attack());
                attackTimer = Time.time + attackCooldown;
            }
        }

    }

    private void Move(Vector2 moveTo)
    {
        Vector2 offset = moveTo - (Vector2)transform.position;
        float distance = offset.magnitude;

        if (distance < 0.25f) 
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

        spriteRenderer.flipX = moveTo.x >= transform.position.x;
    }

    private IEnumerator Attack()
    {
        //GetComponent<Animator>().SetTrigger("isAttacking");
        Transform playerTransform = Player.Instance.transform;

        isAttacking = true;
        
        Vector2 attackDir = -(transform.position - playerTransform.position).normalized;
        Vector2 targetPos = (Vector2)transform.position + (attackDir * 4f);

        moveSpeed = 10f;
        acceleration = 80f;

        float timer = 0;
        while(timer < 0.5f) 
        {
            Move(targetPos); 
            timer += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.flipX = targetPos.x >= transform.position.x;

        isAttacking = false;
    }

    private void ApplyDamageToPlayer()
    {
        if (Time.time >= lastDamageTime + damageCooldown)
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
            lastDamageTime = Time.time;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
            if (collider.gameObject.TryGetComponent(out KnockBack kb))
                kb.GetKnockedBack(transform);
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player")) ApplyDamageToPlayer();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
