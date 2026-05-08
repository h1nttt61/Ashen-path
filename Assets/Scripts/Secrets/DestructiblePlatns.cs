using UnityEngine;
using System;

public class DestructibleBlock : MonoBehaviour
{
    public static event Action OnAnyBlockDestroyed;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<HandAttack>())
        {
            Break();
        }
    }

    public void Break()
    {
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.15f, 0.3f); 

        OnAnyBlockDestroyed?.Invoke();
        Destroy(gameObject);
    }
}