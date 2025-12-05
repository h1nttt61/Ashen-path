using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Metroidvania/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Base Stats")]
    public string enemyName = "Enemy";
    public int maxHealth = 10;
    public int contactDamage = 1;
    public float damageCooldown = 0.5f;
    
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float patrolDistance = 5f;
    public bool canPatrol = true;
    public bool hasIdleState = true;
    public float idleTime = 1f;
    
    [Header("Visual")]
    public Sprite defaultSprite;
    public RuntimeAnimatorController animatorController;
    public Color spriteColor = Color.white;
    
    [Header("Drops")]
    public GameObject[] dropPrefabs;
    public float dropChance = 0.3f;
    public int soulReward = 5;
    
    [Header("AI")]
    public bool useSimpleAI = true;
    public bool useNavigationAI = false;
    public float detectionRange = 0f; 
    
    [Header("Audio")]
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip attackSound;
}