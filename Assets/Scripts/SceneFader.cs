using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeInDuration = 2.0f; // Fade in duration
    public float fadeOutDuration = 2.0f; // Fade out duration
    public float stayDuration = 2.0f; // Duration to stay black

    void Start()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // start with transparent black screen
        canvasGroup.alpha = 0f;
    }
    private bool IsFading = false;


    public void TriggerSceneChange()
    {
        if (!IsFading)
        {
            StartCoroutine(BlackoutRoutine());
        }
    }

    
    IEnumerator BlackoutRoutine()
    {
        IsFading = true;

        // Fade to black
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            yield return null; // Wait until the next frame (? idonknow gemini did ts)
        }
        canvasGroup.alpha = 1f; // make sure it is black

        // staying black (should change to next scene i guess)
        yield return new WaitForSeconds(stayDuration);

        // Fade to transparent
        timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            yield return null; // Wait until the next frame (? idonknow gemini did ts)
        }
        canvasGroup.alpha = 0f; // make sure it is transparent

        IsFading = false;
    }
}
