using System.Collections;
using UnityEngine;

public class SpikesTrigger : MonoBehaviour
{
    [SerializeField] private int damageAmount = 2;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Health playerHealth = collision.gameObject.GetComponent<Health>();
        if (collision.gameObject.CompareTag("Player"))
        {
            // do some damage
            StartCoroutine(SpikeDamage(playerHealth));
        }
    }

    IEnumerator SpikeDamage(Health targetHealth)
    {
        targetHealth.TakeDamage(damageAmount);
        yield return new WaitForSeconds(2);
    }
}