using UnityEngine;

public class NauseaEffect : MonoBehaviour
{
    public static NauseaEffect Instance;

    [SerializeField] private float intensity = 2f; 
    [SerializeField] private float speed = 3f;    
    private float currentDuration = 0f;
    private Quaternion originalRotation;

    private void Awake() => Instance = this;

    private void Start() => originalRotation = transform.localRotation;

    public void StartNausea(float duration) => currentDuration = duration;

    private void Update()
    {
        if (currentDuration > 0)
        {
            currentDuration -= Time.deltaTime;

            float tilt = Mathf.Sin(Time.time * speed) * intensity;
            transform.localRotation = originalRotation * Quaternion.Euler(0, 0, tilt);
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, Time.deltaTime * speed);
        }
    }
}