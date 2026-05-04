using System.Net.NetworkInformation;
using UnityEngine;
using System.Collections;

public class SpiritNPCTwo : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private Vector3 spawnOffset = new Vector3(-3f, 1.5f, 0f);

    [Header("References")]
    private SpriteRenderer sr;
    private NPCDialog dialog;
    private Rigidbody2D rb;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        dialog = GetComponent<NPCDialog>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void ActivateSpirit(string[] phrases)
    {
        gameObject.SetActive(true);
        transform.position = Player.Instance.transform.position + spawnOffset;

        if (dialog != null)
        {
            dialog.lines = phrases;
            StartCoroutine(SpiritSequence());
        }
    }

    private IEnumerator SpiritSequence()
    {
        yield return StartCoroutine(Fade(0, 1));

        SetPlayerLock(true);

        yield return StartCoroutine(dialog.DisplayFullDialog());

        LockProgress();

        SetPlayerLock(false);

        yield return StartCoroutine(Fade(1, 0));

        gameObject.SetActive(false);
    }

    private void LockProgress()
    {
        SaveManager.SaveSpiritEvent();

        SaveManager.SaveDoorStatus(true);

        SaveManager.SaveGame();

        if (NotificationOfSave.Instance != null)
            NotificationOfSave.Instance.Show(); 
    }

    private void SetPlayerLock(bool locked)
    {
        if (Player.Instance == null) return;

        Player.Instance.movement.enabled = !locked;
        Player.Instance.combat.enabled = !locked;

        if (locked)
        {
            Player.Instance.rb.linearVelocity = Vector2.zero;
            Player.Instance.rb.bodyType = RigidbodyType2D.Static;
        }
        else
        {
            Player.Instance.rb.bodyType = RigidbodyType2D.Dynamic;
        }
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

    private void Update()
    {
        if (Player.Instance != null && sr != null)
        {
            float dir = Player.Instance.transform.position.x - transform.position.x;
            sr.flipX = dir < 0;
        }
    }
}
