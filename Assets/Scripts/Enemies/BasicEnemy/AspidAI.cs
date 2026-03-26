using UnityEngine;
using UnityEngine.AI;

public class AspidAI : MonoBehaviour
{
    private Transform playerTransform;
    private NavMeshAgent navMeshAgent;

    [Header("Дистанционное поведение")]
    [SerializeField] private float preferredDistance = 5f;  // Предпочитаемая дистанция
    [SerializeField] private float distanceTolerance = 1f;   // Допустимое отклонение
    [SerializeField] private float retreatSpeed = 4f;        // Скорость отступления
    [SerializeField] private float approachSpeed = 3f;       // Скорость приближения

    [Header("Атака")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float attackRange = 6f;         // Дальность атаки
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int damage = 1;
    private float lastAttackTime;

    [Header("Здоровье")]
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    private enum AspidState
    {
        Approaching,  // Приближается к игроку
        Retreating,   // Отступает от игрока
        Circling      // Кружит на оптимальной дистанции
    }

    private AspidState currentState = AspidState.Approaching;

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
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            UpdateState(distanceToPlayer);

            switch (currentState)
            {
                case AspidState.Approaching:
                    ApproachPlayer();
                    break;
                case AspidState.Retreating:
                    RetreatFromPlayer();
                    break;
                case AspidState.Circling:
                    CirclePlayer();
                    break;
            }

            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                RangedAttack();
            }
        }
    }

    void UpdateState(float distance)
    {
        if (distance < preferredDistance - distanceTolerance)
        {
            currentState = AspidState.Retreating;
        }
        else if (distance > preferredDistance + distanceTolerance)
        {
            currentState = AspidState.Approaching;
        }
        else
        {
            currentState = AspidState.Circling;
        }
    }

    void ApproachPlayer()
    {
        navMeshAgent.speed = approachSpeed;
        navMeshAgent.SetDestination(playerTransform.position);
    }

    void RetreatFromPlayer()
    {
        navMeshAgent.speed = retreatSpeed;

        Vector3 retreatDirection = (transform.position - playerTransform.position).normalized;
        Vector3 retreatPoint = transform.position + retreatDirection * (preferredDistance * 1.5f);

        retreatPoint = Vector3.ClampMagnitude(retreatPoint - playerTransform.position, 10f) + playerTransform.position;

        navMeshAgent.SetDestination(retreatPoint);
    }

    void CirclePlayer()
    {
        navMeshAgent.speed = (approachSpeed + retreatSpeed) / 2f;

        // Двигаемся по кругу вокруг игрока
        Vector3 direction = (transform.position - playerTransform.position).normalized;
        Vector3 perpendicular = new Vector3(-direction.z, 0, direction.x);

        // Чередуем направление движения
        float circleDirection = Mathf.Sin(Time.time * 0.5f) > 0 ? 1f : -1f;
        Vector3 targetOffset = perpendicular * circleDirection * preferredDistance;

        Vector3 targetPosition = playerTransform.position + direction * preferredDistance + targetOffset;
        navMeshAgent.SetDestination(targetPosition);
    }

    void RangedAttack()
    {
        lastAttackTime = Time.time;
        if (projectilePrefab != null)
        {
            // Создаем снаряд
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Projectile projectileScript = projectile.GetComponent<Projectile>();

            if (projectileScript != null)
            {
                // Вычисляем направление на игрока
                Vector3 direction = (playerTransform.position - transform.position).normalized;
                projectileScript.Initialize(direction, damage);
            }
            else
            {
                Debug.Log("У снаряда отсутствует компонент Projectile!");
            }
        }
        else
        {
            Debug.Log("ProjectilePrefab не назначен в инспекторе!");
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            currentState = AspidState.Retreating;
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}