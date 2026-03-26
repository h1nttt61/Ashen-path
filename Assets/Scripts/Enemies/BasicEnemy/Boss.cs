using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Boss : MonoBehaviour
{
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float moveSpeed = 3f;

    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = moveSpeed;
    }

    private void Update()
    {
        if (Player.Instance == null) return;

        float distance = Vector2.Distance(transform.position, Player.Instance.transform.position);

        if (distance < detectionRange)
        {
            agent.isStopped = false;
            agent.SetDestination(Player.Instance.transform.position);
        }
        else
        {
            agent.isStopped = true;
        }
    }
}