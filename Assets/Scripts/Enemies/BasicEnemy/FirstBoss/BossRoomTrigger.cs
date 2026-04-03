using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    [SerializeField] private BossRoomController controller;

    private void OnTriggerEnter2D(Collider2D collision)
    {   Debug.Log("Объект вошел в триггер: " + collision.gameObject.name + " с тегом: " + collision.tag);
        if (collision.CompareTag("Player"))
        {
            controller.StartBossFight();
        }
    } 
}
