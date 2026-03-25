using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    private bool isPause = false;


    private void Start()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (isPause)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pausePanel?.SetActive(false);
        Time.timeScale = 1f;
        isPause = false;
    }

    public void Pause()
    {
        pausePanel?.SetActive(true);
        Time.timeScale = 0f;
        isPause = true;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
