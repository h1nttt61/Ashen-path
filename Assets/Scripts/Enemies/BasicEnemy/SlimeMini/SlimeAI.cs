using System.Collections;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

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

    public enum State { Idle, Chase, Dash, Cooldown, Patrol };
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
        if (Player.Instance == null || !Player.Instance.IsAlive() || isActionActive) return;

        float distance = Vector2.Distance(transform.position, Player.Instance.transform.position);

        if (distance <= 7f)
        {
            if (curState == State.Patrol) StopAllCoroutines(); 

            MoveTowardsPlayer();

            if (distance <= 3f && Time.time >= lastDashTime + dashCooldown)
            {
                StartCoroutine(DashRoutine());
            }
        }
        else
        {
            StartPatrol();
        }
    }
    private void StartPatrol()
    {
        float distance = Vector2.Distance(transform.position, Player.Instance.transform.position);
        if (distance > 7f && curState != State.Patrol)
        {
            curState = State.Patrol;
            StartCoroutine(PatrolRoutine());
        }
    }

    private IEnumerator PatrolRoutine()
    {
        while (curState == State.Patrol)
        {
            float patrolDir = Random.value > 0.5f ? 1f : -1f;
            float walkTime = Random.Range(1f, 3f);
            float elapsed = 0;

            while (elapsed < walkTime && curState == State.Patrol)
            {
                if (Vector2.Distance(transform.position, Player.Instance.transform.position) < 5f)
                {
                    curState = State.Chase;
                    yield break;
                }

                rb.linearVelocity = new Vector2(patrolDir * moveSpeed * 0.5f, rb.linearVelocity.y); 
                spriteRenderer.flipX = patrolDir < 0;

                elapsed += Time.deltaTime;
                yield return null;
            }
            
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (animator != null) animator.SetBool("isMoving", false);
            yield return new WaitForSeconds(Random.Range(1f, 2f));
            if (animator != null) animator.SetBool("isMoving", true);
        }
    }
    private void MoveTowardsPlayer()
    {
        curState = State.Chase;

        int wallsLayerMask = LayerMask.GetMask("Wall", "Ground");
        Vector2 playerPos = Player.Instance.transform.position;

        float moveDir = Mathf.Sign(playerPos.x - transform.position.x);

        RaycastHit2D floorRay = Physics2D.Raycast(
            (Vector2)transform.position,
            Vector2.down,
            1f,
            wallsLayerMask
        );
        RaycastHit2D sideRay = Physics2D.Raycast(
            transform.position,
            new Vector2(moveDir, 0),
            1f,
            wallsLayerMask
        );
        if (floorRay.distance > 0.125f)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y - (floorRay.distance - 0.125f));
        }
        else if (floorRay.distance == 0 && floorRay.collider != null)
        {
            transform.position = new Vector2(transform.position.x, floorRay.point.y + floorRay.collider.bounds.size.y / 2 + 0.125f);
        }

        float targetVelX = moveDir * moveSpeed;
        float newVelX = Mathf.MoveTowards(rb.linearVelocity.x, targetVelX, acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);

        spriteRenderer.flipX = playerPos.x < transform.position.x;

        if (animator != null) animator.SetBool("isMoving", true);
    }

    private IEnumerator DashRoutine()
    {
        LayerMask wallsLayerMask = LayerMask.GetMask("Wall", "Ground");

        isActionActive = true;
        curState = State.Dash;
        lastDashTime = Time.time; 

        float gravityBefore = rb.gravityScale;
        rb.gravityScale = 0; 
        //col.isTrigger = true; 

        Vector2 startPos = transform.position;
        float dashDir = Mathf.Sign(Player.Instance.transform.position.x - transform.position.x);
        Vector2 VectorDashDir = new Vector2(dashDir, 0);
        Vector2 castSize = new Vector2(col.bounds.size.x, col.bounds.size.y * 0.7f - col.bounds.size.y / 2);

        RaycastHit2D hit = Physics2D.BoxCast(
            startPos,
            castSize,
            0f,
            VectorDashDir,
            dashDistance,
            wallsLayerMask
        );

        float actualDistance = hit ? hit.distance - 0.05f : dashDistance;
        actualDistance = Mathf.Max(actualDistance, 0f);
        Vector2 dashTarget = startPos + VectorDashDir * actualDistance;

        float elapsed = 0;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            rb.MovePosition(Vector2.Lerp(startPos, dashTarget, elapsed / dashDuration));

            SpawnGhost();
            yield return null;
        }

        rb.gravityScale = gravityBefore;
        //col.isTrigger = false;
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