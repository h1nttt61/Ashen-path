using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class BossAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;
    public enum BossState { Idle, Chasing, Dash, Slam, Enraged, Recovering, Dead, Roaring };
    public BossState curState = BossState.Chasing;


    [Header("Settings")]
    private NavMeshAgent agent;
    private float currentHealth;
    private bool isAttacking = false;
    private float workingNormalSpeed;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (data != null)
        {
            currentHealth = data.enemyHealth;
            workingNormalSpeed = data.normalSpeed;
            agent.speed = workingNormalSpeed;
        }

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        if (curState == BossState.Dead || isAttacking || Player.Instance == null) return;
        float disctance = Vector3.Distance(transform.position, Player.Instance.transform.position);

        if (currentHealth < data.enemyHealth * 0.5f && curState != BossState.Enraged)
            StartPhaseTwo();

        if (curState == BossState.Chasing)
        {
            if (disctance <= data.attackRange)
                StartCoroutine(SlamAttack());
            else if (disctance > data.attackRange)
                StartCoroutine(DashAttack());
            else
                agent.SetDestination(Player.Instance.transform.position);
        }
    }

    private void HandleMovement(float distance)
    {
        if (distance <= 4)
            StartCoroutine(SlamAttack());
        else if (distance > 10f)
            StartCoroutine(DashAttack());
    }

    IEnumerator SlamAttack()
    {
        isAttacking = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(1f); 
        if (Vector3.Distance(transform.position, Player.Instance.transform.position) <= data.attackRange + 1f)
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
            if (Player.Instance.TryGetComponent(out KnockBack kb)) kb.GetKnockedBack(transform);
        }

        yield return StartCoroutine(Recover(1.5f));
    }


    IEnumerator DashAttack()
    {
        isAttacking = true;
        Vector3 dashDir = (Player.Instance.transform.position - transform.position).normalized;

        yield return new WaitForSeconds(1f); 

        float startTime = Time.time;
        agent.speed = data.dashSpeed;

        while (Time.time < startTime + 0.4f)
        {
            transform.Translate(dashDir * data.dashSpeed * Time.deltaTime, Space.World);
            yield return null;
        }

        yield return StartCoroutine(Recover(2f));
    }
    IEnumerator Recover(float time)
    {
        curState = BossState.Recovering;
        yield return new WaitForSeconds(time);

        isAttacking = false;
        agent.isStopped = false;
        curState = BossState.Chasing;
    }

    void StartPhaseTwo()
    {
        StartCoroutine(EnrageSequence());
    }

    IEnumerator EnrageSequence()
    {
        isAttacking = true;
        curState = BossState.Roaring;
        agent.isStopped = true;

        Debug.Log("ÁÎÑÑ Â ßÐÎÑÒÈ!");
        transform.localScale *= 1.2f;

        float elapsed = 0;
        while (elapsed < 1.5f)
        {
            GetComponent<Renderer>().material.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 5, 1));
            elapsed += Time.deltaTime;
            yield return null;
        }

        workingNormalSpeed = data.normalSpeed * 1.6f; 
        agent.speed = workingNormalSpeed;
        GetComponent<Renderer>().material.color = Color.red;

        isAttacking = false;
        agent.isStopped = false;
        curState = BossState.Enraged;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);
            if (collision.gameObject.TryGetComponent(out KnockBack kb)) kb.GetKnockedBack(transform);
        }
    }
}
