using System.Collections;
using UnityEngine;

public class SpikesTrigger : MonoBehaviour
{
    [SerializeField] private int damageAmount = 2;

    private void OCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // do some damage
            StartCoroutine(SpikeDamage());
        }
    }

    IEnumerator SpikeDamage()
    {
        Player.Instance.maxHealth -= damageAmount;
        UnityEngine.Debug.Log("Damage");
        yield return new WaitForSeconds(2);
    }
}
