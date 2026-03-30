using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string checkpointID; 
    [SerializeField] private Animator animator;
    [SerializeField] private CanvasGroup hintCanvasGroup;
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
            if (isSaved)
            {
                isActivated = true;
                animator.SetBool("isLit", true);
                animator.Play("Burning"); 
            }
            else
            {
                animator.SetBool("isLit", false);
                animator.Play("idle_off");
            }
        }
    }

    private void Update()
    {
        if (isPlayerInside && Input.GetKeyDown(KeyCode.T))
            ActiveCheck();
    }

    private void ActiveCheck()
    {
        if (Player.Instance != null)
        {
            Player.Instance.UpdateCheckpoint(transform.position);

            SaveManager.SaveCurrentCheckpoint(checkpointID);
            SaveManager.SaveGame();
            if (NotificationOfSave.Instance != null)
            {
                NotificationOfSave.Instance.Show();
            }
            isActivated = true;
            StartFade(0);

            Checkpoint[] allCheckpoints = FindObjectsOfType<Checkpoint>();
            foreach (Checkpoint cp in allCheckpoints)
            {
                cp.UpdateVisualState(); 
            }
        }
    }

    public void UpdateVisualState()
    {
        bool isSaved = SaveManager.GetLastCheckpointID() == checkpointID;
        isActivated = isSaved;

        if (animator != null)
        {
            animator.SetBool("isLit", isSaved);

            if (isSaved)
            {
                animator.Play("Burning", 0, 0f);
            }
            else
            {
                animator.Play("idle_off", 0, 0f);
            }
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
