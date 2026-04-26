using UnityEngine;

public class Spikes : MonoBehaviour
{
    private int damageAmount = 1;
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(damageAmount, transform);
        }
    }
}