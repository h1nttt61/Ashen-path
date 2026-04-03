using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NavMeshAgent))]
public class BossAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;
    public enum BossState { Idle, Chasing, Attacking, Retreating, Healing, Dead, Enranged };
    public BossState curState = BossState.Chasing;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Phase Settings")]
    [SerializeField] private GameObject rockPrefab; 
    private float currentHealth;
    private bool isHealing = false;
    private bool isIntroDone = false;
    private bool hasOverhealed = false; 
    private bool canDamagePlayer = true;

    private int facingDirection = 1;
    private float attackRange = 4.5f;
    private GameObject bossDoor;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        bossDoor = GameObject.Find("BoosDoor");

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Start()
    {
        if (data != null) currentHealth = data.enemyHealth;

        agent.enabled = true;
        agent.speed = data.normalSpeed;
        agent.stoppingDistance = 4.0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
    }

    private void Update()
    {
        if (!isIntroDone || curState == BossState.Dead || Player.Instance == null) return;

        float hpPercent = currentHealth / data.enemyHealth;

        if (hpPercent <= 0.3f && curState != BossState.Enranged)
        {
            StartEnragedPhase();
        }

        if (curState == BossState.Enranged && hpPercent <= 0.2f && !hasOverhealed)
        {
            StartCoroutine(OverHealRoutine());
        }

        if (hpPercent < 0.45f && hpPercent > 0.31f && !isHealing && curState != BossState.Enranged)
        {
            StartCoroutine(HealRoutine());
        }

        if (animator != null)
        {
            bool isMoving = agent.enabled && agent.velocity.magnitude > 0.1f && !agent.isStopped;
            animator.SetBool("isChasing", isMoving);
        }

        if (curState == BossState.Chasing || curState == BossState.Enranged)
        {
            MoveToPlayer();
        }
    }

    private void StartEnragedPhase()
    {
        curState = BossState.Enranged;
        isHealing = false; 
        StopCoroutine(nameof(HealRoutine));

        agent.speed = data.normalSpeed * 1.5f;
        spriteRenderer.color = new Color(1f, 0.5f, 0.5f); 

        StartCoroutine(RockRainRoutine());
    }

    private IEnumerator RockRainRoutine()
    {
        while (curState == BossState.Enranged)
        {
            yield return new WaitForSeconds(Random.Range(6f, 9f));

            if (rockPrefab != null)
            {
                Vector3 spawnPos = Player.Instance.transform.position + new Vector3(Random.Range(-2f, 2f), 12f, 0f);
                Instantiate(rockPrefab, spawnPos, Quaternion.identity);
            }
        }
    }

    private IEnumerator OverHealRoutine()
    {
        hasOverhealed = true; 
        isHealing = true;
        agent.isStopped = true; 

        float targetHP = data.enemyHealth * 0.45f;

        while (currentHealth < targetHP && curState != BossState.Dead)
        {
            currentHealth += 4;
            spriteRenderer.color = Color.cyan;
            yield return new WaitForSeconds(0.05f);
        }

        spriteRenderer.color = new Color(1f, 0.5f, 0.5f);
        isHealing = false;
        agent.isStopped = false;
    }

    private IEnumerator HealRoutine()
    {
        isHealing = true;
        while (currentHealth < data.enemyHealth && curState != BossState.Enranged && curState != BossState.Dead)
        {
            currentHealth += 1;
            spriteRenderer.color = Color.green;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.5f);
        }
        isHealing = false;
    }

    private void MoveToPlayer()
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            Vector2 playerPos = Player.Instance.transform.position;
            float distanceToPlayer = Vector3.Distance(transform.position, playerPos);

            if (distanceToPlayer > attackRange)
            {
                agent.isStopped = false;
                agent.SetDestination(playerPos);
            }
            else
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            Flip(agent.velocity.x);
        }
    }

    private void Flip(float dir)
    {
        if ((dir > 0 && facingDirection < 0) || (dir < 0 && facingDirection > 0))
        {
            facingDirection *= -1;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facingDirection, transform.localScale.y, transform.localScale.z);
        }
    }

    public void TakeDamage(float damage)
    {
        if (curState == BossState.Dead || !isIntroDone) return;

        currentHealth -= damage;
        StopCoroutine(nameof(DamageFlash));
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0) Die();
    }

    IEnumerator DamageFlash()
    {
        Color currentBaseColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = isHealing ? Color.green : currentBaseColor;
    }

    public void ActivateBoss()
    {
        isIntroDone = true;
        canDamagePlayer = true;
        curState = BossState.Chasing;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (curState == BossState.Dead || !canDamagePlayer) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
            StartCoroutine(TouchDamageCooldown());
        }
    }

    private IEnumerator TouchDamageCooldown()
    {
        canDamagePlayer = false;
        yield return new WaitForSeconds(1.0f);
        canDamagePlayer = true;
    }

    private void Die()
    {
        curState = BossState.Dead;
        agent.isStopped = true;
        agent.enabled = false;
        rb.simulated = false;

        if (Player.Instance != null)
        {
            Player.Instance.UnlockSuperDash();
        }
        if (animator != null)
        {
            animator.SetBool("isChasing", false);
            animator.enabled = false;
        }
        SaveManager.SaveBossStatus(true);
        StopAllCoroutines();
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        spriteRenderer.color = Color.gray;
        float elapsed = 0;
        float duration = 2f;
        Color startColor = spriteRenderer.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}