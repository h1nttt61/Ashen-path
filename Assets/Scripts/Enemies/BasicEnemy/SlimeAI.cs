using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SlimeAI : MonoBehaviour
{
    [SerializeField] private EnemySO data;
    public enum State { Idle, Chase, Dash, Cooldown };
    public State curState = State.Idle;

    private NavMeshAgent agent;
    private bool isActionActive = false; 
    private float currentHealth; 

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        if (data != null)
        {
            agent.speed = data.normalSpeed;
            currentHealth = data.enemyHealth; 
        }
    }

    private void Update()
    {
        if (Player.Instance == null || !Player.Instance.IsAlive()) return;

        float distance = Vector3.Distance(transform.position, Player.Instance.transform.position);

        switch (curState)
        {
            case State.Idle:
                if (distance < data.detectionRange) curState = State.Chase;
                break;
            case State.Chase:
                HandleChase(distance); 
                break;
        }
    }

    private void HandleChase(float distance)
    {
        if (isActionActive) return;

        if (distance <= data.detectionRange && distance > 2f)
        {
            agent.SetDestination(Player.Instance.transform.position);
        }
        else if (distance <= 2f)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        isActionActive = true;
        curState = State.Dash;

        Vector3 targetPos = Player.Instance.transform.position;
        agent.ResetPath();

        yield return new WaitForSeconds(0.2f); 

        agent.speed = data.dashSpeed;
        agent.SetDestination(targetPos);

        yield return new WaitForSeconds(0.5f); 

        curState = State.Cooldown;
        agent.speed = data.normalSpeed;

        yield return new WaitForSeconds(1f); 

        isActionActive = false;
        curState = State.Chase;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(data.enemyDamageAmount, transform);

            if (collision.gameObject.TryGetComponent(out KnockBack kb))
                kb.GetKnockedBack(transform);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (SpiritDIalogManager.Instance != null)
        {
            SpiritDIalogManager.Instance.RegistrKills();
        }

        Destroy(gameObject);
    }
}