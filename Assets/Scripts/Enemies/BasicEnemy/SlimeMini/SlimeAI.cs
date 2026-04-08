using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SlimeAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Effects")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashSpeedMultiplier = 3f;
    [SerializeField] private float ghostDelay = 0.05f;
    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 0.5f; 
    private float lastDamageTime;
    public enum State { Idle, Chase, Dash, Cooldown };
    public State curState = State.Idle;

    private NavMeshAgent agent;
    private bool isActionActive = false; 
    private float currentHealth;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D col;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody2D>(); 
        col = GetComponent<Collider2D>(); 

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }
    private void Start()
    {
        if (data != null)
        {
            agent.speed = data.normalSpeed;
            currentHealth = data.enemyHealth;
        }
    }

    private void Update()
    {
        if (Player.Instance == null || data == null || agent == null) return;
        if (!Player.Instance.IsAlive()) return;
        if (isActionActive) return;

        float distance = Vector2.Distance(transform.position, Player.Instance.transform.position);

        if (curState == State.Idle || curState == State.Chase)
        {
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(Player.Instance.transform.position);
                curState = State.Chase;
            }

            if (distance <= 3f)
            {
                StartCoroutine(DashRoutine());
            }
        }

        if (spriteRenderer != null)
            spriteRenderer.flipX = Player.Instance.transform.position.x < transform.position.x;
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
    private IEnumerator DashRoutine()
    {
        isActionActive = true;
        curState = State.Dash;
        agent.enabled = false;

        float gravityBefore = rb.gravityScale;
        rb.gravityScale = 0; 
        col.isTrigger = true;

        Vector3 playerPos = Player.Instance.transform.position;
        Vector3 dashTarget = new Vector3(playerPos.x, transform.position.y, transform.position.z);

        float elapsed = 0;
        float duration = 0.4f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, dashTarget, elapsed / duration);
            SpawnGhost();
            yield return null;
        }

        rb.gravityScale = gravityBefore;
        col.isTrigger = false;

        yield return new WaitForFixedUpdate();

        agent.enabled = true;
        curState = State.Cooldown;
        yield return new WaitForSeconds(1f);

        isActionActive = false;
        curState = State.Chase;
    }

    private void ApplyDamageToPlayer()
    {
        if (Time.time >= lastDamageTime + damageCooldown)
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
            lastDamageTime = Time.time;
            Debug.Log("Слайм нанес урон!");
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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
            Debug.Log("Слайм протаранил игрока в деше!");
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