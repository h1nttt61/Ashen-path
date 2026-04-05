using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SlimeAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Dash and Effects")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashSpeedMultiplier = 3f;
    [SerializeField] private float ghostDelay = 0.05f;

    public enum State { Idle, Chase, Dash, Cooldown };
    public State curState = State.Idle;

    private NavMeshAgent agent;
    private bool isActionActive = false; 
    private float currentHealth;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        if (data != null)
        {
            agent.speed = data.normalSpeed;
            currentHealth = data.enemyHealth;
        }
    }

    private void Update()
    {
        if (Player.Instance == null || !Player.Instance.IsAlive()) return;

        float distance = Vector3.Distance(transform.position, Player.Instance.transform.position);

        if (animator != null)
        {
            bool isMoving = agent.enabled && agent.velocity.magnitude > 0.1f;
            animator.SetBool("isMoving", isMoving);
        }

        if (curState != State.Dash)
        {
            float directionX = Player.Instance.transform.position.x - transform.position.x;

            if (Mathf.Abs(directionX) > 0.1f)
            {
                spriteRenderer.flipX = (directionX < 0);
            }
        }

        switch (curState)
        {
            case State.Idle:
                if (distance < data.detectionRange) curState = State.Chase;
                break;
            case State.Chase:
                HandleChase(distance);
                break;
        }
    }

    private void HandleChase(float distance)
    {
        if (isActionActive) return;

        if (distance <= data.detectionRange && distance > 2f)
        {
            agent.SetDestination(Player.Instance.transform.position);
        }
        else if (distance <= 2f)
        {
            StartCoroutine(PerformDash());
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
    private IEnumerator PerformDash()
    {
        isActionActive = true;
        curState = State.Dash;

        if (animator != null) animator.SetTrigger("dashTrigger");

        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        if (col != null) col.isTrigger = true;

        Vector3 playerPos = Player.Instance.transform.position;
        Vector3 flatPlayerPos = new Vector3(playerPos.x, transform.position.y, transform.position.z);

        float maxDashDist = Vector3.Distance(transform.position, flatPlayerPos) + dashDistance;
        Vector3 dir = (flatPlayerPos - transform.position).normalized;

        LayerMask layerMask = LayerMask.GetMask("Wall");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, maxDashDist, layerMask);

        Vector3 dashTarget;

        if (hit.collider != null)
        {
            dashTarget = hit.point - (Vector2)dir * 0.2f;
            Debug.Log("dash target range decreased");
        }
        else
        {
            dashTarget = transform.position + dir * maxDashDist;
        }

        agent.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        float gravityBefore = 0;
        if (rb != null)
        {
            gravityBefore = rb.gravityScale;
            rb.gravityScale = 0; 
            rb.linearVelocity = Vector2.zero; 
        }

        float elapsed = 0;
        float duration = 0.4f;
        float ghostTimer = 0;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ghostTimer += Time.deltaTime;

            transform.position = Vector3.Lerp(startPos, dashTarget, elapsed / duration);

            if (ghostTimer >= ghostDelay)
            {
                SpawnGhost();
                ghostTimer = 0;
            }
            yield return null;
        }

        if (rb != null) rb.gravityScale = gravityBefore;
        if (col != null) col.isTrigger = false;

        agent.enabled = true;
        curState = State.Cooldown;
        yield return new WaitForSeconds(1f);

        isActionActive = false;
        curState = State.Chase;
    }

    private void SpawnGhost()
    {
        GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);
        var gScript = ghost.GetComponent<TrailGhost>();
        gScript.Init(spriteRenderer.sprite);
        ghost.GetComponent<SpriteRenderer>().flipX = spriteRenderer.flipX;
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (SpiritDIalogManager.Instance != null)
        {
            SpiritDIalogManager.Instance.RegistrKills();
        }

        Destroy(gameObject);
    }
}