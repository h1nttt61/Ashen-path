using System.Collections;
using UnityEngine;

public class SpiderAI : MonoBehaviour
{
    [Header("Detection & Jump")]
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float jumpForceX = 7f;
    [SerializeField] private float jumpForceY = 10f;
    [SerializeField] private float jumpCooldown = 3f;

    [Header("Combat")]
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private int poisonDamage = 1;
    [SerializeField] private float poisonDuration = 5f;

    [Header("Death Effect")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Optimization")]
    [SerializeField] private float activationDistance = 25f;
    [SerializeField] private float sleepCheckInterval = 0.5f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;

    private bool isAggressed = false;
    private bool canJump = true;
    private bool isDead = false;
    private float initialScaleX;
    private float nextSleepCheck;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        initialScaleX = transform.localScale.x;

        if (rb != null)
        {
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
        }
    }

    private void Update()
    {
        if (isDead || Player.Instance == null || !Player.Instance.IsAlive()) return;

        float dist = Vector2.Distance(transform.position, Player.Instance.transform.position);

        if (Time.time >= nextSleepCheck)
        {
            nextSleepCheck = Time.time + sleepCheckInterval;
            bool nearby = dist <= activationDistance;

            if (sr.enabled != nearby) sr.enabled = nearby;
            if (rb.simulated != nearby) rb.simulated = nearby;
        }

        if (!sr.enabled) return;

        if (!isAggressed && dist <= detectionRadius)
        {
            isAggressed = true;
        }

        if (isAggressed && canJump && IsGrounded())
        {
            StartCoroutine(JumpRoutine());
        }
    }

    private IEnumerator JumpRoutine()
    {
        canJump = false;

        float dir = transform.position.x < Player.Instance.transform.position.x ? 1 : -1;
        FlipSprite(Player.Instance.transform.position.x);

        rb.linearVelocity = new Vector2(dir * jumpForceX, jumpForceY);

        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    private bool IsGrounded() => Mathf.Abs(rb.linearVelocity.y) < 0.05f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(contactDamage, transform);

            if (Random.value > 0.5f)
            {
                Player.Instance.ApplyPoison(poisonDamage, poisonDuration);
            }

            StartCoroutine(SmoothDeathRoutine());
        }
    }

    public void TakeDamage(int amount)
    {
        if (!isDead) StartCoroutine(SmoothDeathRoutine());
    }

    private IEnumerator SmoothDeathRoutine()
    {
        isDead = true;

        col.enabled = false;
        rb.simulated = false;

        if (SpiritDIalogManager.Instance != null)
            SpiritDIalogManager.Instance.RegistrKills();

        float elapsed = 0;
        Color startColor = sr.color;
        Vector3 startScale = transform.localScale;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float pct = elapsed / fadeDuration;

            sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1, 0, pct));
            transform.localScale = startScale * Mathf.Lerp(1, 1.2f, pct);

            yield return null;
        }

        Destroy(gameObject);
    }

    private void FlipSprite(float targetX)
    {
        float scaleX = transform.position.x < targetX ? -Mathf.Abs(initialScaleX) : Mathf.Abs(initialScaleX);
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
    }
}