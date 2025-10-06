using UnityEngine;
using System;
using System.Security.Authentication.ExtendedProtection;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //function to deal damage to the enemy
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
