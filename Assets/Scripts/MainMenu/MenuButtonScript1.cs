using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuButtonScript : MonoBehaviour
{

    private void Start()
    {
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
        StartCoroutine(LoadWithDelay(2, 1.5f));
    }

    public void ContinueGame()
    {
        if (MusicManagerPersistent.Instance != null)
        {
            MusicManagerPersistent.Instance.FadeOut(1.0f);
        }
        SceneManager.LoadScene(2);
    }

    public void Settings()
    {
        SceneManager.LoadScene(1);
    }

    public void Exit()
    {
        Application.Quit();
    }

    private IEnumerator LoadWithDelay(int sceneIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneIndex);
    }
}