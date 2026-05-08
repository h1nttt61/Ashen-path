using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            Player playerScript = col.GetComponent<Player>();

            if (playerScript != null)
            {
                playerScript.TakeDamage(1, transform);
            }
         
            Destroy(gameObject);
            return; 
        }

        if (col.gameObject.layer == 3 || col.gameObject.layer == 6)
        {
            Destroy(gameObject);
        }
    }
}