using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NavMeshAgent))]
public class BossAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;

    public enum BossState { Idle, Chasing, Dash, Slam, Enraged, Recovering, Dead, Roaring };
    public BossState curState = BossState.Chasing;

    private Rigidbody2D rb;
    private NavMeshAgent agent;
    private float currentHealth;
    private bool isAttacking = false;
    private float workingNormalSpeed;
    private int facingDirection = 1;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        if (data != null)
        {
            currentHealth = data.enemyHealth;
            workingNormalSpeed = data.normalSpeed;
            agent.speed = workingNormalSpeed;
        }
    }

    private void Update()
    {
        if (curState == BossState.Dead || Player.Instance == null) return;

        // Flip по фактическому направлению движения агента
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Flip(agent.velocity.x);
        }

        if (isAttacking) return;

        float distance = Vector2.Distance(transform.position, Player.Instance.transform.position);

        // Проверка на enrage (ниже 50% HP)
        if (currentHealth < data.enemyHealth * 0.5f && curState != BossState.Enraged && curState != BossState.Roaring)
        {
            StartCoroutine(EnrageSequence());
            return;
        }

        // Атака, если игрок в радиусе
        if (distance <= data.attackRange)
        {
            // В enraged-состоянии чередуем slam и dash
            if (curState == BossState.Enraged)
            {
                StartCoroutine(Random.value > 0.5f ? DashAttack() : SlamAttack());
            }
            else
            {
                StartCoroutine(SlamAttack());
            }
            return;
        }

        // Преследование через NavMesh
        if (curState == BossState.Chasing || curState == BossState.Enraged)
        {
            agent.isStopped = false;
            agent.SetDestination(Player.Instance.transform.position);
        }
    }

    private void Flip(float dir)
    {
        if ((dir > 0 && facingDirection < 0) || (dir < 0 && facingDirection > 0))
        {
            facingDirection *= -1;
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x) * facingDirection,
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    private void StopAgent()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
    }

    private void ResumeAgent()
    {
        agent.isStopped = false;
    }

    IEnumerator SlamAttack()
    {
        isAttacking = true;
        curState = BossState.Slam;
        StopAgent();

        // Замах
        yield return new WaitForSeconds(0.6f);

        // Проверка попадания
        if (Player.Instance != null &&
            Vector2.Distance(transform.position, Player.Instance.transform.position) <= data.attackRange + 1f)
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
            if (Player.Instance.TryGetComponent(out KnockBack kb))
                kb.GetKnockedBack(transform);
        }

        yield return StartCoroutine(Recover(1f));
    }

    IEnumerator DashAttack()
    {
        isAttacking = true;
        curState = BossState.Dash;
        StopAgent();

        // Направление рывка к игроку
        float dashDir = Player.Instance.transform.position.x > transform.position.x ? 1 : -1;
        Flip(dashDir);

        // Подготовка к рывку
        yield return new WaitForSeconds(0.5f);

        // Рывок через rb (агент остановлен, чтобы не конфликтовать)
        float startTime = Time.time;
        while (Time.time < startTime + 0.3f)
        {
            rb.linearVelocity = new Vector2(dashDir * data.dashSpeed, rb.linearVelocity.y);
            yield return null;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        yield return StartCoroutine(Recover(1.5f));
    }

    IEnumerator Recover(float time)
    {
        curState = BossState.Recovering;
        StopAgent();
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        yield return new WaitForSeconds(time);

        isAttacking = false;
        ResumeAgent();
        curState = currentHealth < data.enemyHealth * 0.5f ? BossState.Enraged : BossState.Chasing;
    }

    IEnumerator EnrageSequence()
    {
        isAttacking = true;
        curState = BossState.Roaring;
        StopAgent();
        rb.linearVelocity = Vector2.zero;

        Debug.Log("Босс в ярости!");
        transform.localScale *= 1.2f;

        // Мигание красным
        float elapsed = 0;
        Renderer rend = GetComponent<Renderer>();
        while (elapsed < 1.5f)
        {
            rend.material.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 5, 1));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Увеличение скорости
        workingNormalSpeed = data.normalSpeed * 1.5f;
        agent.speed = workingNormalSpeed;
        rend.material.color = Color.red;

        isAttacking = false;
        ResumeAgent();
        curState = BossState.Enraged;
    }

    public void TakeDamage(float damage)
    {
        if (curState == BossState.Dead) return;
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        curState = BossState.Dead;
        StopAgent();
        agent.enabled = false;
        rb.linearVelocity = Vector2.zero;
        StopAllCoroutines();
        GetComponent<Renderer>().material.color = Color.gray;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
        }
    }
}