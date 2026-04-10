using UnityEngine;

public class BatAI : MonoBehaviour
{
   
    private bool isFlockAggressed = false;
    private bool isFlockAnnoying = false;

    [Header("Optimization")]
    [SerializeField] private float activationDistance = 15f;
    [SerializeField] private float sleepCheckInterval = 0.5f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float smoothTime = 0.6f;
    [SerializeField] private float wobbleAmount = 0.3f;
    [SerializeField] private float wobbleSpeed = 1.5f;
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float obstacleDetectionDist = 1.5f;
    [SerializeField] private float avoidForce = 5f;

    [Header("Combat")]
    [SerializeField] private int health = 1;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 2.0f;

    [Header("Flock Settings")]
    [SerializeField] private float separationRadius = 1.5f;
    [SerializeField] private float separationForce = 2f;    
    [SerializeField] private float playerStopDistance = 1.2f;

    private float lastAttackTime;
    private Vector3 currentVelocity;
    private Vector3 randomOffset;
    private bool isFollowingFront;
    private float initialScaleX;

    private Vector3 spawnPosition;
    private float randomTimeOffset;
    private KnockBack knockback;

    void Start()
    {
        isFlockAggressed = false;
        isFlockAnnoying = false;
        initialScaleX = transform.localScale.x;
        isFollowingFront = Random.value > 0.5f;
        spawnPosition = transform.position;
        randomTimeOffset = Random.Range(0f, 100f);

        randomOffset = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(1.0f, 2.5f), 0);

        knockback = GetComponent<KnockBack>();
        InvokeRepeating(nameof(CheckDistanceToPlayer), 0, sleepCheckInterval);
    }

    void CheckDistanceToPlayer()
    {
        if (Player.Instance == null) return;
        float dist = Vector3.Distance(transform.position, Player.Instance.transform.position);
        bool shouldBeActive = dist < activationDistance;
        if (this.enabled != shouldBeActive) this.enabled = shouldBeActive;
    }

    void Update()
    {
        if (Player.Instance == null || !Player.Instance.IsAlive()) return;

        if (knockback != null && knockback.isGettingKnock) return;

        if (!isFlockAggressed && !isFlockAnnoying)
        {
            if (Vector3.Distance(transform.position, Player.Instance.transform.position) <= detectionRadius)
            {
                isFlockAnnoying = true;
            }
        }

        if (isFlockAggressed)
        {
            AttackPlayer();
        }
        else if (isFlockAnnoying)
        {
            AnnoyPlayer();
        }
        else
        {
            IdleCircle();
        }
    }

    public static bool IsAnyBatAnnoying
    {
        get
        {
            BatAI[] allBats = FindObjectsByType<BatAI>(FindObjectsSortMode.None);
            foreach (var bat in allBats)
            {
                if (bat.isFlockAnnoying) return true;
            }
            return false;
        }
    }

    private void IdleCircle()
    {
        float time = (Time.time + randomTimeOffset) * wobbleSpeed;
        float x = Mathf.Cos(time * 0.5f) * 2f;
        float y = Mathf.Sin(time) * 0.5f;

        Vector3 targetPos = spawnPosition + new Vector3(x, y, 0);

        Vector3 finalTarget = targetPos + GetObstacleAvoidanceVector();

        transform.position = Vector3.SmoothDamp(transform.position, finalTarget, ref currentVelocity, smoothTime, moveSpeed);

        FlipSprite(targetPos.x);
    }
    private void AnnoyPlayer()
    {
        float side = isFollowingFront ? 1f : -1f;
        float lookDir = Player.Instance.transform.localScale.x > 0 ? 1 : -1;

        Vector3 targetPos = Player.Instance.transform.position + new Vector3(side * lookDir, 0, 0) + randomOffset;
        targetPos.y += Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;

        Vector3 finalTarget = targetPos + GetSeparationVector() + GetObstacleAvoidanceVector();

        Vector3 finalVelocity = currentVelocity;
        transform.position = Vector3.SmoothDamp(transform.position, finalTarget, ref currentVelocity, smoothTime, moveSpeed);


        FlipSprite(Player.Instance.transform.position.x);
    }

    private void AttackPlayer()
    {
        Vector3 playerPos = Player.Instance.transform.position;
        Vector3 dirToPlayer = (transform.position - playerPos).normalized;

        Vector3 targetPos = playerPos + dirToPlayer * playerStopDistance;
        targetPos.y += Mathf.Sin((Time.time + randomTimeOffset) * wobbleSpeed * 2f) * wobbleAmount;

        Vector3 finalVelocity = currentVelocity;
        Vector3 finalTarget = targetPos + GetSeparationVector() + GetObstacleAvoidanceVector();
        transform.position = Vector3.SmoothDamp(transform.position, finalTarget, ref currentVelocity, smoothTime, moveSpeed);
        currentVelocity = finalVelocity;

        FlipSprite(Player.Instance.transform.position.x);
    }

    private void FlipSprite(float targetX)
    {
        if (transform.position.x < targetX)
            transform.localScale = new Vector3(-Mathf.Abs(initialScaleX), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(initialScaleX), transform.localScale.y, transform.localScale.z);
    }

    public void TakeDamage(int amount)
    {
        isFlockAggressed = true;

        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 10f);
        foreach (var col in nearby)
        {
            if (col.TryGetComponent(out BatAI bat))
            {
                bat.isFlockAggressed = true;
            }
        }

        health -= amount;
        if (knockback != null) knockback.GetKnockedBack(Player.Instance.transform);
        if (health <= 0) Die();
    }

    private void Die()
    {
        BatAI[] remainingBats = FindObjectsByType<BatAI>(FindObjectsSortMode.None);
        if (remainingBats.Length <= 1)
        {
            isFlockAggressed = false;
            isFlockAnnoying = false;
        }

        if (SpiritDIalogManager.Instance != null)
            SpiritDIalogManager.Instance.RegistrKills();

        Destroy(gameObject);
    }

    private Vector3 GetSeparationVector()
    {
        Vector3 separation = Vector3.zero;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, separationRadius);

        foreach (var col in nearby)
        {
            if (col.gameObject == gameObject) continue; 

            if (col.TryGetComponent(out BatAI otherBat))
            {
                float distance = Vector3.Distance(transform.position, otherBat.transform.position);
                Vector3 diff = transform.position - otherBat.transform.position;
                separation += diff.normalized / (distance + 0.1f);
            }
        }
        return separation * separationForce;
    }

    private Vector3 GetObstacleAvoidanceVector()
    {
        Vector3 avoidance = Vector3.zero;
        Vector3 moveDir = currentVelocity.normalized;
        if (moveDir == Vector3.zero) return Vector3.zero;

        Vector2[] rayDirections = {
        moveDir,
        Quaternion.Euler(0, 0, 30) * moveDir,
        Quaternion.Euler(0, 0, -30) * moveDir
    };

        foreach (var dir in rayDirections)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, obstacleDetectionDist, obstacleLayer);

            if (hit.collider != null)
            {
                avoidance += (Vector3)hit.normal * (obstacleDetectionDist - hit.distance);
            }
        }

        return Vector3.ClampMagnitude(avoidance * avoidForce, moveSpeed * 2f);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isFlockAggressed)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Player.Instance.TakeDamage(damage, transform);
                lastAttackTime = Time.time;
            }
        }
    }
}