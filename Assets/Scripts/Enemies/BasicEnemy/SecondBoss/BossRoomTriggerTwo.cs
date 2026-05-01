using UnityEngine;

public class BossRoomTriggerTwo : MonoBehaviour
{
    [SerializeField] private SandBossRoomController controller;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Объект вошел в триггер: " + collision.gameObject.name + " с тегом: " + collision.tag);
        if (collision.CompareTag("Player"))
        {
            controller.StartBossFight();
        }
    }
}
