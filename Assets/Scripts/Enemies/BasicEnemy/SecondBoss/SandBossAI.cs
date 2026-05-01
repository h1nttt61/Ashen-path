using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SandBossAI : MonoBehaviour
{
    public enum BossState { Intro, Chasing, SandWave, Dash, Burrow, Phase2Leap, Dead }
    public BossState curState = BossState.Intro;

    [Header("References")]
    [SerializeField] private EnemySO data;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private GameObject sandWavePrefab;

    private float currentHealth;
    private bool isPhase2Triggered = false;
    private bool canAttack = true;

    private void Start()
    {
        currentHealth = data.enemyHealth; 
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = data.normalSpeed;
    }

    private void Update()
    {
        if (curState == BossState.Intro || curState == BossState.Dead) return;

        float hpPercent = currentHealth / data.enemyHealth;

        if (hpPercent <= 0.1f && !isPhase2Triggered)
        {
            StartCoroutine(Phase2Sequence());
            return;
        }

        if (canAttack && curState == BossState.Chasing)
        {
            float dist = Vector2.Distance(transform.position, Player.Instance.transform.position);
            if (dist < 5f) ChooseAttack();
            else MoveToPlayer();
        }
    }

    private void MoveToPlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(Player.Instance.transform.position); 
    }

    private void ChooseAttack()
    {
        int rand = Random.Range(0, 3);
        if (rand == 0) StartCoroutine(SandWaveAttack());
        else if (rand == 1) StartCoroutine(DashAttack());
        else StartCoroutine(BurrowAttack());
    }

    IEnumerator SandWaveAttack()
    {
        canAttack = false;
        agent.isStopped = true;
        yield return new WaitForSeconds(0.5f);
        Instantiate(sandWavePrefab, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(1.5f);
        canAttack = true;
    }

    IEnumerator DashAttack()
    {
        canAttack = false;
        Vector2 targetPos = Player.Instance.transform.position;
        agent.speed = data.normalSpeed * 3f;
        agent.SetDestination(targetPos);

        yield return new WaitUntil(() => Vector2.Distance(transform.position, targetPos) < 0.5f);

        agent.speed = data.normalSpeed;
        yield return new WaitForSeconds(1f);
        canAttack = true;
    }

    IEnumerator BurrowAttack()
    {
        canAttack = false;
        sr.color = new Color(1, 1, 1, 0.5f); 
        agent.speed = data.normalSpeed * 1.5f;

        yield return new WaitForSeconds(1f);
        transform.position = Player.Instance.transform.position;

        int damage = Player.Instance.maxHealth / 2;
        Player.Instance.TakeDamage(damage, transform);

        CameraShake.Instance.Shake(0.3f, 0.5f);
        sr.color = Color.white;
        yield return new WaitForSeconds(1f);
        canAttack = true;
    }

    public IEnumerator FadeIn(float duration)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        float t = 0;
        Color startColor = sr.color;
        sr.color = new Color(startColor.r, startColor.g, startColor.b, 0);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(0, 1, t));
            yield return null;
        }
    }
    IEnumerator Phase2Sequence()
    {
        isPhase2Triggered = true;
        canAttack = false;
        curState = BossState.Phase2Leap;
        agent.speed = data.normalSpeed * 4f;

        for (int i = 0; i < 4; i++)
        {
            Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * 10f;
            agent.SetDestination(randomPoint);
            yield return new WaitForSeconds(0.4f);
            CameraShake.Instance.Shake(0.1f, 0.2f);
        }

        agent.speed = data.normalSpeed * 1.3f; 
        canAttack = true;
        curState = BossState.Chasing;
    }

    public void Activate() => curState = BossState.Chasing;
}