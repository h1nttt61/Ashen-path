using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BugAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement (Physics-based)")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float acceleration = 40f;

    [Header("Step Climbing")]
    public float maxStepHeight = 0.75f;
    public float stepSmoothing = 0.1f; 
    public float obstacleCheckDistance = 1f;
    
    [Header("Pit Detection")]
    public float pitCheckDistance = 1f;
    public float pitRayLength = 1.5f;

    [Header("Patrol Settings")]
    [SerializeField] private float maxRadius = 10f;
    public float stuckThreshold = 5f;
    public float timeUntilRecalculate = 0.5f;

    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 0.5f;
    private float lastDamageTime;
    private bool isMoving = false;

    private Rigidbody2D rb;
    private Collider2D col;
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

        StartCoroutine(PatrolArea());
    }

    private void Move(Vector2 moveTo)
    {
        int wallsLayerMask = LayerMask.GetMask("Wall", "Ground", "Overlay");
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

        if (floorRay.collider != null && floorRay.distance > .5f)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y - (floorRay.distance - .5f));
        }
        else
        {
            Debug.Log($"ray - {floorRay.distance}");
        }

        Vector2 pitRayOrigin = (Vector2)transform.position + new Vector2(moveDir * pitCheckDistance, 0);
        RaycastHit2D pitHit = Physics2D.Raycast(pitRayOrigin, Vector2.down, pitRayLength, wallsLayerMask);

        if (pitHit.collider == null && floorRay.collider != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        spriteRenderer.flipX = moveTo.x >= transform.position.x;
    }

    private IEnumerator PatrolArea()
    {
        while (true)
        {
            float patrolDir = Random.value > 0.5f ? 1f : -1f;
            float randDist = Random.Range(5f, 8f);
            Vector2 patrolPos = (Vector2)transform.position + new Vector2(patrolDir * randDist, 0);

            stuckTimer = 0f;
            lastPosition = transform.position;

            while (Vector2.Distance(new Vector2(transform.position.x, 0), new Vector2(patrolPos.x, 0)) > 0.1f)
            {
                Move(patrolPos);

                if (Vector2.Distance(transform.position, lastPosition) < stuckThreshold)
                {
                    stuckTimer += Time.deltaTime;
                }
                else
                {
                    stuckTimer = 0f;
                }
                
                lastPosition = transform.position;

                if (stuckTimer >= timeUntilRecalculate)
                {
                    Debug.Log($"stopped, {stuckTimer}");
                    break;
                }

                yield return new WaitForFixedUpdate();
            }

            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        }
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
