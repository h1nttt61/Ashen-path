using UnityEngine;

[CreateAssetMenu()]
public class EnemySO : ScriptableObject
{
    public string enemyName;
    public int enemyHealth;
    public int enemyDamageAmount;

    [Header("Movement Settings")]
    public float normalSpeed = 3.5f;
    public float dashSpeed = 12f;
    public float detectionRange = 7f;
    public float attackRange = 1.5f;
}
