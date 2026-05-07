using UnityEngine;
using System.Collections;

public class EyeAI : MonoBehaviour
{
    private enum EyeState { Idle, Gaze, Cooldown }
    [SerializeField] private EyeState currentState = EyeState.Idle;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float smoothTime = 0.6f;
    [SerializeField] private float detectionRadius = 12f;
    [SerializeField] private float stopDistance = 6f;
    private Vector3 currentVelocity;

    [Header("Gaze Settings")]
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float maxAttackAngle = 30f;
    [SerializeField] private float timeToPetrify = 1.5f; 
    [SerializeField] private float petrifyDuration = 2.5f; 
    private float gazeTimer = 0f;

    [Header("Timings")]
    [SerializeField] private float attackCooldown = 20f;

    [Header("References")]
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Health")]
    [SerializeField] private int health = 8;

    private Rigidbody2D rb;
    private float initialScale;
    private Color normalColor = Color.white;
    private Color chargingColor = new Color(0.8f, 0f, 0f, 1f); 



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = Mathf.Abs(transform.localScale.x);
        normalColor = spriteRenderer.color;
    }

    void FixedUpdate()
    {
        if (Player.Instance == null || !Player.Instance.IsAlive()) return;

        float dist = Vector2.Distance(transform.position, Player.Instance.transform.position);

        switch (currentState)
        {
            case EyeState.Idle:
                HandleMovement(dist);
                RotateTowardsPlayer(true);
                if (dist <= attackRange && IsPlayerInView()) currentState = EyeState.Gaze;
                break;

            case EyeState.Gaze:
                RotateTowardsPlayer(false); 
                if (IsPlayerInView())
                {
                    gazeTimer += Time.fixedDeltaTime;
                    spriteRenderer.color = Color.Lerp(normalColor, chargingColor, gazeTimer / timeToPetrify);

                    if (gazeTimer >= timeToPetrify) ExecutePetrify();
                }
                else
                {
                    gazeTimer -= Time.fixedDeltaTime;
                    spriteRenderer.color = Color.Lerp(normalColor, chargingColor, gazeTimer / timeToPetrify);
                    if (gazeTimer <= 0) { gazeTimer = 0; currentState = EyeState.Idle; }
                }
                break;

            case EyeState.Cooldown:
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime);
                break;
        }
    }

    private void RotateTowardsPlayer(bool smooth)
    {
        if (Player.Instance == null) return;

        Vector3 dir = Player.Instance.transform.position - transform.position;
        float yRot = (dir.x < 0) ? 0f : 180f;

        float angle = Mathf.Atan2(dir.y, Mathf.Abs(dir.x)) * Mathf.Rad2Deg;

        float clampedZ = Mathf.Clamp(angle, -maxAttackAngle, 0f);

        Quaternion targetRot = Quaternion.Euler(0, yRot, clampedZ);
        transform.rotation = smooth ? Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f) : targetRot;
    }

    private bool IsPlayerInView()
    {
        Vector2 dirToPlayer = (Player.Instance.transform.position - transform.position).normalized;
        return Vector2.Angle(-transform.right, dirToPlayer) <= maxAttackAngle;
    }

    private void ExecutePetrify()
    {
        Player.Instance.Petrify(petrifyDuration);
        anim.SetTrigger("Attack");
        StartCoroutine(StartCooldown());
    }

    private IEnumerator StartCooldown()
    {
        currentState = EyeState.Cooldown;
        gazeTimer = 0;
        spriteRenderer.color = new Color(0.2f, 0.2f, 0.2f, 1f); 
        yield return new WaitForSeconds(attackCooldown);
        spriteRenderer.color = normalColor;
        currentState = EyeState.Idle;
    }

    public void TakeDamage(int amount)
    {
        if (health <= 0) return; 

        health -= amount;

        StopCoroutine(DamageFlash());
        StartCoroutine(DamageFlash());

        if (health <= 0)
        {
            StartCoroutine(SmoothDeath());
        }
    }

    private IEnumerator SmoothDeath()
    {
        currentState = EyeState.Cooldown;
        GetComponent<Collider2D>().enabled = false;
        rb.gravityScale = 0.5f; 
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -1f);

        float duration = 1.5f;
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            transform.localScale = Vector3.Lerp(new Vector3(initialScale, initialScale, 1f), Vector3.zero, elapsed / duration);

            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator DamageFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = (currentState == EyeState.Cooldown) ? new Color(0.2f, 0.2f, 0.2f, 1f) : Color.white;
    }

    private void HandleMovement(float dist)
    {
        if (dist <= detectionRadius && dist > stopDistance)
        {
            Vector3 nextPos = Vector3.SmoothDamp(transform.position, Player.Instance.transform.position, ref currentVelocity, smoothTime, moveSpeed);
            rb.MovePosition(nextPos);
        }
    }
}