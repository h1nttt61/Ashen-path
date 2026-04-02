using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Collections;
public class NPCDialog : MonoBehaviour
{
    public string[] lines;
    public GameObject dialogPanel;
    public TextMeshProUGUI textDisplay;

    [Header("Reward settings")]
    [SerializeField] public bool giveDashOnEnd = true;
    [SerializeField] private float typingSpeed = 0.04f;
    [SerializeField] public float timeBetweenLines = 2f;
    [SerializeField] public bool giveWallJumpOnEnd = false;

    [Header("Animation")]

    private bool isPlayerNear;
    private bool isTalking = false;
    private int currentLineIndex = 0;
    private Coroutine dialogCoroutine;
    [SerializeField] private string alreadyObtainedText;

    private void Update()
    {
        if (dialogPanel == null || textDisplay == null) return;

        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && !isTalking)
        {
            bool rewardAlreadyObtained = (giveDashOnEnd && Player.Instance.isDashUnlocked) ||
                                         (giveWallJumpOnEnd && Player.Instance.isWallJumpUnlocked);

            if (rewardAlreadyObtained)
            {
                lines = new string[] { alreadyObtainedText };
            }

            if (dialogCoroutine != null) StopCoroutine(dialogCoroutine);
            dialogCoroutine = StartCoroutine(DisplayFullDialog());
        }
    }

    public void StartForcedDialog()
    {
        if (!isTalking)
        {
            StartCoroutine(DisplayFullDialog());
        }
    }

    IEnumerator DisplayFullDialog()
    {
        isTalking = true;
        dialogPanel.SetActive(true);
        for (int i = currentLineIndex; i < lines.Length; i++)
        {
            currentLineIndex = i;
            textDisplay.text = "";

            foreach (var letter in lines[i].ToCharArray())
            {
                textDisplay.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(timeBetweenLines);
        }

        EndDialog();
    }



    void EndDialog()
    {
        
        if (Player.Instance != null)
        {
            if (giveDashOnEnd) Player.Instance.isDashUnlocked = true;
            if (giveWallJumpOnEnd) Player.Instance.isWallJumpUnlocked = true;
            if (NotificationOfSave.Instance != null)
            {
                NotificationOfSave.Instance.Show();
            }
            SaveManager.SaveGame();
        }

        dialogPanel.SetActive(false);
        isTalking = false; 
        currentLineIndex = 0; 
        dialogCoroutine = null;

        SpiritNPC spirit = GetComponent<SpiritNPC>();
        if (spirit != null) spirit.FinalizeSpirit();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            isPlayerNear = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;

            if (isTalking)
            {
                if (dialogCoroutine != null) StopCoroutine(dialogCoroutine);
                isTalking = false; 

                SpiritNPC spirit = GetComponent<SpiritNPC>();
                if (spirit != null)
                {
                    spirit.ResumeChase();
                }
                else
                {
                    if (dialogPanel != null) dialogPanel.SetActive(false);
                    currentLineIndex = 0;
                }
            }
        }
    }
}
