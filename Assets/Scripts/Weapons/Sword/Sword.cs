using System;
using System.Security.Authentication.ExtendedProtection;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public class Sword : MonoBehaviour
{
    [SerializeField] private int damageAmount = 2;
    public event EventHandler OnSwordSwing;
    private PolygonCollider2D polygonCollider2D;

    private void Awake()
    {
        polygonCollider2D = GetComponent<PolygonCollider2D>();
    }

    private void Start()
    {
        AttackColliderTurnOffOn();
    }
    public void Attack()
    {
        OnSwordSwing?.Invoke(this, EventArgs.Empty);
        Debug.Log("Атака мечом!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //function to deal damage to the enemy
        //will be added after the first version of the mob
        if (collision.gameObject != Player.Instance?.gameObject) // Игнорируем игрока
        {
            Debug.Log($"Попадание в: {collision.gameObject.name}");

            // Создаем временный визуальный эффект
            StartCoroutine(HitEffect(collision.transform));
        }
    }

    private System.Collections.IEnumerator HitEffect(Transform target)
    {
        if (target.TryGetComponent<SpriteRenderer>(out SpriteRenderer sprite))
        {
            Color originalColor = sprite.color;
            sprite.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            sprite.color = originalColor;
        }
    }

    public void AttackColliderTurnOff()
    {
        polygonCollider2D.enabled = false;
    }

    public void AttackColliderTurnOn()
    {
        polygonCollider2D.enabled = true;
    }

    public void AttackColliderTurnOffOn()
    {
        AttackColliderTurnOff();
        AttackColliderTurnOn();
    }
}
