using UnityEngine;
using TMPro;
using System.Collections;

public class FireAutoDialogue : MonoBehaviour
{
    [System.Serializable]
    public class DialogueSequence
    {
        public string sequenceName;
        [TextArea(3, 10)] public string[] pages;
    }

    [Header("NPC Settings")]
    [SerializeField] private string npcName = "Cinder Elder";
    [SerializeField] private float interactionRange = 4f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("UI Components")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI textDisplay;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float delayBetweenPages = 2f;

    [Header("Dialogues")]
    [SerializeField] private DialogueSequence firstMeeting;
    [SerializeField] private DialogueSequence[] randomStories;

    [Header("Visuals")]
    [SerializeField] private Animator animator;

    private bool isPlayerInsideTrigger;
    private bool isTalking;

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (dialogueCanvas != null) dialogueCanvas.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInsideTrigger && Input.GetKeyDown(interactKey) && !isTalking)
        {
            if (Player.Instance != null && Player.Instance.IsAlive()) 
            {
                StartCoroutine(PlayFullDialogue());
            }
        }
    }
    private IEnumerator PlayFullDialogue()
    {
        if (dialogueCanvas == null || textDisplay == null)
        {
            isTalking = false;
            yield break;
        }

        isTalking = true;
        dialogueCanvas.SetActive(true);

        if (animator != null)
        {
            animator.SetTrigger("Talk");
        }
        DialogueSequence currentSequence = SaveManager.IsRingGiven()
            ? randomStories[Random.Range(0, randomStories.Length)]
            : firstMeeting;
        if (currentSequence == null || currentSequence.pages == null || currentSequence.pages.Length == 0)
        {
            EndDialogue();
            yield break;
        }

        foreach (string page in currentSequence.pages)
        {
            textDisplay.text = "";
            foreach (char letter in page.ToCharArray())
            {
                textDisplay.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(delayBetweenPages);
        }

        if (!SaveManager.IsRingGiven())
        {
            SaveManager.SaveRingStatus(true);
        }

        EndDialogue();
    }
    private void EndDialogue()
    {
        if (dialogueCanvas != null) dialogueCanvas.SetActive(false);
        isTalking = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<Player>() != null) 
        {
            isPlayerInsideTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<Player>() != null) 
        {
            isPlayerInsideTrigger = false;

            if (isTalking)
            {
                StopAllCoroutines();
                EndDialogue();
            }
        }
    }
}