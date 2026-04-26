using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
public class BasicAI : MonoBehaviour
{
    public enum State { Patrol, Chase};
    public State state = State.Patrol;

    [Header("Data")]
    [SerializeField] private EnemySO data;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public int currentPatrolIndex = 0;

    public Transform playerTarget;
    private NavMeshAgent agent;
   


    public void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        if (data != null) agent.speed = data.normalSpeed;

        if (Player.Instance != null)
            playerTarget = Player.Instance.transform;

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    public void Update()
    {
        if (playerTarget == null) return;

        float distToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        switch (state)
        {
            case State.Patrol:
                PatrolUpdate(distToPlayer);
                break;
            case State.Chase:
                ChaseUpdate(distToPlayer);
                break;
        }
    }

    private void PatrolUpdate(float distToPlayer)
    {
        if (distToPlayer < data.detectionRange)
        {
            state = State.Chase;
            return;
        }
        if (patrolPoints.Length == 0) return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private void ChaseUpdate(float distToPlayer)
    {
        if (distToPlayer > data.detectionRange * 1.2f)
        {
            state = State.Patrol;

            if (patrolPoints.Length > 0)
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);

            return;
        }

        if (distToPlayer <= data.attackRange)
        {
            //agent.isStopped = true;
            //TODO: Create attack
            return;
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
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
}