using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NavMeshAgent))]
public class BossAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;
    public enum BossState { Idle, Chasing, Attacking, Retreating, Healing, Dead };
    public BossState curState = BossState.Chasing;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private NavMeshAgent agent;
    private SpriteRenderer spriteRenderer;

    private float currentHealth;
    private bool isAttacking = false;
    private bool isHealing = false;
    private int facingDirection = 1;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Start()
    {
        if (data != null)
            currentHealth = data.enemyHealth;

        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        agent.enabled = true;
        agent.speed = data.normalSpeed;

        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Flip(float dir)
    {
        if ((dir > 0 && facingDirection < 0) || (dir < 0 && facingDirection > 0))
        {
            facingDirection *= -1;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facingDirection, transform.localScale.y, transform.localScale.z);
        }
    }
    private void Update()
    {
        if (curState == BossState.Dead || Player.Instance == null) return;

        // Лечение при низком HP
        if (currentHealth < 20 && !isHealing && curState != BossState.Dead)
        {
            StartCoroutine(HealRoutine());
        }

        float distance = Vector2.Distance(transform.position, Player.Instance.transform.position);

        switch (curState)
        {
            case BossState.Chasing:
                // Если мы достаточно близко, останавливаемся и бьем
                if (distance <= data.attackRange)
                {
                    StartCoroutine(CombatSequence());
                }
                else
                {
                    MoveToPlayer();
                }
                break;

            case BossState.Retreating:
                // В этом состоянии Update ничего не делает, ждем завершения RetreatRoutine
                break;
        }
    }

    private void MoveToPlayer()
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false; // Убеждаемся, что он может идти
            agent.SetDestination(Player.Instance.transform.position);
            Flip(agent.velocity.x);
        }
    }

    IEnumerator CombatSequence()
    {
        curState = BossState.Attacking;
        isAttacking = true;
        agent.isStopped = true;
        agent.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.4f); // Замах

        // Увеличиваем радиус проверки для большого босса (поставь 2.5f или больше)
        float attackDistance = data.attackRange + 1.5f;
        float currentDist = Vector2.Distance(transform.position, Player.Instance.transform.position);

        if (currentDist <= attackDistance)
        {
            Debug.Log("БОСС ПОПАЛ!");
            // Проверь, что метод TakeDamage в Player.cs ПУБЛИЧНЫЙ (public)
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
        }
        else
        {
            Debug.Log($"Промах! Дистанция: {currentDist}, нужно: {attackDistance}");
        }

        yield return new WaitForSeconds(0.6f);
        StartCoroutine(RetreatRoutine());
    }

    IEnumerator RetreatRoutine()
    {
        curState = BossState.Retreating;
        agent.isStopped = false;

        // Отходим не просто "назад", а в случайную точку подальше от игрока
        Vector3 directionAway = (transform.position - Player.Instance.transform.position).normalized;
        Vector3 retreatTarget = transform.position + directionAway * 5f;

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(retreatTarget);
        }

        yield return new WaitForSeconds(2.0f); // Время на "потупить" и отойти

        isAttacking = false;
        curState = BossState.Chasing;
    }

    IEnumerator HealRoutine()
    {
        isHealing = true;
        Debug.Log("Босс начал лечиться...");

        while (currentHealth < 20 && curState != BossState.Dead)
        {
            currentHealth += 2;
            Debug.Log($"Регенерация... HP: {currentHealth}");

            spriteRenderer.color = Color.green;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = Color.white;

            yield return new WaitForSeconds(2f);
        }

        isHealing = false;
    }

    public void TakeDamage(float damage)
    {
        if (curState == BossState.Dead) return;

        currentHealth -= damage;

        StopCoroutine(nameof(DamageFlash));
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0) Die();
    }

    IEnumerator DamageFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = isHealing ? Color.green : Color.white;
    }

  
    private void Die()
    {
        curState = BossState.Dead;
        agent.enabled = false;
        rb.simulated = false;
        SaveManager.SaveBossStatus(true);

        StopAllCoroutines();

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        spriteRenderer.color = Color.gray;
        Debug.Log("Босс повержен...");

        yield return new WaitForSeconds(2f);

        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime;
            spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}