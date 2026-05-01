using System.Collections;
using UnityEngine;

public class SandStormIntro : MonoBehaviour
{
    [SerializeField] private GameObject sandParticlePrefab;

    public IEnumerator PlayVortex(Vector3 center, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            for (int i = 0; i < 3; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2);
                float radius = 5f;
                Vector3 spawnPos = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;

                GameObject p = Instantiate(sandParticlePrefab, spawnPos, Quaternion.identity);
                StartCoroutine(MoveInVortex(p, center, 0.7f));
            }
            yield return null;
        }
    }

    private IEnumerator MoveInVortex(GameObject p, Vector3 center, float duration)
    {
        float elapsed = 0;
        Vector3 startPos = p.transform.position;
        float startAngle = Mathf.Atan2(startPos.y - center.y, startPos.x - center.x);

        while (elapsed < duration)
        {
            if (p == null) yield break;
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float currentRadius = Mathf.Lerp(5f, 0f, progress);
            float currentAngle = startAngle + (progress * 10f);

            p.transform.position = center + new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0) * currentRadius;
            p.transform.localScale = Vector3.Lerp(Vector3.one * 0.2f, Vector3.zero, progress);
            yield return null;
        }
        Destroy(p);
    }
}