using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private GameObject visual;
    private bool isPlayerInside = false;


    private void Start()
    {
        if (visual != null)
            visual.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInside && Input.GetKeyDown(KeyCode.T))
            ActiveCheck();
    }

    private void ActiveCheck()
    {
        if (Player.Instance != null)
        {
            isPlayerInside = true;
            Player.Instance.UpdateCheckpoint(transform.position);

            Debug.Log("Checkpoint saved");
            visual.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
            if (visual != null)
                visual.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (visual != null)
                visual.SetActive(false);
        }
    }
}
