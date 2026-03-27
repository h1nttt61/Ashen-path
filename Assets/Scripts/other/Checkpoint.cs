using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string checkpointID;
    [SerializeField] private Animator animator;
    [SerializeField] private CanvasGroup hintCanvasGroup;
    [SerializeField] private GameObject activeVisual;
    [SerializeField] private float fadeSpeed = 2f;

    private bool isPlayerInside = false;
    private bool isActivated = false;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (hintCanvasGroup != null) hintCanvasGroup.alpha = 0;

        bool isSaved = PlayerPrefs.GetString("LastCheckpointID") == checkpointID;

        if (animator != null)
        {
            if (isSaved) animator.Play("Burning");
            else animator.Play("Idle_Off");
        }
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
            PlayerPrefs.SetString("LastCheckpointID", checkpointID);
            SaveManager.SaveGame(); // Сохраняем прогресс

            isActivated = true;

            StartFade(0);

            if (animator != null)
            {
                animator.SetTrigger("IgniteTrigger");
            }

            foreach (Checkpoint cp in FindObjectsOfType<Checkpoint>())
            {
                if (cp != this) cp.Extinguish();
            }
        }
    }

    public void Extinguish()
    {
        if (animator != null) animator.SetBool("isLit", false);
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
        if (gameObject.activeInHierarchy)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha));
        }
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
