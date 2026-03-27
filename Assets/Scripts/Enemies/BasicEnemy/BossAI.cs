using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NavMeshAgent))]
public class BossAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;
    public enum BossState { Idle, Chasing, Dead, Recovering };
    public BossState curState = BossState.Chasing;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private NavMeshAgent agent;
    private SpriteRenderer spriteRenderer;

    private float currentHealth;
    private bool isAttacking = false;
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

    private void Update()
    {
        if (curState == BossState.Dead || Player.Instance == null) return;

        if (curState == BossState.Chasing && !isAttacking)
        {
            MoveToPlayer();
        }

        float distance = Vector2.Distance(transform.position, Player.Instance.transform.position);
        if (distance <= data.attackRange && !isAttacking)
        {
            StartCoroutine(SimpleAttack());
        }
    }

    private void MoveToPlayer()
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(Player.Instance.transform.position);
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

    IEnumerator SimpleAttack()
    {
        isAttacking = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(0.5f);

        if (Vector2.Distance(transform.position, Player.Instance.transform.position) <= data.attackRange + 0.5f)
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
        }

        yield return new WaitForSeconds(0.5f); 

        agent.isStopped = false;
        isAttacking = false;
    }

    public void TakeDamage(float damage)
    {
        if (curState == BossState.Dead) return; 

        currentHealth -= damage;
        Debug.Log($"Босс получил урон! Осталось HP: {currentHealth}");

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        curState = BossState.Dead;
        agent.enabled = false;
        if (spriteRenderer != null) spriteRenderer.color = Color.gray;
        Debug.Log("Босс повержен");
    }
}