using UnityEngine;
using TMPro;
public class NPCDialog : MonoBehaviour
{
    public string[] lines;
    public GameObject dialogPanel;
    public TextMeshProUGUI textDisplay;

    private bool isPlayerNear;
    private int lineIndex;

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
            if (!dialogPanel.activeSelf)
                startDialog();
            else
                nextLine();
    }

    void startDialog()
    {
        lineIndex = 0;
        dialogPanel.SetActive(true);
        textDisplay.text = lines[lineIndex];
    }

    void nextLine()
    {
        if (lineIndex < lines.Length - 1)
        {
            lineIndex++;
            textDisplay.text = lines[lineIndex];
        }
        else
            dialogPanel.SetActive(false);
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
            isPlayerNear = true;
            dialogPanel.SetActive(false);
        }
    }
}
