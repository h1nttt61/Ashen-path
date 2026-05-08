using System.Collections;
using UnityEngine;

public class SandStormIntro : MonoBehaviour
{
    [SerializeField] private GameObject spherePrefab;

    public IEnumerator PlayVortex(Vector3 center, float duration)
    {
        Vector3[] sphereOffsets = {
            new Vector3(-5, 5), new Vector3(5, 5),
            new Vector3(-5, -5), new Vector3(5, -5)
        };

        GameObject[] spheres = new GameObject[4];
        LineRenderer[] beams = new LineRenderer[4];

        for (int i = 0; i < 4; i++)
        {
            spheres[i] = Instantiate(spherePrefab, center + sphereOffsets[i], Quaternion.identity);

            LineRenderer lr = spheres[i].AddComponent<LineRenderer>();
            beams[i] = lr;

            lr.positionCount = 2;
            lr.startWidth = 0.15f;
            lr.endWidth = 0.02f;

            lr.material = new Material(Shader.Find("Sprites/Default"));

            Color redColor = Color.red;
            lr.startColor = redColor;
            lr.endColor = redColor;

            lr.useWorldSpace = true;
            lr.SetPosition(0, spheres[i].transform.position);
            lr.SetPosition(1, center);

            SetBeamAlpha(lr, 0);
        }

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / (duration * 0.4f);
            foreach (var beam in beams) SetBeamAlpha(beam, t);
            yield return null;
        }

        yield return new WaitForSeconds(duration * 0.6f);

        foreach (var s in spheres) Destroy(s);
    }

    private void SetBeamAlpha(LineRenderer line, float alpha)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.red, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0f), new GradientAlphaKey(alpha, 1f) }
        );
        line.colorGradient = gradient;
    }
}