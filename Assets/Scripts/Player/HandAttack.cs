using UnityEngine;

public class HandAttack : MonoBehaviour
{
    [SerializeField] private int damage = 2;
    [SerializeField] private float knockbackForce = 5f; 
    private PolygonCollider2D polyCollider;

    private void Awake()
    {
        polyCollider = GetComponent<PolygonCollider2D>();
        polyCollider.enabled = false;
    }

    public void EnableAttack() => polyCollider.enabled = true;
    public void DisableAttack() => polyCollider.enabled = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out SlimeAI slime)) slime.TakeDamage(damage);

        if (collision.TryGetComponent(out BossAI boss)) boss.TakeDamage(damage);

        if (collision.TryGetComponent(out BatAI bat)) bat.TakeDamage(damage);

        if (collision.TryGetComponent(out KnockBack kb))
        {
            kb.GetKnockedBack(transform);
        }
    }
}