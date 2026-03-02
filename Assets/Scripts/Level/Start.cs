using UnityEngine;
using UnityEngine.SceneManagement;

public class Start : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private int targetSpawnId;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            LoadPrevScene();
        }
    }

    private void LoadPrevScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int prevSceneIndex = currentSceneIndex - 1;

        if (prevSceneIndex >= 0 && prevSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            PlayerPositionStorage.TargetSceneIndex = prevSceneIndex;
            PlayerPositionStorage.TargetSpawnId = targetSpawnId;

            SceneManager.LoadScene(prevSceneIndex);
        }
        else
        {
            Debug.Log("Это первая сцена!");
            SceneManager.LoadScene(0);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log($"Объект в триггере: {collision.gameObject.name}, тег: {collision.tag}");
    }
}