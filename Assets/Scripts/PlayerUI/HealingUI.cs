using UnityEngine;
using UnityEngine.UI;

public class HealingUI : MonoBehaviour
{
    public Image healthBar; 
    public Image regenBar;  

    private void OnEnable()
    {
        Player.OnHealthChanged += UpdateHealthBar;
        Player.OnHealProgressChanged += UpdateRegenBar;
    }

    private void OnDisable()
    {
        Player.OnHealthChanged -= UpdateHealthBar;
        Player.OnHealProgressChanged -= UpdateRegenBar;
    }

    private void UpdateHealthBar(int hp)
    {
        if (healthBar) healthBar.fillAmount = (float)hp / Player.Instance.maxHealth;
    }

    private void UpdateRegenBar(float progress)
    {
        if (regenBar) regenBar.fillAmount = progress;
    }
}