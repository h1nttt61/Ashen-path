using System.Collections;
using UnityEngine;

public class SpiritDIalogManager : MonoBehaviour
{
    public static SpiritDIalogManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int killReq = 5;
    [SerializeField] private float fadeDuration = 2f;

    [Header("Referens")]
    [SerializeField] private SpiritNPC spirit;
    [SerializeField] private CanvasGroup blackSreen;

    private int killCount = 0;
    private bool eventTrigger = false;

    private void Awake()
    {
        Instance = this;
        if (spirit != null)
            spirit.gameObject.SetActive(false);
    }

    public void RegistrKills()
    {
        if (eventTrigger) return;

        killCount++;
        if (killCount >= killReq)
        {
            StartCoroutine(TriggerSpiritEvent());
        }
    }

    private IEnumerator TriggerSpiritEvent()
    {
        eventTrigger = true;
        SlimeSpawner[] spawners = FindObjectsOfType<SlimeSpawner>();
        foreach (var s in spawners)
        {
            s.DeactivateSpawner(120f); 
        }
        if (Player.Instance != null)
        {
            Player.Instance.enabled = false;
            var rb = Player.Instance.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            float side = Player.Instance.transform.localScale.x > 0 ? -4f : 4f;
            spirit.transform.position = Player.Instance.transform.position + new Vector3(side, 2f, 0);
        }

        spirit.gameObject.SetActive(true);

        SpriteRenderer spiritRenderer = spirit.GetComponentInChildren<SpriteRenderer>();

        if (spiritRenderer != null)
        {
            Color c = spiritRenderer.color;
            c.a = 0;
            spiritRenderer.color = c;

            float elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(0, 1, elapsed / fadeDuration);
                spiritRenderer.color = c;
                yield return null;
            }
        }

        var dialog = spirit.GetComponent<NPCDialog>();
        if (dialog != null)
            dialog.StartForcedDialog();
    }

    public void UnfreezePlayer()
    {
        if (Player.Instance != null)
            Player.Instance.enabled = true;
    }
}
