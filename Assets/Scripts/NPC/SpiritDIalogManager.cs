using System.Collections;
using UnityEngine;

public class SpiritDIalogManager : MonoBehaviour
{
    public static SpiritDIalogManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int killReq = 2;
    [SerializeField] private float fadeDuration = 2f;

    [Header("References")]
    [SerializeField] private SpiritNPC spirit;
    [SerializeField] private CanvasGroup blackScreen;

    private int killCount = 0;
    private bool eventTrigger = false;

    private void Awake()
    {
        Instance = this;
        if (SaveManager.IsSpiritEventTriggered())
        {
            eventTrigger = true;
            if (spirit != null) spirit.gameObject.SetActive(false);
        }
        else
        {
            if (spirit != null) spirit.gameObject.SetActive(false);
        }
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

        SaveManager.SaveSpiritEvent();

        ClearNearbyEnemies();

        if (Player.Instance != null)
        {
            SetPlayerControl(false);

            Rigidbody2D rb = Player.Instance.GetComponent<Rigidbody2D>();
            Animator playerAnim = Player.Instance.GetComponentInChildren<Animator>();

            float timeout = 2f;
            while (Mathf.Abs(rb.linearVelocity.y) > 0.1f && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;

            if (playerAnim != null)
            {
                playerAnim.SetBool("isRunning", false);
                playerAnim.SetBool("isGd", true);
            }

            float side = Player.Instance.transform.localScale.x > 0 ? -3f : 3f;
            spirit.transform.position = Player.Instance.transform.position + new Vector3(side, 1.5f, 0);
        }

        spirit.gameObject.SetActive(true);
        yield return StartCoroutine(FadeSpiritIn());

        var dialog = spirit.GetComponent<NPCDialog>();
        if (dialog != null)
            dialog.StartForcedDialog();
    }

    private IEnumerator FadeSpiritIn()
    {
        SpriteRenderer spiritRenderer = spirit.GetComponentInChildren<SpriteRenderer>();
        if (spiritRenderer == null) yield break;

        Color c = spiritRenderer.color;
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            spiritRenderer.color = c;
            yield return null;
        }
    }

    private void ClearNearbyEnemies()
    {
        float clearRadius = 15f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(Player.Instance.transform.position, clearRadius);
        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out BatAI bat)) Destroy(bat.gameObject);
        }

        foreach (var s in FindObjectsOfType<SlimeSpawner>())
        {
            s.DeactivateSpawner(120f);
        }
    }

    private void SetPlayerControl(bool state)
    {
        if (Player.Instance.TryGetComponent(out PlayerMovement mov)) mov.enabled = state;
        if (Player.Instance.TryGetComponent(out PlayerCombat comb)) comb.enabled = state;
    }

    public void UnfreezePlayer()
    {
        if (Player.Instance != null)
        {
            Rigidbody2D rb = Player.Instance.GetComponent<Rigidbody2D>();

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;

            SetPlayerControl(true);
        }
    }
}