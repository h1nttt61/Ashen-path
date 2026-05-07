using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class GhostKing : MonoBehaviour
{
    [Header("Base Settings")]
    public float maxHealth = 200f;
    public float currentHealth;
    public float activationDistance = 15f;

    [Header("Dash Settings")]
    public float dashSpeed = 25f;
    public float dashDistanceExtra = 5f; 

    [Header("Minion Settings")]
    public GameObject minionPrefab;
    public float spawnInterval = 30f;

    [Header("Bullet Hell (Spam) Settings")]
    public GameObject projectilePrefab;
    public GameObject visualOnlyProjectilePrefab;
    public int projectileCount = 8;
    public float projectileSpeed = 7f;

    [Header("Visual Effects")]
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;
    private Color originalColor;

    [Header("Phase 2 (Invis)")]
    public float invisDuration = 5f;
    private bool isPhaseTwoTriggered = false;
    private bool isInvisible = false;

    [Header("Audio")]
    public AudioSource roarSource;

    private NavMeshAgent agent;
    private Animator anim;
    private Transform player;
    private SpriteRenderer sprite;
    private bool isAttacking = false;
    private bool isDead = false;
    private float spawnProtection = 1.0f;
    private bool isSpamming = false;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        currentHealth = maxHealth;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (currentHealth <= 0) currentHealth = maxHealth;

        isDead = false;
    }

    void Update()
    {
        if (spawnProtection > 0) spawnProtection -= Time.deltaTime;
        if (!isAttacking && !isDead) FlipSprite();
    }

    public void StartFight()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(BossBehavior());
        StartCoroutine(PassiveSpawn());
    }

    IEnumerator BossBehavior()
    {
        while (currentHealth > 0)
        {
            if (isSpamming) { yield return new WaitForSeconds(0.5f); continue; }

            float dist = Vector3.Distance(transform.position, player.position);

            if (dist < 4f)
            {
                yield return StartCoroutine(KeepDistancePhase(1.5f));
                continue;
            }

            if (dist > activationDistance + 5f)
            {
                agent.SetDestination(player.position);
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            if (!isPhaseTwoTriggered && (currentHealth / maxHealth) <= 0.3f)
                yield return StartCoroutine(InvisibilityPhase());

            float choice = Random.value;
            if (choice < 0.4f) yield return StartCoroutine(SmartDash());
            else if (choice < 0.7f) yield return StartCoroutine(BulletHellAttack());
            else yield return StartCoroutine(KeepDistancePhase(2f));
        }
    }

    IEnumerator SmartDash()
    {
        isAttacking = true;
        anim.SetTrigger("Dash");

        Vector3 dir = (player.position - transform.position).normalized;
        Vector3 targetPos = player.position + dir * dashDistanceExtra;

        agent.speed = dashSpeed;
        agent.acceleration = 100f;
        agent.SetDestination(targetPos);

        float timeout = 1.2f; 
        while (agent.remainingDistance > 0.8f && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        Vector3 escapePos = transform.position + (transform.position - player.position).normalized * 5f;
        agent.speed = 12f;
        agent.SetDestination(escapePos);
        yield return new WaitForSeconds(0.8f);

        agent.speed = 4f;
        agent.acceleration = 8f;
        isAttacking = false;
    }

    IEnumerator BulletHellAttack()
    {
        isAttacking = true;
        Vector3 retreatPos = transform.position + (transform.position - player.position).normalized * 7f;
        agent.SetDestination(retreatPos);
        yield return new WaitForSeconds(1.5f);

        agent.isStopped = true;
        anim.SetTrigger("Spawn");

        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.5f, 0.2f);

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * Mathf.PI * 2 / projectileCount;
            Vector3 spawnDir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            proj.GetComponent<Rigidbody2D>().linearVelocity = spawnDir * projectileSpeed;
        }

        yield return new WaitForSeconds(2f);
        agent.isStopped = false;
        isAttacking = false;
    }

    IEnumerator KeepDistancePhase(float duration)
    {
        float timer = duration;
        while (timer > 0)
        {
            Vector3 target = player.position + (transform.position - player.position).normalized * 8f;
            agent.SetDestination(target);
            timer -= Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator InvisibilityPhase()
    {
        isPhaseTwoTriggered = true;
        isInvisible = true;
        anim.SetTrigger("Idle");

        Color col = sprite.color;
        col.a = 0.2f;
        sprite.color = col;

        float timer = 0;
        while (timer < invisDuration)
        {
            Vector3 target = player.position + (transform.position - player.position).normalized * 6f;
            agent.SetDestination(target);
            timer += Time.deltaTime;
            yield return null;
        }

        col.a = 1f;
        sprite.color = col;
        isInvisible = false;
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.5f, 0.3f);
    }

    IEnumerator PassiveSpawn()
    {
        while (currentHealth > 0)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (isDead) yield break;

            isAttacking = true;
            isSpamming = true;

            Vector3 fleeDir = (transform.position - player.position).normalized;
            agent.SetDestination(transform.position + fleeDir * 10f);
            agent.speed = 10f;

            anim.SetTrigger("Spawn");
            for (int i = 0; i < 3; i++)
            {
                Instantiate(minionPrefab, transform.position + (Vector3)Random.insideUnitCircle * 2f, Quaternion.identity);
            }

            float spamTimer = 4f;
            while (spamTimer > 0)
            {
                for (int j = -1; j <= 1; j++)
                {
                    float angleOffset = j * 15f; // Разброс веера
                    Vector3 dirToPlayer = (player.position - transform.position).normalized;
                    Quaternion rotation = Quaternion.Euler(0, 0, angleOffset);
                    Vector3 finalDir = rotation * dirToPlayer;

                    GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                    proj.GetComponent<Rigidbody2D>().linearVelocity = finalDir * (projectileSpeed * 1.2f);
                }

                spamTimer -= 0.3f; 
                yield return new WaitForSeconds(0.3f);
            }

            agent.speed = 4f;
            isSpamming = false;
            isAttacking = false;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvisible || isDead) return;

        currentHealth -= damage;
        StopCoroutine("FlashEffect"); 
        StartCoroutine(FlashEffect());

        if (currentHealth <= 0)
        {
            isDead = true; 
            Die();
        }
        }

        IEnumerator FlashEffect()
    {
        sprite.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        sprite.color = isInvisible ? new Color(1, 1, 1, 0.2f) : Color.white;
    }

    void Die()
    {
        SaveManager.SaveBossStatus(true);
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        agent.isStopped = true;
        BossArenaManager manager = FindObjectOfType<BossArenaManager>();
        if (manager != null) manager.EndBossFight();

        if (roarSource != null) { roarSource.pitch = 0.8f; roarSource.Play(); }
        anim.SetTrigger("Spawn");
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(3.0f, 1.5f);

        for (int wave = 0; wave < 8; wave++)
        {
            for (int i = 0; i < 25; i++)
            {
                float angle = i * (360f / 25);
                Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                GameObject p = Instantiate(visualOnlyProjectilePrefab, transform.position, Quaternion.identity);
                p.GetComponent<Rigidbody2D>().linearVelocity = dir * (10f + wave);
                Destroy(p, 2f);
            }
            yield return new WaitForSeconds(0.4f);
        }

        float fadeBoss = 1f;
        while (fadeBoss > 0)
        {
            fadeBoss -= Time.deltaTime * 0.4f;
            Color c = sprite.color;
            c.a = fadeBoss;
            sprite.color = c;
            transform.position += (Vector3)Random.insideUnitCircle * 0.03f;
            yield return null;
        }
        if (roarSource != null) roarSource.Stop();

        yield return new WaitForSeconds(5f);

        if (player != null)
        {
            SpriteRenderer playerSprite = player.GetComponentInChildren<SpriteRenderer>();


            if (playerSprite != null)
            {
                float fadePlayer = 1f;
                while (fadePlayer > 0)
                {
                    fadePlayer -= Time.deltaTime * 0.5f;
                    Color pc = playerSprite.color;
                    pc.a = fadePlayer;
                    playerSprite.color = pc;
                    yield return null;
                }
                playerSprite.enabled = false; 
            }
        }
        FinaleManager finale = FindObjectOfType<FinaleManager>();
        if (finale != null)
        {
            finale.StartFinale();
        }

        SaveManager.SaveBossStatus(true);
        Destroy(gameObject);
    }

    void FlipSprite()
    {
        if (player == null) return;
        sprite.flipX = player.position.x < transform.position.x;
    }
}