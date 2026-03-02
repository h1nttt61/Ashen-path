using UnityEngine;
using System;

public class DestructiblePlatns : MonoBehaviour
{
    public event EventHandler OnDestructiblePlatns;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Sword>())
        {
            OnDestructiblePlatns?.Invoke(this, EventArgs.Empty);
            Destroy(collision.gameObject);
            //if we used navMesh - NavMeshSurfaceManagment.Instance.RebakeNavMeshSurface();
        }
    }
}
