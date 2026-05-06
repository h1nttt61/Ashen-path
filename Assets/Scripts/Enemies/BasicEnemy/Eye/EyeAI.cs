using UnityEngine;
using System.Collections;

public class EyeAI : MonoBehaviour
{
    private enum EyeState { Idle, Attacking, Cooldown }
    [SerializeField] private EyeState currentState = EyeState.Idle;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float smoothTime = 0.6f;
    [SerializeField] private float detectionRadius = 12f;
    [SerializeField] private float stopDistance = 5f;
    private Vector3 currentVelocity;

    [Header("Combat Settings")]
    [SerializeField] private int health = 8;
    [SerializeField] private float attackRange = 9f;
    [SerializeField] private float laserMaxDistance = 15f;
    [SerializeField] private int damagePerSecond = 1;

    [Header("Timings")]
    [SerializeField] private float fireDuration = 2.0f;
    [SerializeField] private float cooldownTime = 1.5f;

    [Header("References")]
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private LayerMask obstacleLayer;
    [Header("Laser Settings")]
    [SerializeField] private LineRenderer lineRenderer;

    private Rigidbody2D rb;
    private float lastDamageTime;
    private float initialScale;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = Mathf.Abs(transform.localScale.x);
    }

    void FixedUpdate()
    {
        if (Player.Instance == null || !Player.Instance.IsAlive()) return;

        float dist = Vector2.Distance(transform.position, Player.Instance.transform.position);

        switch (currentState)
        {
            case EyeState.Idle:
                HandleMovement(dist);
                LookAtPlayer();
                if (dist <= attackRange) StartCoroutine(AttackSequence());
                break;

            case EyeState.Attacking:
                currentVelocity = Vector3.zero;
                UpdateLaser();
                break;

            case EyeState.Cooldown:
                RaycastHit2D groundCheck = Physics2D.Raycast(transform.position, Vector2.down, 1f, obstacleLayer);
                if (groundCheck.collider == null)
                {
                    rb.linearVelocity = new Vector2(0, -0.5f);
                }
                else
                {
                    rb.linearVelocity = Vector2.zero;
                }
                break; ;
        }
    }

    private void HandleMovement(float dist)
    {
        if (dist <= detectionRadius && dist > stopDistance)
        {
            Vector3 targetPos = Player.Instance.transform.position;
            Vector3 nextPos = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime, moveSpeed);
            rb.MovePosition(nextPos);
        }
    }

    private void LookAtPlayer()
    {
        Vector3 dir = Player.Instance.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle + 180f);

        if (dir.x > 0)
            transform.localScale = new Vector3(initialScale, -initialScale, 1f);
        else
            transform.localScale = new Vector3(initialScale, initialScale, 1f);
    }

    private void UpdateLaser()
    {
        Vector2 fireDir = -transform.right;

        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, fireDir, laserMaxDistance, obstacleLayer);
        float distToWall = wallHit.collider != null ? wallHit.distance : laserMaxDistance;

        RaycastHit2D playerHit = Physics2D.Raycast(transform.position, fireDir, distToWall, 1 << LayerMask.NameToLayer("Player"));

        float finalDist = distToWall;

        if (playerHit.collider != null)
        {
            if (Time.time > lastDamageTime + 1f)
            {
                Player.Instance.TakeDamage(damagePerSecond, transform); 
            lastDamageTime = Time.time;
            }
        }

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, new Vector3(-finalDist, 0, 0));
    }

    private IEnumerator AttackSequence()
    {
        currentState = EyeState.Attacking;

        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.3f);

        lineRenderer.enabled = true;
        yield return new WaitForSeconds(fireDuration);

        lineRenderer.enabled = false;
        currentState = EyeState.Cooldown;
        anim.SetTrigger("Cooldown");
        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        yield return new WaitForSeconds(cooldownTime);

        spriteRenderer.color = Color.white;
        currentState = EyeState.Idle;
        anim.SetTrigger("Idle");
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}