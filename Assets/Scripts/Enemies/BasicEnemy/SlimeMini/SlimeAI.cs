using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SlimeAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement (Physics-based)")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float acceleration = 40f;

    [Header("Dash Settings")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private float dashDistance = 4f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 3f; 
    private float lastDashTime = -999f;

    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 0.5f;
    private float lastDamageTime;

    public enum State { Idle, Chase, Dash, Cooldown };
    public State curState = State.Idle;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isActionActive = false;
    private float currentHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        rb.freezeRotation = true;
    }

    private void Start()
    {
        if (data != null) currentHealth = data.enemyHealth;
    }

    private void FixedUpdate()
    {
        if (Player.Instance == null || !Player.Instance.IsAlive() || isActionActive)
        {
            if (!isActionActive)
                rb.linearVelocity = new Vector2(Mathf.MoveTowards(rb.linearVelocity.x, 0, acceleration * Time.fixedDeltaTime), rb.linearVelocity.y);
            return;
        }

        float distance = Vector2.Distance(transform.position, Player.Instance.transform.position);

        if (curState == State.Idle || curState == State.Chase)
        {
            MoveTowardsPlayer();

            if (distance <= 3f && Time.time >= lastDashTime + dashCooldown)
            {
                StartCoroutine(DashRoutine());
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        curState = State.Chase;

        float moveDir = Mathf.Sign(Player.Instance.transform.position.x - transform.position.x);

        float targetVelX = moveDir * moveSpeed;
        float newVelX = Mathf.MoveTowards(rb.linearVelocity.x, targetVelX, acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);

        spriteRenderer.flipX = Player.Instance.transform.position.x < transform.position.x;

        if (animator != null) animator.SetBool("isChasing", true);
    }

    private IEnumerator DashRoutine()
    {
        isActionActive = true;
        curState = State.Dash;
        lastDashTime = Time.time; 

        float gravityBefore = rb.gravityScale;
        rb.gravityScale = 0; 
        col.isTrigger = true; 

        Vector2 startPos = transform.position;
        float dashDir = Mathf.Sign(Player.Instance.transform.position.x - transform.position.x);
        Vector2 dashTarget = new Vector2(startPos.x + (dashDir * dashDistance), startPos.y);

        float elapsed = 0;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            rb.MovePosition(Vector2.Lerp(startPos, dashTarget, elapsed / dashDuration));

            SpawnGhost();
            yield return null;
        }

        rb.gravityScale = gravityBefore;
        col.isTrigger = false;
        rb.linearVelocity = Vector2.zero;

        curState = State.Cooldown;
        yield return new WaitForSeconds(0.5f); 

        isActionActive = false;
        curState = State.Chase;
    }

    private void SpawnGhost()
    {
        if (ghostPrefab == null) return;
        GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);
        var gScript = ghost.GetComponent<TrailGhost>();
        if (gScript != null) gScript.Init(spriteRenderer.sprite);
        ghost.GetComponent<SpriteRenderer>().flipX = spriteRenderer.flipX;
    }

    private void ApplyDamageToPlayer()
    {
        if (Time.time >= lastDamageTime + damageCooldown)
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount / 3, transform);
            lastDamageTime = Time.time;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
            if (collision.gameObject.TryGetComponent(out KnockBack kb))
                kb.GetKnockedBack(transform);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) ApplyDamageToPlayer();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) ApplyDamageToPlayer();
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