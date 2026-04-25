using UnityEngine;
using TMPro;

public class StatusEffectsUI : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    [SerializeField] private CanvasGroup panelAlphaControl;
    private float effectTimer = 0f;

    private void Awake()
    {
        if (panelAlphaControl == null) panelAlphaControl = GetComponentInParent<CanvasGroup>();
    }

    public void ShowNausea(float duration)
    {
        effectTimer = duration;
        if (statusText != null) statusText.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Player.Instance == null || Player.Instance.movement == null) return;

        float currentCD = Player.Instance.movement.GetSuperDashTimer();

        if (effectTimer > 0 || currentCD > 0)
        {
            if (panelAlphaControl != null) panelAlphaControl.alpha = 1f;

            if (statusText != null)
            {
                statusText.gameObject.SetActive(true);
                string nauseaText = effectTimer > 0 ? $"NAUSEA: {effectTimer:F1}s\n" : "";
                string cdText = currentCD > 0 ? $"SD COOLDOWN: {currentCD:F1}s" : "";
                statusText.text = nauseaText + cdText;
            }

            if (effectTimer > 0) effectTimer -= Time.deltaTime;
        }
        else
        {
            if (statusText != null) statusText.gameObject.SetActive(false);
            if (panelAlphaControl != null) panelAlphaControl.alpha = 0f;
        }
    }
}