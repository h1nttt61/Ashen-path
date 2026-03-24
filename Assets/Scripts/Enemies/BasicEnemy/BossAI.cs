using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;
    public enum BossState { Idle, Chasing, Dash, Slam, Enraged, Recovering, Dead, Roaring };
    public BossState curState = BossState.Chasing;

    private Rigidbody2D rb;
    private float currentHealth;
    private bool isAttacking = false;
    private float workingNormalSpeed;
    private int facingDirection = 1;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (data != null)
        {
            currentHealth = data.enemyHealth;
            workingNormalSpeed = data.normalSpeed;
        }
    }

    private void FixedUpdate() 
    {
        if (curState == BossState.Dead || isAttacking || Player.Instance == null)
        {
            if (!isAttacking && curState != BossState.Dead)
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (curState == BossState.Chasing || curState == BossState.Enraged)
        {
            float moveDir = Player.Instance.transform.position.x > transform.position.x ? 1 : -1;
            rb.linearVelocity = new Vector2(moveDir * workingNormalSpeed, rb.linearVelocity.y);
            Flip(moveDir);
        }
    }

    private void Update()
    {
        if (curState == BossState.Dead || isAttacking || Player.Instance == null) return;

        float distance = Vector2.Distance(transform.position, Player.Instance.transform.position);

        // Фаза ярости
        if (currentHealth < data.enemyHealth * 0.5f && curState != BossState.Enraged && curState != BossState.Roaring)
        {
            StartCoroutine(EnrageSequence());
        }

        if (distance <= data.attackRange)
        {
            StartCoroutine(SlamAttack());
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

    IEnumerator SlamAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.6f); 

        if (Vector2.Distance(transform.position, Player.Instance.transform.position) <= data.attackRange + 1f)
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
            if (Player.Instance.TryGetComponent(out KnockBack kb)) kb.GetKnockedBack(transform);
        }

        yield return StartCoroutine(Recover(1f));
    }

    IEnumerator DashAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        float dashDir = Player.Instance.transform.position.x > transform.position.x ? 1 : -1;
        Flip(dashDir);

        yield return new WaitForSeconds(0.5f);

        float startTime = Time.time;
        while (Time.time < startTime + 0.3f) 
        {
            rb.linearVelocity = new Vector2(dashDir * data.dashSpeed, rb.linearVelocity.y);
            yield return null;
        }

        yield return StartCoroutine(Recover(1.5f));
    }

    IEnumerator Recover(float time)
    {
        curState = BossState.Recovering;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        yield return new WaitForSeconds(time);

        isAttacking = false;
        curState = BossState.Chasing;
    }

    IEnumerator EnrageSequence()
    {
        isAttacking = true;
        curState = BossState.Roaring;
        rb.linearVelocity = Vector2.zero;

        Debug.Log("���� � ������!");
        transform.localScale *= 1.2f;

        float elapsed = 0;
        Renderer rend = GetComponent<Renderer>();
        while (elapsed < 1.5f)
        {
            rend.material.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 5, 1));
            elapsed += Time.deltaTime;
            yield return null;
        }

        workingNormalSpeed = data.normalSpeed * 1.5f;
        rend.material.color = Color.red;

        isAttacking = false;
        curState = BossState.Enraged;
    }

    public void TakeDamage(float damage)
    {
        if (curState == BossState.Dead) return;
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        curState = BossState.Dead;
        rb.linearVelocity = Vector2.zero;
        StopAllCoroutines();
        GetComponent<Renderer>().material.color = Color.gray;
        Debug.Log("���� �����!");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
        }
    }
}