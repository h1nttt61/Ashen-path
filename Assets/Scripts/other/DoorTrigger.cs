using UnityEngine;
using System.Collections;

public class DoorTrigger : MonoBehaviour
{
    [Header("Door Movement")]
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Vector3 openPosition;   
    [SerializeField] private Vector3 closedPosition; 
    [SerializeField] private float closeSpeed = 5f;

    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeMagnitude = 0.3f;

    private bool isTriggered = false;

    private void Start()
    {
        if (SaveManager.IsDoorClosed())
        {
            doorTransform.position = closedPosition;
            isTriggered = true;
        }
        else
        {
            doorTransform.position = openPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isTriggered && collision.CompareTag("Player"))
        {
            StartCoroutine(CloseDoorSequence());
        }
    }

    private IEnumerator CloseDoorSequence()
    {
        isTriggered = true;
        SaveManager.SaveDoorStatus(true);

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);

        while (Vector3.Distance(doorTransform.position, closedPosition) > 0.01f)
        {
            doorTransform.position = Vector3.MoveTowards(
                doorTransform.position,
                closedPosition,
                closeSpeed * Time.deltaTime
            );
            yield return null;
        }
        doorTransform.position = closedPosition;
    }
}