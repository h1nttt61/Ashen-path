using UnityEngine;

/// <summary>
/// Базовый класс для всех врагов в игре
/// Абстрактный, требует реализации конкретного поведения
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class BaseEnemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Base Stats")]
    [SerializeField] protected int maxHealth = 10;
    [SerializeField] protected int contactDamage = 1;
    [SerializeField] protected float damageCooldown = 0.5f;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected LayerMask groundLayer;
    
    [Header("Components")]
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Collider2D hitCollider;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Animator animator;
    
    // Текущее состояние
    protected int currentHealth;
    protected float lastDamageTime;
    protected bool isAlive = true;
    
    // События
    public System.Action OnDeath;
    public System.Action<int> OnHealthChanged;
    public System.Action OnDamageTaken;
    
    #region Unity Lifecycle
    
    protected virtual void Awake()
    {
        ValidateComponents();
        currentHealth = maxHealth;
        Initialize();
    }
    
    protected virtual void Start()
    {
        // Можно добавить поиск игрока или другие инициализации
    }
    
    protected virtual void Update()
    {
        if (!isAlive) return;
        UpdateEnemy();
    }
    
    protected virtual void FixedUpdate()
    {
        if (!isAlive) return;
        FixedUpdateEnemy();
    }
    
    protected virtual void OnValidate()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (hitCollider == null) hitCollider = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }
    
    #endregion
    
    #region Abstract Methods
    
    /// <summary>
    /// Инициализация конкретного врага
    /// </summary>
    protected abstract void Initialize();
    
    /// <summary>
    /// Обновление логики врага
    /// </summary>
    protected abstract void UpdateEnemy();
    
    /// <summary>
    /// Физическое обновление врага
    /// </summary>
    protected abstract void FixedUpdateEnemy();
    
    #endregion
    
    #region Health & Damage
    
    public virtual void TakeDamage(int damage, Vector2 direction, DamageType damageType = DamageType.Normal)
    {
        if (!isAlive) return;
        
        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth);
        OnDamageTaken?.Invoke();
        
        // Эффект получения урона
        StartCoroutine(DamageFlashEffect());
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Отбрасывание при получении урона
            ApplyKnockback(direction);
        }
    }
    
    public virtual void Heal(int amount)
    {
        if (!isAlive) return;
        
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    protected virtual void Die()
    {
        isAlive = false;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        hitCollider.enabled = false;
        
        OnDeath?.Invoke();
        
        // Анимация смерти
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // Уничтожение через время
        Destroy(gameObject, 2f);
    }
    
    #endregion
    
    #region Contact Damage
    
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isAlive) return;
        
        // Проверяем, является ли коллизия с игроком
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            TryDealContactDamage(collision.gameObject);
        }
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive) return;
        
        // Проверяем, является ли триггер с игроком
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            TryDealContactDamage(other.gameObject);
        }
    }
    
    protected virtual void TryDealContactDamage(GameObject playerObject)
    {
        if (Time.time - lastDamageTime < damageCooldown) return;
        
        // Получаем компонент здоровья игрока
        var playerHealth = playerObject.GetComponent<IPlayerHealth>();
        if (playerHealth != null)
        {
            Vector2 damageDirection = (playerObject.transform.position - transform.position).normalized;
            playerHealth.TakeDamage(contactDamage, damageDirection);
            lastDamageTime = Time.time;
            
            // Визуальная обратная связь
            StartCoroutine(ContactDamageEffect());
        }
    }
    
    #endregion
    
    #region Effects
    
    protected System.Collections.IEnumerator DamageFlashEffect()
    {
        if (spriteRenderer == null) yield break;
        
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.white;
        
        yield return new WaitForSeconds(0.1f);
        
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
    
    protected System.Collections.IEnumerator ContactDamageEffect()
    {
        if (spriteRenderer == null) yield break;
        
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 1f);
        
        yield return new WaitForSeconds(0.05f);
        
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
    
    #endregion
    
    #region Helper Methods
    
    protected virtual void ValidateComponents()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (hitCollider == null) hitCollider = GetComponent<Collider2D>();
        
        // Настройка Rigidbody2D для 2D платформера
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }
    
    protected virtual void ApplyKnockback(Vector2 direction)
    {
        if (rb != null)
        {
            rb.AddForce(direction * 5f, ForceMode2D.Impulse);
        }
    }
    
    #endregion
    
    #region Navigation Support (задел для NavMesh)
    
    // Виртуальные методы для возможной интеграции с навигацией
    protected virtual bool CanUseNavigation() => false;
    
    protected virtual void InitializeNavigation() { }
    
    protected virtual void UpdateNavigation() { }
    
    #endregion
    
    #region Gizmos
    
    protected virtual void OnDrawGizmosSelected()
    {
        if (hitCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(hitCollider.bounds.center, hitCollider.bounds.size);
        }
    }
    
    #endregion
}

// Интерфейсы для системы урона
public interface IDamageable
{
    void TakeDamage(int damage, Vector2 direction, DamageType damageType = DamageType.Normal);
}

public interface IPlayerHealth
{
    void TakeDamage(int amount, Vector2 direction);
}

public enum DamageType
{
    Normal,
    Fire,
    Ice,
    Poison,
    Spirit
}