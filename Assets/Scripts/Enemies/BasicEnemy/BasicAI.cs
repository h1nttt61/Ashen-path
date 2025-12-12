using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
public class BasicAI : MonoBehaviour
{
    public enum State { Patrol, Chase};
    public State state = State.Patrol;

    public float visionRange = 5f;
    public float attackRange = 1.5f;
    public Transform[] patrolPoints;

    public Transform playerTarget;
    private NavMeshAgent agent;
    public int currentPatrolIndex = 0;


    public void Start()
    {
        agent = GetComponent<NavMeshAgent>();

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
        if (distToPlayer < visionRange)
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
        if (distToPlayer > visionRange * 1.2f)
        {
            state = State.Patrol;

            if (patrolPoints.Length > 0)
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);

            return;
        }

        if (distToPlayer <= attackRange)
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
}