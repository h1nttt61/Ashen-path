using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    public int maxHealth = 10;
    private void OnEnable() { Health.OnHealthChanged += UpdateUI; }
    private void OnDisable() { Health.OnHealthChanged -= UpdateUI; }

    void UpdateUI(int currHealth)
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currHealth / maxHealth;
            Debug.Log("UI Updated to: " + currHealth);
        }
    }
}
