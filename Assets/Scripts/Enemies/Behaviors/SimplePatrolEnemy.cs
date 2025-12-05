using UnityEngine;

/// <summary>
/// Простой враг, который патрулирует по платформе
/// Не реагирует на окружение кроме столкновений со стенами
/// </summary>
public class SimplePatrolEnemy : BaseEnemy
{
    [Header("Patrol Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private float edgeCheckDistance = 0.5f;
    [SerializeField] private bool startFacingRight = true;
    
    [Header("Idle Settings")]
    [SerializeField] private float idleTime = 1f;
    [SerializeField] private bool hasIdleState = true;
    
    // Патрульные переменные
    private Vector2 startPosition;
    private Vector2 patrolLeftBound;
    private Vector2 patrolRightBound;
    private bool isMovingRight;
    private bool isIdle;
    private float idleTimer;
    
    #region BaseEnemy Implementation
    
    protected override void Initialize()
    {
        startPosition = transform.position;
        isMovingRight = startFacingRight;
        
        // Определяем границы патрулирования
        patrolLeftBound = startPosition + Vector2.left * patrolDistance;
        patrolRightBound = startPosition + Vector2.right * patrolDistance;
        
        // Настраиваем Rigidbody
        if (rb != null)
        {
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        // Инициализация аниматора
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
            animator.SetBool("IsFacingRight", isMovingRight);
        }
        
        // Задел для навигации
        if (CanUseNavigation())
        {
            InitializeNavigation();
        }
    }
    
    protected override void UpdateEnemy()
    {
        if (isIdle)
        {
            UpdateIdleState();
            return;
        }
        
        UpdatePatrol();
        UpdateSpriteDirection();
        UpdateAnimator();
    }
    
    protected override void FixedUpdateEnemy()
    {
        if (!isAlive || isIdle) return;
        
        Move();
        CheckForEdges();
    }
    
    #endregion
    
    #region Patrol Logic
    
    private void UpdatePatrol()
    {
        // Простое патрулирование между точками
        if (isMovingRight)
        {
            if (transform.position.x >= patrolRightBound.x)
            {
                TurnAround();
            }
        }
        else
        {
            if (transform.position.x <= patrolLeftBound.x)
            {
                TurnAround();
            }
        }
    }
    
    private void Move()
    {
        if (rb == null) return;
        
        float moveDirection = isMovingRight ? 1f : -1f;
        Vector2 velocity = rb.velocity;
        velocity.x = moveDirection * moveSpeed;
        rb.velocity = velocity;
    }
    
    private void CheckForEdges()
    {
        if (!isAlive) return;
        
        // Проверка края платформы
        Vector2 checkPosition = (Vector2)transform.position + 
            new Vector2((isMovingRight ? hitCollider.bounds.extents.x : -hitCollider.bounds.extents.x) + 
                       (isMovingRight ? edgeCheckDistance : -edgeCheckDistance), 
                       -hitCollider.bounds.extents.y - 0.1f);
        
        bool hasGround = Physics2D.Raycast(checkPosition, Vector2.down, 0.2f, groundLayer);
        
        if (!hasGround)
        {
            TurnAround();
        }
        
        // Проверка стены впереди
        Vector2 wallCheckPos = (Vector2)transform.position + 
            new Vector2((isMovingRight ? hitCollider.bounds.extents.x : -hitCollider.bounds.extents.x) + 
                       (isMovingRight ? 0.1f : -0.1f), 0);
        
        bool hasWall = Physics2D.Raycast(wallCheckPos, 
            isMovingRight ? Vector2.right : Vector2.left, 
            0.1f, groundLayer);
        
        if (hasWall)
        {
            TurnAround();
        }
    }
    
    private void TurnAround()
    {
        if (!isAlive) return;
        
        if (hasIdleState && !isIdle)
        {
            StartIdle();
            return;
        }
        
        isMovingRight = !isMovingRight;
        rb.velocity = new Vector2(0, rb.velocity.y);
    }
    
    #endregion
    
    #region Idle State
    
    private void StartIdle()
    {
        isIdle = true;
        idleTimer = 0f;
        
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
        }
        
        if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }
    
    private void UpdateIdleState()
    {
        idleTimer += Time.deltaTime;
        
        if (idleTimer >= idleTime)
        {
            EndIdle();
        }
    }
    
    private void EndIdle()
    {
        isIdle = false;
        isMovingRight = !isMovingRight;
        
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
    }
    
    #endregion
    
    #region Visual Updates
    
    private void UpdateSpriteDirection()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !isMovingRight;
        }
    }
    
    private void UpdateAnimator()
    {
        if (animator == null) return;
        
        animator.SetBool("IsMoving", !isIdle);
        animator.SetBool("IsFacingRight", isMovingRight);
        animator.SetFloat("MoveSpeed", Mathf.Abs(rb.velocity.x));
    }
    
    #endregion
    
    #region Navigation Implementation
    
    protected override bool CanUseNavigation()
    {
        // Возвращаем false, так как этот враг не использует навигацию
        // Но метод оставлен для переопределения в дочерних классах
        return false;
    }
    
    protected override void InitializeNavigation()
    {
        // Базовая реализация для навигации
        // В реальном проекте здесь будет инициализация NavMeshAgent
    }
    
    protected override void UpdateNavigation()
    {
        // Базовая реализация для навигации
        // В реальном проекте здесь будет обновление пути
    }
    
    #endregion
    
    #region Gizmos
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        if (!Application.isPlaying && startPosition == Vector2.zero)
        {
            startPosition = transform.position;
        }
        
        // Границы патрулирования
        Gizmos.color = Color.yellow;
        Vector2 leftBound = Application.isPlaying ? patrolLeftBound : (Vector2)transform.position + Vector2.left * patrolDistance;
        Vector2 rightBound = Application.isPlaying ? patrolRightBound : (Vector2)transform.position + Vector2.right * patrolDistance;
        
        Gizmos.DrawLine(leftBound, rightBound);
        Gizmos.DrawWireSphere(leftBound, 0.2f);
        Gizmos.DrawWireSphere(rightBound, 0.2f);
        
        // Проверка края платформы
        if (hitCollider != null)
        {
            Gizmos.color = Color.cyan;
            Vector2 checkPos = (Vector2)transform.position + 
                new Vector2((isMovingRight ? hitCollider.bounds.extents.x : -hitCollider.bounds.extents.x) + 
                           (isMovingRight ? edgeCheckDistance : -edgeCheckDistance), 
                           -hitCollider.bounds.extents.y - 0.1f);
            
            Gizmos.DrawLine(checkPos, checkPos + Vector2.down * 0.2f);
        }
    }
    
    #endregion
}