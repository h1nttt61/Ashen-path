using UnityEngine;

public class SecondLocSpiritTrigger : MonoBehaviour
{
    [SerializeField] private SpiritNPCTwo spirit;
    [SerializeField] private string[] phrases;

    private void Start()
    {
        if (SaveManager.IsSpiritEventTriggered() || SaveManager.IsDoorClosed())
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && spirit != null)
        {
            spirit.ActivateSpirit(phrases);
            gameObject.SetActive(false); 
        }
    }
}