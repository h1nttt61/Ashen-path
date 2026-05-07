using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;

public class FinaleManager : MonoBehaviour
{
    public CanvasGroup blackScreen;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI storyText;
    public GameObject playerUI;
    [Header("Final Story")]
    public string finalTitle = "ФИНАЛ";
    [TextArea(5, 10)] 
    public string finalStory = "Ваш текст истории здесь...";

    public void StartFinale()
    {
        StartCoroutine(FinaleSequence());
    }

    IEnumerator FinaleSequence()
    {
        if (playerUI != null) playerUI.SetActive(false);
        yield return new WaitForSeconds(5f);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.5f;
            blackScreen.alpha = t;
            yield return null;
        }

        titleText.text = finalTitle;
        titleText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);

        storyText.gameObject.SetActive(true);
        storyText.text = "";

        foreach (char c in finalStory)
        {
            storyText.text += c;
            yield return new WaitForSeconds(0.04f); 
        }

        yield return new WaitForSeconds(5f);

        SaveManager.SaveGameCompleted(true);

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}