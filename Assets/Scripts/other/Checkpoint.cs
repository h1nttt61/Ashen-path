using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private CanvasGroup hintCanvasGroup;
    [SerializeField] private GameObject activeVisual;
    [SerializeField] private float fadeSpeed = 2f;

    private bool isPlayerInside = false;
    private bool isActivated = false;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (hintCanvasGroup != null) hintCanvasGroup.alpha = 0;
        if (activeVisual != null) activeVisual.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInside && Input.GetKeyDown(KeyCode.T) && !isActivated)
            ActiveCheck();
    }

    private void ActiveCheck()
    {
        if (Player.Instance != null)
        {
            Player.Instance.UpdateCheckpoint(transform.position);
            isActivated = true;
            SaveManager.SaveGame();
            StartFade(0);

            if (activeVisual != null) activeVisual.SetActive(true);

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
            StartFade(1);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            StartFade(0);
        }
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha));
    }

    private IEnumerator FadeCoroutine(float targe)
    {
        while (!Mathf.Approximately(hintCanvasGroup.alpha, targe))
        {
            hintCanvasGroup.alpha = Mathf.MoveTowards(hintCanvasGroup.alpha, targe, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
