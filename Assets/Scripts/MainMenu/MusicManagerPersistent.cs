using UnityEngine;
using System.Collections;

public class MusicManagerPersistent : MonoBehaviour
{
    public static MusicManagerPersistent Instance;
    private AudioSource audioSource;

    [Header("Music Settings")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameMusic;

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

    public void SwitchToGameMusic(float duration)
    {
        StartCoroutine(SwitchTrackRoutine(gameMusic, duration));
    }

    private IEnumerator SwitchTrackRoutine(AudioClip newClip, float duration)
    {
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / (duration / 2);
            yield return null;
        }
        audioSource.Stop();

        audioSource.clip = newClip;
        audioSource.Play();

        while (audioSource.volume < startVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / (duration / 2);
            yield return null;
        }
        audioSource.volume = startVolume;
    }

    public void PlayMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            if (audioSource.clip == null) audioSource.clip = mainMenuMusic;
            audioSource.Play();
            audioSource.volume = 1f;
        }
    }
}