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

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private NavMeshAgent agent;
    private float currentHealth;
    private bool isAttacking = false;
    private float workingNormalSpeed;
    private int facingDirection = 1;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody2D>();

        agent.enabled = false;

        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
        {
            transform.position = hit.position; 
            agent.enabled = true;             
            agent.Warp(hit.position);          
        }
        else
        {
            Debug.LogError("Босс не нашел NavMesh! Проверь Z-координату сетки.");
        }

        rb.simulated = true;

        if (data != null)
        {
            currentHealth = data.enemyHealth;
            workingNormalSpeed = data.normalSpeed;
            agent.speed = workingNormalSpeed;
        }
    }

    private void FixedUpdate()
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;

        if (curState == BossState.Dead || isAttacking || Player.Instance == null)
        {
            if (!isAttacking && curState != BossState.Dead)
                StopAgent();
            return;
        }

        if (curState == BossState.Chasing || curState == BossState.Enraged)
        {
            agent.isStopped = false;
            agent.SetDestination(Player.Instance.transform.position);

            if (agent.velocity.sqrMagnitude > 0.01f)
                Flip(agent.velocity.x);
        }
    }

    private void Update()
    {
        if (curState == BossState.Dead || isAttacking || Player.Instance == null) return;

        float distance = Vector2.Distance(transform.position, Player.Instance.transform.position);

        if (currentHealth < data.enemyHealth * 0.5f && curState != BossState.Enraged && curState != BossState.Roaring)
        {
            StartCoroutine(EnrageSequence());
        }

        if (distance <= data.attackRange)
        {
            StartCoroutine(SlamAttack());
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

    private void StopAgent()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
    }

    IEnumerator SlamAttack()
    {
        isAttacking = true;
        StopAgent();

        yield return new WaitForSeconds(0.6f);

        if (Vector2.Distance(transform.position, Player.Instance.transform.position) <= data.attackRange + 1f)
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
            if (Player.Instance.TryGetComponent(out KnockBack kb)) kb.GetKnockedBack(transform);
        }

        yield return StartCoroutine(Recover(1f));
    }

    IEnumerator DashAttack()
    {
        isAttacking = true;
        StopAgent();

        float dashDir = Player.Instance.transform.position.x > transform.position.x ? 1 : -1;
        Flip(dashDir);

        yield return new WaitForSeconds(0.5f);

        rb.simulated = true;
        float startTime = Time.time;
        while (Time.time < startTime + 0.3f)
        {
            rb.linearVelocity = new Vector2(dashDir * data.dashSpeed, rb.linearVelocity.y);
            yield return null;
        }
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        yield return StartCoroutine(Recover(1.5f));
    }

    IEnumerator Recover(float time)
    {
        curState = BossState.Recovering;
        StopAgent();
        yield return new WaitForSeconds(time);

        isAttacking = false;
        agent.isStopped = false;
        curState = BossState.Chasing;
    }

    IEnumerator EnrageSequence()
    {
        isAttacking = true;
        curState = BossState.Roaring;
        StopAgent();

        Debug.Log("Босс в ярости!");
        transform.localScale *= 1.2f;

        float elapsed = 0;
        Renderer rend = GetComponent<Renderer>();
        while (elapsed < 1.5f)
        {
            rend.material.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 5, 1));
            elapsed += Time.deltaTime;
            yield return null;
        }

        workingNormalSpeed = data.normalSpeed * 1.5f;
        agent.speed = workingNormalSpeed;
        rend.material.color = Color.red;

        isAttacking = false;
        agent.isStopped = false;
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