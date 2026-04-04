using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FireFlicker : MonoBehaviour
{
    private Light2D fireLight;
    [SerializeField] private float minIntensity = 1.0f;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float flickerSpeed = 0.07f; 

    void Awake()
    {
        fireLight = GetComponent<Light2D>();
    }

    void Update()
    {
        if (fireLight == null) return;

        float noise = Mathf.PerlinNoise(Time.time * 5f, 0f);
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        fireLight.intensity = Mathf.MoveTowards(fireLight.intensity, targetIntensity, flickerSpeed);
    }
}