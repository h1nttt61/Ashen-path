using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class AsyncSceneLoad : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup fadeGroup;

    [Header("NPC & Walls")]
    public GameObject npc;

    private bool isLoading = false;

    public string sceneToLoad = "2.1";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isLoading)
        {
            StartCoroutine(LoadSceneRoutine());
        }
    }

    IEnumerator LoadSceneRoutine()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("Забыл написать имя сцены в инспекторе!");
            yield break;
        }

        isLoading = true;

        SaveManager.ClearCheckpointData();

        float timer = 0;
        while (timer < 1f)
        {
            timer += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(0, 1, timer);
            yield return null;
        }

        if (Player.Instance != null)
        {
            Player.Instance.rb.linearVelocity = Vector2.zero;
            Player.Instance.movement.enabled = false;
            Player.Instance.GetComponentInChildren<Animator>().speed = 0;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        if (operation == null) yield break;

        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        operation.allowSceneActivation = true;
    }
}