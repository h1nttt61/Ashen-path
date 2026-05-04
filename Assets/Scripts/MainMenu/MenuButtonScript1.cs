using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuButtonScript : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeScreen;
    [SerializeField] private CanvasGroup buttonsGroup;
    public float fadeSpeed = 1.0f;

    private void Start()
    {
        buttonsGroup = GetComponentInParent<CanvasGroup>();

        if (fadeScreen == null)
        {
            GameObject fadeObj = GameObject.Find("backgroud");

            if (fadeObj != null)
            {
                fadeScreen = fadeObj.GetComponent<CanvasGroup>();
            }
        }

        if (fadeScreen != null)
        {
            fadeScreen.alpha = 0;
            fadeScreen.blocksRaycasts = false;
        }

        if (MusicManagerPersistent.Instance != null)
        {
            MusicManagerPersistent.Instance.PlayMusic();
        }
    }

    public void NewGame()
    {
        SaveManager.ResetProgress();
        if (MusicManagerPersistent.Instance != null)
        {
            MusicManagerPersistent.Instance.FadeOut(1.5f);
        }
        StartCoroutine(LoadWithFade(2));
    }

    public void ContinueGame()
    {
        if (MusicManagerPersistent.Instance != null)
        {
            MusicManagerPersistent.Instance.FadeOut(1.0f);
        }
        int sceneToLoad = SaveManager.GetSavedSceneIndex();
    
        SceneManager.LoadScene(sceneToLoad);
    }

    public void Settings()
    {
        SceneManager.LoadScene(1);
    }

    public void Exit()
    {
        PlayerPrefs.Save();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private IEnumerator LoadWithFade(int sceneIndex)
    {
        if (fadeScreen == null)
        {
            SceneManager.LoadScene(sceneIndex);
            yield break;
        }
        if (buttonsGroup != null) buttonsGroup.interactable = false;
        fadeScreen.blocksRaycasts = true;

        float timer = 0;
        while (timer < fadeSpeed)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / fadeSpeed;

            fadeScreen.alpha = progress;
            if (buttonsGroup != null) buttonsGroup.alpha = 1 - progress;

            yield return null;
        }

        fadeScreen.alpha = 1;

        if (MusicManagerPersistent.Instance != null)
            MusicManagerPersistent.Instance.FadeOut(1.5f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1.0f);

        asyncLoad.allowSceneActivation = true;
    }
}