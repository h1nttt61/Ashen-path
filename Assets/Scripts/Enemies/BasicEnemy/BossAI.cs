using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NavMeshAgent))]
public class BossAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;
    public enum BossState { Idle, Chasing, Attacking, Retreating, Healing, Dead, Enranged};
    public BossState curState = BossState.Chasing;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private NavMeshAgent agent;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private float currentHealth;
    private bool isAttacking = false;
    private bool isHealing = false;
    private int facingDirection = 1;
    private bool canDamagePlayer = false;
    private bool isIntroDone = false;
    private float delayLavaAttack;
    private float attackRange = 4.5f;
    private GameObject bossDoor;
 
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        bossDoor = GameObject.Find("BoosDoor");

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Start()
    {
        if (data != null) currentHealth = data.enemyHealth;

        agent.enabled = true;
        agent.speed = data.normalSpeed;
        agent.stoppingDistance = 4.0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true; 
    }

    private void Update()
    {
        if (!isIntroDone || curState == BossState.Dead || Player.Instance == null) return;

        if (currentHealth < data.enemyHealth * 0.4f && !isHealing)
        {
            StartCoroutine(HealRoutine());
        }

        if (animator != null)
        {
            bool isMoving = agent.enabled && agent.velocity.magnitude > 0.1f && !agent.isStopped;
            animator.SetBool("isChasing", isMoving);
        }

        if (curState == BossState.Chasing)
        {
            MoveToPlayer();
        }
    }
    public void ActivateBoss()
    {
        isIntroDone = true;
        canDamagePlayer = true;
        curState = BossState.Chasing;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (curState == BossState.Dead) return;

        if (collision.gameObject.CompareTag("Player") && canDamagePlayer)
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
            StartCoroutine(TouchDamageCooldown());
        }
    }

    private IEnumerator TouchDamageCooldown()
    {
        canDamagePlayer = false;
        yield return new WaitForSeconds(1.0f); 
        canDamagePlayer = true;
    }

    private IEnumerator HealRoutine()
    {
        isHealing = true;
        while (currentHealth < data.enemyHealth && curState != BossState.Dead)
        {
            currentHealth += 1; 
            spriteRenderer.color = Color.green;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.5f);
        }
        isHealing = false;
    }

    private void MoveToPlayer()
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            Vector2 playerPos = Player.Instance.transform.position;
            LayerMask wallLayer = LayerMask.GetMask("Wall");
                        
            float distanceToPlayer = Vector3.Distance(transform.position, playerPos);
            float distToDoor = (bossDoor != null) ? Vector2.Distance(transform.position, bossDoor.transform.position) : 100f;

            //agent.SetDestination(playerPos);
            //Flip(agent.velocity.x);
            Vector2 directionToPlayer = (playerPos - (Vector2)transform.position).normalized;
            RaycastHit2D wallCheck = Physics2D.Raycast(playerPos, directionToPlayer, 2.0f, wallLayer);

            if ((wallCheck.collider != null && distanceToPlayer < attackRange + 1.0f) || distToDoor < attackRange * 1.5f)
            {
                // 2. The player is near a wall! Move the boss BACKWARD.
                Vector2 retreatPos = (Vector2)transform.position - (directionToPlayer * attackRange * 1.5f);
                agent.SetDestination(retreatPos);
            }
            else if (distanceToPlayer > attackRange)
            {
                // 3. Normal follow behavior
                agent.isStopped = false;
                agent.SetDestination(playerPos);
            }
            else
            {
                // 4. Close enough to attack, stop moving
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            Flip(agent.velocity.x);
        }
    }

    private void Flip(float dir)
    {
        if ((dir > 0 && facingDirection < 0) || (dir < 0 && facingDirection > 0))
        {
            facingDirection *= -1;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facingDirection, transform.localScale.y, transform.localScale.z);
        }
    }


    private void Enreged()
    {

        if (currentHealth < data.enemyHealth * 0.3f)
        {
            StartCoroutine(LavaAttack(1f));
            StartCoroutine(OverHeal());
        }
    }

    private IEnumerator LavaAttack(float delay)
    {

        delayLavaAttack = (float)Random.Range(2, 4);
        while (delay < delayLavaAttack)
        {
            Player.Instance.TakeDamage(1, Player.Instance.transform);
            yield return null;
        }
        yield return new WaitForSeconds(8); //каждые 8 секунды можеты
    }
    private IEnumerator OverHeal()
    {
        while (currentHealth < data.enemyHealth && curState != BossState.Dead)
        {
            currentHealth += 3;
            spriteRenderer.color = Color.green;
            yield return new WaitForSeconds(0.5f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.01f);
        }
        isHealing = false;
    }
    public void TakeDamage(float damage)
    {
        if (curState == BossState.Dead || !isIntroDone) return;

        currentHealth -= damage;
        StopCoroutine(nameof(DamageFlash));
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0) Die();
    }

    IEnumerator DamageFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = isHealing ? Color.green : Color.white;
    }

    private void Die()
    {
        curState = BossState.Dead;

        agent.isStopped = true;
        agent.enabled = false;
        rb.simulated = false; 

        if (animator != null)
        {
            animator.SetBool("isChasing", false);
            animator.enabled = false; 
        }

        SaveManager.SaveBossStatus(true);
        StopAllCoroutines();
        StartCoroutine(DeathSequence()); 
    }

    IEnumerator DeathSequence()
    {
        spriteRenderer.color = Color.gray;

        float elapsed = 0;
        float duration = 2f;
        Color startColor = spriteRenderer.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}