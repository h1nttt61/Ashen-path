using UnityEngine;

public class Health : MonoBehaviour
{
    public RectTransform healthBar;
    private float initialWidth;

    private void Start()
    {
        initialWidth = healthBar.sizeDelta.x;
    }

    private void Update()
    {
        float newWidth = initialWidth * (float)Player.Instance.Health / Player.Instance.maxHealth;
        healthBar.sizeDelta = new Vector2(newWidth, healthBar.sizeDelta.y);
    }
}
