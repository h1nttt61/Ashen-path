using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
   public static CameraShake Instance { get; private set; }

    private Vector3 originalPos3;
    private void Awake() => Instance = this;

    public void Shake(float duration, float magnitude)
    {
        //StopAllCoroutines();
        StartCoroutine(ShakeDuration(duration, magnitude));
    }

    private IEnumerator ShakeDuration(float duration, float magnitude)
    {
        originalPos3 = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos3.x + x, originalPos3.y + y, originalPos3.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos3;
    }
}
