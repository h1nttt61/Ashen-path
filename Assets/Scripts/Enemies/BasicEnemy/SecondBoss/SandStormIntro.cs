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
            for (int i = 0; i < 3; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2);
                Vector3 spawnPos = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 5f;
                GameObject p = Instantiate(sandParticlePrefab, spawnPos, Quaternion.identity);
                StartCoroutine(MoveToCenter(p, center, 0.6f));
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator MoveToCenter(GameObject p, Vector3 center, float time)
    {
        float t = 0;
        Vector3 start = p.transform.position;
        while (t < 1f)
        {
            if (p == null) yield break;
            t += Time.deltaTime / time;
            p.transform.position = Vector3.Lerp(start, center, t);
            p.transform.localScale = Vector3.Lerp(Vector3.one * 0.2f, Vector3.zero, t);
            yield return null;
        }
        Destroy(p);
    }
}