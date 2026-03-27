using System;
using System.Security.Authentication.ExtendedProtection;
using Unity.VisualScripting;
//using UnityEditor.Experimental.GraphView;
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
        AttackColliderTurnOff();
    }
    public void Attack()
    {
        AttackColliderTurnOffOn(); 
        OnSwordSwing?.Invoke(this, EventArgs.Empty);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BossAI boss = collision.GetComponentInParent<BossAI>();
        if (boss != null)
        {
            boss.TakeDamage(damageAmount);
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
