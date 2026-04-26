using TMPro;
using UnityEngine;

public class StatusEffectsUI : MonoBehaviour
{
    public static StatusEffectsUI Instance { get; private set; } 

    public TextMeshProUGUI statusText;
    [SerializeField] private CanvasGroup panelAlphaControl;
    private float effectTimer = 0f;

    private void Awake()
    {
        Instance = this; 
        if (panelAlphaControl == null) panelAlphaControl = GetComponent<CanvasGroup>();
    }

    public void ShowNausea(float duration) => effectTimer = duration;

    private void Update()
    {
        if (Player.Instance == null) return;

        float currentCD = Player.Instance.movement.GetSuperDashTimer();

        if (effectTimer > 0 || currentCD > 0)
        {
            if (panelAlphaControl != null) panelAlphaControl.alpha = 1f;
            if (statusText != null)
            {
                statusText.gameObject.SetActive(true);
                string nText = effectTimer > 0 ? $"NAUSEA: {effectTimer:F1}s\n" : "";
                string cText = currentCD > 0 ? $"SD COOLDOWN: {currentCD:F1}s" : "";
                statusText.text = nText + cText;
            }
            if (effectTimer > 0)
            {
                statusText.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 2, 1));
                effectTimer -= Time.deltaTime;
            }
            else
            {
                statusText.color = Color.white;
            }
        }
        else
        {
            if (panelAlphaControl != null) panelAlphaControl.alpha = 0f;
            if (statusText != null) statusText.gameObject.SetActive(false);
        }
    }
}