using Unity.VisualScripting;
using UnityEngine;

public class InstaKill : MonoBehaviour
{
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player.Instance.Die();
        }
    }
}
