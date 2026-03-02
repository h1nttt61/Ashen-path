using UnityEngine;

public class Spikes : MonoBehaviour
{
    private int damageAmount = 1;
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(damageAmount, transform);
        }
    }
}