using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class BossAI : MonoBehaviour
{
    public enum BossState { Idle, Chasing, Dash, Slam, Enraged, Recovering, Dead, Roaring };
    public BossState curState = BossState.Idle;

    [Header("Stats")]
    public float health = 200f;
    private float maxHealth;
    public float dashSpeed = 20f;
    public float normalSpeed = 3.5f;

    [Header("Settings")]
    public Transform player;
    public LayerMask playerLayer;
    private NavMeshAgent agent;
    private bool isAttacking = false;

    private void Start()
    {
        maxHealth = health;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = normalSpeed;
        curState = BossState.Chasing;
    }

    private void Update()
    {
        if (curState == BossState.Dead || isAttacking) return;
        float disctance = Vector3.Distance(transform.position, player.position);

        if (health < maxHealth * 0.5f && curState != BossState.Enraged)
            StartPhaseTwo();

        switch (curState)
        {
            case BossState.Chasing:
                HandleMovement(disctance);
                break;
            case BossState.Recovering:
                //–≈¿À»«Œ¬¿“‹ ¿Õ»Ã¿÷»» ƒÀﬂ "œ≈–≈ƒ€ÿ » ¡Œ——¿"
                break;
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

        GetComponent<Renderer>().material.color = Color.yellow;
        yield return new WaitForSeconds(1f);

        Debug.Log("¡Œ—— ”ƒ¿–»À œŒ «≈ÃÀ≈");

        //ƒÓ·‡‚ËÚ¸ ÛÓÌ ÔÓ ÔÂÒÛ!!!
        GetComponent<Renderer>().material.color = Color.white;
        yield return new WaitForSeconds(1.5f);
    }


    IEnumerator DashAttack()
    {
        isAttacking = true;
        agent.isStopped = true;

        Vector3 dashDir = (player.position - transform.position).normalized;

        //«‡ÏË‡ÌËÂ ÔÂÂ‰ ‰Â¯ÂÏ
        GetComponent<Renderer>().material.color = Color.blue;
        yield return new WaitForSeconds(1.5f);

        float startTime = Time.time;
        while (Time.time < startTime + 0.4f)
        {
            transform.Translate(dashDir * dashSpeed * Time.deltaTime, Space.World);
            yield return null;
        }

        GetComponent<Renderer>().material.color = Color.white;
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

        Debug.Log("’¿’¿’¿’¿’¿’");
        transform.localScale *= 1.2f; 

        float elapsed = 0;
        while (elapsed < 1.5f)
        {
            GetComponent<Renderer>().material.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 5, 1));
            elapsed += Time.deltaTime;
            yield return null;
        }

     
        normalSpeed *= 1.6f;
        agent.speed = normalSpeed;
        GetComponent<Renderer>().material.color = Color.red; 

        isAttacking = false;
        agent.isStopped = false;
        curState = BossState.Chasing;
    }
}
