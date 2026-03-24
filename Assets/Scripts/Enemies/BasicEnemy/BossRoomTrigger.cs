using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    [SerializeField] private BossRoomController controller;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            controller.StartBossFight();
        }
    }
}
