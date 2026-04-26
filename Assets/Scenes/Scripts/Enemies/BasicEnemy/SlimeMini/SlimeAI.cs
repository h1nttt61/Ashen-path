using System.Collections;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody2D))]
public class SlimeAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement (Physics-based)")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float acceleration = 40f;

    [Header("Step Climbing")]
    public float maxStepHeight = 0.5f;
    public float stepSmoothing = 0.1f; 
    public float obstacleCheckDistance = 0.2f;
    
    [Header("Pit Detection")]
    public float pitCheckDistance = 0.5f;
    public float pitRayLength = 1.5f;

    [Header("Patrol Settings")]
    public float stuckThreshold = 0.05f;
    public float timeUntilRecalculate = 0.5f;

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
    private float stuckTimer = 0f;
    private Vector2 lastPosition;
    

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

    private void Move(Vector2 moveTo)
    {
        int wallsLayerMask = LayerMask.GetMask("Wall", "Ground");
        float moveDir = Mathf.Sign(moveTo.x - transform.position.x);

        float targetVelX = moveDir * moveSpeed;
        float newVelX = Mathf.MoveTowards(rb.linearVelocity.x, targetVelX, acceleration * Time.fixedDeltaTime);

        Vector2 footPos = (Vector2)transform.position;
        Vector2 lowerRayOrigin = footPos + new Vector2(0, 0.1f);
        Vector2 upperRayOrigin = footPos + new Vector2(0, maxStepHeight);

        RaycastHit2D lowerHit = Physics2D.Raycast(lowerRayOrigin, new Vector2(moveDir, 0), obstacleCheckDistance, wallsLayerMask);
        RaycastHit2D upperHit = Physics2D.Raycast(upperRayOrigin, new Vector2(moveDir, 0), obstacleCheckDistance, wallsLayerMask);

        if (lowerHit.collider != null && upperHit.collider == null)
        {
            transform.position += new Vector3(0, stepSmoothing, 0);
            rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);
        }
        else if (lowerHit.collider != null && upperHit.collider != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);
        }

        RaycastHit2D floorRay = Physics2D.Raycast(transform.position, Vector2.down, 100f, wallsLayerMask);

        if (floorRay.collider != null && floorRay.distance > 0.125f)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y - (floorRay.distance - 0.125f));
        }

        Vector2 pitRayOrigin = (Vector2)transform.position + new Vector2(moveDir * pitCheckDistance, 0);
        RaycastHit2D pitHit = Physics2D.Raycast(pitRayOrigin, Vector2.down, pitRayLength, wallsLayerMask);

        if (pitHit.collider == null && floorRay.collider != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (animator != null) animator.SetBool("isMoving", false);
            return;
        }

        spriteRenderer.flipX = moveTo.x < transform.position.x;
        if (animator != null) animator.SetBool("isMoving", true);
    }

    private IEnumerator PatrolRoutine()
    {

         while (curState == State.Patrol)
        {
            float patrolDir = Random.value > 0.5f ? 1f : -1f;
            Vector2 patrolPos = (Vector2)transform.position + new Vector2(patrolDir * Random.Range(2f, 5f), 0);

            stuckTimer = 0f;
            lastPosition = transform.position;

            while (Vector2.Distance(new Vector2(transform.position.x, 0), new Vector2(patrolPos.x, 0)) > 0.1f && curState == State.Patrol)
            {
                Move(patrolPos);

                if (Vector2.Distance(transform.position, lastPosition) < stuckThreshold)
                    stuckTimer += Time.deltaTime;
                else
                    stuckTimer = 0f;

                if (stuckTimer >= timeUntilRecalculate)
                {
                    
                    float escapeDir = -Mathf.Sign(patrolPos.x - transform.position.x);
                    float escapeTime = 0.2f;
                    
                    while (escapeTime > 0)
                    {
                        rb.linearVelocity = new Vector2(escapeDir * moveSpeed, rb.linearVelocity.y);
                        escapeTime -= Time.deltaTime;
                        yield return null;
                    }
                    
                    break; 
                }

                lastPosition = transform.position;
                yield return null;
        }

            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (animator != null) animator.SetBool("isMoving", false);

            yield return new WaitForSeconds(Random.Range(1f, 2f));
        }
    }
    private void MoveTowardsPlayer()
    {
        curState = State.Chase;
        Vector2 playerPos = Player.Instance.transform.position;
        Move(playerPos);
    }

    private IEnumerator DashRoutine()
    {
        LayerMask wallsLayerMask = LayerMask.GetMask("Wall", "Ground");

        isActionActive = true;
        curState = State.Dash;
        lastDashTime = Time.time; 

        float gravityBefore = rb.gravityScale;
        rb.gravityScale = 0; 

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
        Vector2 estimatedTarget = startPos + VectorDashDir * actualDistance;

        RaycastHit2D floorHit = Physics2D.Raycast(estimatedTarget, Vector2.down, 5f, wallsLayerMask);
        if (floorHit.collider == null)
        {
            for (float i = actualDistance; i > 0; i -= 0.1f)
            {
                Vector2 checkPos = startPos + VectorDashDir * i;
                RaycastHit2D edgeCheck = Physics2D.Raycast(checkPos, Vector2.down, 5f, wallsLayerMask);

                if (edgeCheck.collider != null)
                {
                    actualDistance = i - 0.2f; 
                    break;
                }
            }
        }
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
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
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