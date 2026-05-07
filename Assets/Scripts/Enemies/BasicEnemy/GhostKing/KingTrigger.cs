using UnityEngine;

public class KingTrigger : MonoBehaviour
{
    [Header("Settings")]
    public BossArenaManager arenaManager; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.gameObject.layer == 10)
        {
            if (arenaManager != null)
            {
                arenaManager.StartIntro();
                Destroy(gameObject);
            }
        }
    }
}