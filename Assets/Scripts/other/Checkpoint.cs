using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private GameObject visual;
    private bool isPlayerInside = false;
    private Health health;

    private void Start()
    {
        if (visual != null)
            visual.SetActive(false);
    }

    private void pdate()
    {
        if (isPlayerInside && Input.GetKeyDown(KeyCode.T))
            ActiveCheck();
    }

    private void ActiveCheck()
    {
        if (health != null)
        {
            isPlayerInside = true;
            health.UpdateCheckpoint(transform.position);

            Debug.Log("Checkpoint saved");
            visual.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
            health = collision.GetComponent<Health>();
            if (visual != null)
                visual.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            health = collision.GetComponent<Health>();
            if (visual != null)
                visual.SetActive(false);
        }
    }
}
