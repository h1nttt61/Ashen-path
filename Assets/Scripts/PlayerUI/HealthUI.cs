using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Image healthBar;
    private int maxHealth;
    private void OnEnable() { Player.OnHealthChanged += UpdateUI; }
    private void OnDisable() { Player.OnHealthChanged -= UpdateUI; }
    private void Start()
    {
        maxHealth = Player.Instance.maxHealth;
    }
    private void UpdateUI(int currentHealth)
    {
        float fill = (float)currentHealth / maxHealth;
        healthBar.fillAmount = fill;
    }
}
