using UnityEngine;
using System.Collections;
using TMPro;

public class SpiritNPCTwo : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private Vector3 spawnOffset = new Vector3(-3f, 1.5f, 0f);
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("UI References")]
    [SerializeField] private GameObject dialogCanvas;
    [SerializeField] private TextMeshProUGUI dialogText;

    private SpriteRenderer sr;
    private string[] currentPhrases;
    private bool canContinue = false;
    private bool isProcessing = false;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (dialogCanvas != null) dialogCanvas.SetActive(false);
    }

    public void ActivateSpirit(string[] phrases)
    {
        if (isProcessing) return;

        isProcessing = true;
        currentPhrases = phrases;
        gameObject.SetActive(true);
        transform.position = Player.Instance.transform.position + spawnOffset;
        StartCoroutine(SpiritSequence());
    }
    private IEnumerator SpiritSequence()
    {
        yield return StartCoroutine(Fade(0, 1));

        SetPlayerLock(true);

        if (dialogCanvas != null && dialogText != null)
        {
            dialogCanvas.SetActive(true);
            foreach (string line in currentPhrases)
            {
                yield return StartCoroutine(TypeText(line));
                yield return new WaitForSeconds(2f);
            }
            dialogCanvas.SetActive(false);
        }

        LockProgress();
        SetPlayerLock(false);

        yield return StartCoroutine(Fade(1, 0));

        isProcessing = false;
        gameObject.SetActive(false);
    }
    private IEnumerator TypeText(string line)
    {
        dialogText.text = string.Empty;
        yield return null;
        foreach (char letter in line.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void SetPlayerLock(bool locked)
    {
        if (Player.Instance == null) return;

        Player.Instance.movement.enabled = !locked;
        Player.Instance.combat.enabled = !locked;

        if (locked)
        {
            Player.Instance.rb.linearVelocity = Vector2.zero;
            Player.Instance.rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
            Player.Instance.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void LockProgress()
    {
        SaveManager.SaveSpiritEvent();
        SaveManager.SaveDoorStatus(true);
        SaveManager.SaveGame();

        if (NotificationOfSave.Instance != null)
            NotificationOfSave.Instance.Show();
    }

    public IEnumerator Fade(float start, float end)
    {
        if (sr == null) yield break;
        Color c = sr.color;
        float elapsed = 0;
        float duration = 1f / fadeSpeed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(start, end, elapsed / duration);
            sr.color = c;
            yield return null;
        }
    }
}