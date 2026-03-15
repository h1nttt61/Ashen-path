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

    

    private bool isPlayerNear;
    private bool isTalking = false;

    private void Update()
    {
        if (dialogPanel == null || textDisplay == null) return;
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && !isTalking)
            StartCoroutine(DisplayFullDialog());
    }

    IEnumerator DisplayFullDialog()
    {
        isTalking = true;
        dialogPanel.SetActive(true);
        
        foreach (var line in lines)
        {
            textDisplay.text = "";

            foreach (var letter in line.ToCharArray())
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
        if (giveDashOnEnd && Player.Instance != null)
        {
            Player.Instance.isDashUnlocked = true;
        }

        dialogPanel.SetActive(false);
        isTalking = false;
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
            StopAllCoroutines();
            if (dialogPanel != null) dialogPanel.SetActive(false);
            isTalking = false;
        }
    }
}
