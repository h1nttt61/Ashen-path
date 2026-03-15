using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Collections;
public class NPCDialog : MonoBehaviour
{
    public string[] lines;
    public GameObject dialogPanel;
    public TextMeshProUGUI textDisplay;
    public float timeBetweenLines = 2f;

    [Header("Reward settings")]
    public bool giveDashOnEnd = true;

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
        for (int i = 0; i < lines.Length; i++)
        {
            textDisplay.text = lines[i];
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
