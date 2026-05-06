using System;
using System.Collections;
using Unity.VisualScripting;
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

    [Header("Hands (PlayerVisual)")]
    public HandAttack leftHand;
    public HandAttack rightHand;

    [Header("Unlockables")]
    public bool isDashUnlocked = false;
    public bool isWallJumpUnlocked = false;
    public bool isSuperDashUnlocked = false;

    [Header("Gorgon Effect")]
    private SpriteRenderer playerSprite;
    private bool isPetrified = false;
    public bool IsPetrified => isPetrified;
    private Color originalPlayerColor = Color.white;
    private Color stoneColor = new Color(0.4f, 0.4f, 0.4f, 1f);

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
        if (playerSprite == null)
        {
            playerSprite = GetComponentInChildren<SpriteRenderer>();
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

        if (PlayerPrefs.HasKey("CheckpointX") && PlayerPrefs.HasKey("CheckpointY"))
        {
            float x = PlayerPrefs.GetFloat("CheckpointX");
            float y = PlayerPrefs.GetFloat("CheckpointY");
            Vector3 savedPos = new Vector3(x, y, Player.Instance.transform.position.z);

            transform.position = savedPos;
            rb.position = savedPos;
            lastCheckpointPos = savedPos;
        }

        string lastCheckpoint = SaveManager.GetLastCheckpointID();
        if (!string.IsNullOrEmpty(lastCheckpoint))
        {
            if (!PlayerPrefs.HasKey("PlayerHealth"))
            {
                Health = lowHealthOnSpawn;
                OnHealthChanged?.Invoke(Health);
            }
        }
        else
        {
            HandleSceneTransitionSpawn();
        }
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

        if (SaveManager.IsRingGiven())
        {
            damageAmount = Mathf.RoundToInt(damageAmount * 0.8f);
            if (damageAmount < 1) damageAmount = 1;
        }

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
        if (leftHand != null) leftHand.DisableAttack();
        if (rightHand != null) rightHand.DisableAttack();
        StopAllCoroutines();
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
    public void ApplyPoison(int damage, float duration)
    {
        StartCoroutine(PoisonTicks(damage, duration));
    }

    private IEnumerator PoisonTicks(int damage, float duration)
    {
        for (int i = 0; i < duration; i++)
        {
            yield return new WaitForSeconds(1f);
            if (IsAlive())
            {
                TakeDamage(damage, null);
            }
        }
    }

    public void Petrify(float duration)
    {
        if (!isAlive || isPetrified) return;
        StartCoroutine(PetrifyRoutine(duration));
    }

    private IEnumerator PetrifyRoutine(float duration)
    {
        isPetrified = true;
        playerSprite.color = stoneColor;

        Vector2 oldVelocity = rb.linearVelocity;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        yield return new WaitForSeconds(duration);

        playerSprite.color = originalPlayerColor;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isPetrified = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}