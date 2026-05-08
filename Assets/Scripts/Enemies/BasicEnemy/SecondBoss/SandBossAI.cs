using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SandBossAI : MonoBehaviour
{
    public enum BossState { Intro, Idle, Chasing, Jumping, Attacking, Dead }
    public BossState curState = BossState.Intro;

    [Header("Settings")]
    [SerializeField] private EnemySO data;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Animator anim;

    private float currentHealth;
    private bool isIntroDone = false;
    private Coroutine fadeInCoroutine; 

    private void Start()
    {
        if (SaveManager.IsSandBossDefeated())
        {
            Destroy(gameObject);
            return;
        }

        currentHealth = data.enemyHealth;

        if (sr != null)
            sr.color = new Color(1, 1, 1, 0);
    }

    public IEnumerator FadeIn(float duration)
    {
        float t = 0;
        while (t < 1f)
        {
            if (this == null || sr == null) yield break;

            t += Time.deltaTime / duration;
            sr.color = new Color(1, 1, 1, t);
            yield return null;
        }

        if (this != null)
        {
            isIntroDone = true;
            curState = BossState.Idle;
        }
    }

    public void TakeDamage(float damage)
    {
        if (curState == BossState.Dead || !isIntroDone) return;

        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (curState == BossState.Dead) return;

        curState = BossState.Dead;

        SandBossRoomController controller = FindObjectOfType<SandBossRoomController>();
        if (controller != null)
        {
           
            controller.OnBossDefeated();
        }
        else Debug.Log("Controller null!");

        StopAllCoroutines();
        Destroy(gameObject);
    }
}