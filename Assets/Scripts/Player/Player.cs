using System;
using System.Collections;
using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerCollision), typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public static event Action<int> OnHealthChanged;
    public static event Action<float> OnHealProgressChanged;
    public event EventHandler OnPlayerDash;

    [Header("Core References")]
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public PlayerCollision collision;
    [HideInInspector] public PlayerMovement movement;
    [HideInInspector] public PlayerCombat combat;

    [Header("Health & Respawn")]
    public int maxHealth = 10;
    public int Health { get; private set; }
    [SerializeField] private int lowHealthOnSpawn = 1;
    [SerializeField] private float spikesDamageCooldown = 2f;

    [Header("Hands (Äëÿ PlayerVisual)")]
    public HandAttack leftHand;
    public HandAttack rightHand;

    [Header("Unlockables")]
    public bool isDashUnlocked = false;
    public bool isWallJumpUnlocked = false;
    public bool isSuperDashUnlocked = false;

    private Vector3 lastCheckpointPos;
    private bool canTakeDamage = true;
    private bool isAlive = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        collision = GetComponent<PlayerCollision>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();

        rb.freezeRotation = true;

        Health = maxHealth;
        lastCheckpointPos = transform.position;
    }

    private void Start()
    {
        SaveManager.LoadGame();

        if (!PlayerPrefs.HasKey("PlayerHealth"))
        {
            string lastCheckpoint = SaveManager.GetLastCheckpointID();
            Health = string.IsNullOrEmpty(lastCheckpoint) ? maxHealth : lowHealthOnSpawn;
            OnHealthChanged?.Invoke(Health);
        }

        HandleSceneTransitionSpawn();
    }

    public void InitializeHealth(int savedHealth)
    {
        Health = savedHealth;
        InvokeHealthEvent(Health);
    }

    public void UnlockSuperDash()
    {
        isSuperDashUnlocked = true;
        SaveManager.SaveGame();
    }

    public bool IsAlive() => isAlive;
    public bool IsRunning() => movement.IsRunning;
    public bool isJump() => movement.IsJumping;
    public bool isGrouned() => collision.IsGrounded;

    public void InvokeDashEvent() => OnPlayerDash?.Invoke(this, EventArgs.Empty);
    public void InvokeHealthEvent(int hp) => OnHealthChanged?.Invoke(hp);
    public void InvokeHealProgressEvent(float progress) => OnHealProgressChanged?.Invoke(progress);

    public void TakeDamage(int damageAmount, Transform damageSource)
    {
        if (!canTakeDamage || Health <= 0) return;

        Health -= damageAmount;
        OnHealthChanged?.Invoke(Health);

        if (TryGetComponent(out KnockBack kb))
        {
            kb.GetKnockedBack(damageSource);
        }

        if (Health <= 0) Die();
        else StartCoroutine(DamageCooldownRoutine(spikesDamageCooldown));
    }

    public void Die()
    {
        Health = lowHealthOnSpawn;
        OnHealthChanged?.Invoke(Health);
        transform.position = lastCheckpointPos;
        rb.linearVelocity = Vector2.zero;
    }

    public void UpdateCheckpoint(Vector3 newPos) => lastCheckpointPos = newPos;

    private IEnumerator DamageCooldownRoutine(float cooldown)
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(cooldown);
        canTakeDamage = true;
    }

    public void Heal(int amount)
    {
        Health = Mathf.Min(Health + amount, maxHealth);
        OnHealthChanged?.Invoke(Health); 
    }

    private void HandleSceneTransitionSpawn()
    {
        int currentIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        if (PlayerPositionStorage.TargetSceneIndex == currentIndex)
        {
            SpawnPoint[] allSpawns = FindObjectsOfType<SpawnPoint>();
            foreach (var s in allSpawns)
            {
                if (s.spawnId == PlayerPositionStorage.TargetSpawnId)
                {
                    transform.position = s.transform.position;
                    rb.position = s.transform.position;
                    break;
                }
            }
            PlayerPositionStorage.TargetSceneIndex = -1;
        }
    }
}