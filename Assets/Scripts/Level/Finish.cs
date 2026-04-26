using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private int targetSpawnId;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            PlayerPositionStorage.TargetSceneIndex = nextSceneIndex;
            PlayerPositionStorage.TargetSpawnId = targetSpawnId;

            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("Это последняя сцена!");
            SceneManager.LoadScene(0);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log($"Объект в триггере: {collision.gameObject.name}, тег: {collision.tag}");
    }
}