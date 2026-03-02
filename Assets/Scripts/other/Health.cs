using System;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health settings")]
    [SerializeField] public int maxHealth = 10;
    public int currentHealth;

    [Header("Respawn settings")]
    private Vector3 lastCheckpointPos;
    private Rigidbody2D rb;
    private Player player;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        currentHealth = maxHealth;

        lastCheckpointPos = rb.position;
    }

    public static event Action<int> OnHealthChanged;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"HP: {currentHealth}");
        
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
            Die();

    }

    private void Die()
    {
        Debug.Log("Player died!");
        Respawn();
    }

    public void Respawn()
    {
        currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth);

        transform.position = lastCheckpointPos;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;  
    }

    public void UpdateCheckpoint(Vector3 newPos)
    {
        lastCheckpointPos = newPos;
        Debug.Log("Checkpoint Update");
    }
}
