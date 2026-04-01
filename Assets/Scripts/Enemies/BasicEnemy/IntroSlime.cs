using UnityEngine;
using System.Collections;

public class IntroSlime : MonoBehaviour
{
    public void StartGathering(Vector3 target, float duration)
    {
        StartCoroutine(MoveToCenter(target, duration));
    }

    private IEnumerator MoveToCenter(Vector3 target, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, target, elapsed / duration);
            yield return null;
        }
        Destroy(gameObject); 
    }
}
