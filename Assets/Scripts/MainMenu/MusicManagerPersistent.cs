using UnityEngine;
using System.Collections;

public class MusicManagerPersistent : MonoBehaviour
{
    public static MusicManagerPersistent Instance;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponentInChildren<AudioSource>();
        }
        else
        {
            Destroy(gameObject); 
        }
    }
    public void FadeOut(float duration)
    {
        StartCoroutine(FadeRoutine(duration));
    }

    private IEnumerator FadeRoutine(float duration)
    {
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume; 
    }

    public void PlayMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            audioSource.volume = 1f; 
        }
    }
}