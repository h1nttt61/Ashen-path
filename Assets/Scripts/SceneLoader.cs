using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System;

public class SceneLoader : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeInDuration = 2.0f; // Fade in duration
    public float fadeOutDuration = 2.0f; // Fade out duration
    public float stayDuration = 1.0f; // Duration to stay black
    public string sceneToLoad = "Scene2";
    public GameObject player;
    private bool IsFading = false;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        DontDestroyOnLoad(gameObject);
        canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

     private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string StartGeneralName = "Start";
        player = GameObject.FindWithTag("Player");

        if (scene.name == "Scene2")
        {
            GameObject teleportPoint = GameObject.Find("Scene2" + StartGeneralName);
            TeleportPlayerToPoint(teleportPoint);

        }
        
        if (IsFading)
        {
            StartCoroutine(FadeOutRoutine());
        }
    }

    private void TeleportPlayerToPoint(GameObject tpPoint)
    {
        if (player != null && tpPoint != null)
        {
            player.transform.position = tpPoint.transform.position;
        }
    }

    public void TriggerSceneChangeEnter()
    {
        if (!IsFading)
        {
            StartCoroutine(FadeInRoutine());
        }
    }


    IEnumerator FadeInRoutine()
    {
        IsFading = true;
        // Fade to black (also fade player's speed)
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            //player.speed = Mathf.Lerp(currSpeed, 0f, timer / fadeInDuration); should speed change here?
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(stayDuration);
        SceneManager.LoadScene(sceneToLoad);
    }

    IEnumerator FadeOutRoutine()
    {
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(stayDuration);

        // Fade to transparent
        float timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        //player.speed = currSpeed;
        IsFading = false;
    }
}
