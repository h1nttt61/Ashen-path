using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class BackgroundPersistent : MonoBehaviour
{
    public static BackgroundPersistent Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 2)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetAlpha(float alpha)
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group != null) group.alpha = alpha;
    }
}