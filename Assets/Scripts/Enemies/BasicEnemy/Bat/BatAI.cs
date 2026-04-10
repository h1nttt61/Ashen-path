using UnityEngine;

public class BatAI : MonoBehaviour
{
    public static bool IsAnyBatAnnoying => isFlockAnnoying;
    private static bool isFlockAggressed = false;
    private static bool isFlockAnnoying = false;

    [Header("Optimization")]
    [SerializeField] private float activationDistance = 15f;
    [SerializeField] private float sleepCheckInterval = 0.5f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float smoothTime = 0.6f;
    [SerializeField] private float wobbleAmount = 0.3f;
    [SerializeField] private float wobbleSpeed = 1.5f;
    [SerializeField] private float detectionRadius = 8f;

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

    private void IdleCircle()
    {
        float x = Mathf.Cos((Time.time + randomTimeOffset) * moveSpeed * 0.5f) * 2f;
        float y = Mathf.Sin((Time.time + randomTimeOffset) * moveSpeed * 0.5f) * 1f;

        Vector3 targetPos = spawnPosition + new Vector3(x, y, 0);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime, moveSpeed);

        FlipSprite(targetPos.x);
    }

    private void AnnoyPlayer()
    {
        float side = isFollowingFront ? 1f : -1f;
        float lookDir = Player.Instance.transform.localScale.x > 0 ? 1 : -1;

        Vector3 targetPos = Player.Instance.transform.position + new Vector3(side * lookDir, 0, 0) + randomOffset;
        targetPos.y += Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;

        Vector3 finalVelocity = currentVelocity;
        Vector3 movePos = Vector3.SmoothDamp(transform.position, targetPos + GetSeparationVector(), ref finalVelocity, smoothTime, moveSpeed);

        transform.position = movePos;
        currentVelocity = finalVelocity;

        FlipSprite(Player.Instance.transform.position.x);
    }

    private void AttackPlayer()
    {
        Vector3 playerPos = Player.Instance.transform.position;
        Vector3 dirToPlayer = (transform.position - playerPos).normalized;

        Vector3 targetPos = playerPos + dirToPlayer * playerStopDistance;
        targetPos.y += Mathf.Sin((Time.time + randomTimeOffset) * wobbleSpeed * 2f) * wobbleAmount;

        Vector3 finalVelocity = currentVelocity;
        Vector3 movePos = Vector3.SmoothDamp(transform.position, targetPos + GetSeparationVector(), ref finalVelocity, smoothTime / 2f, moveSpeed * 1.5f);

        transform.position = movePos;
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
        BatAI[] allBats = FindObjectsByType<BatAI>(FindObjectsSortMode.None);

        foreach (var bat in allBats)
        {
            if (bat == this) continue;

            float distance = Vector3.Distance(transform.position, bat.transform.position);
            if (distance < separationRadius)
            {
                Vector3 diff = transform.position - bat.transform.position;
                separation += diff.normalized / (distance + 0.1f);
            }
        }
        return separation * separationForce;
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