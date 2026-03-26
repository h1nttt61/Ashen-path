using UnityEngine;
using UnityEngine.AI;

public class BatAI : MonoBehaviour
{
    private Transform playerTransform;
    private NavMeshAgent navMeshAgent;

    [Header("Атака")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackRange = 3f;
    private float lastAttackTime;

    [Header("Здоровье")]
    [SerializeField] private int maxHealth = 4;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;
        }
    }

    void Update()
    {
        if (playerTransform != null && navMeshAgent != null && currentHealth > 0)
        {
            navMeshAgent.SetDestination(playerTransform.position);

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
    }

    void Attack()
    {
        //Debug.Log("-1");
        Player.Instance.TakeDamage(damage,transform);
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}