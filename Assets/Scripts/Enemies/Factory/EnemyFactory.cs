using UnityEngine;

public static class EnemyFactory
{
    public static GameObject CreateEnemy(EnemyData data, Vector2 position, Transform parent = null)
    {
        if (data == null)
        {
            Debug.LogError("EnemyData is null!");
            return null;
        }
        
        // Создаем базовый объект врага
        GameObject enemyGO = new GameObject($"Enemy_{data.enemyName}");
        enemyGO.transform.position = position;
        
        if (parent != null)
        {
            enemyGO.transform.SetParent(parent);
        }
        
        // Добавляем компоненты
        var collider = enemyGO.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.8f, 1.2f);
        
        var rb = enemyGO.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        var spriteRenderer = enemyGO.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = data.defaultSprite;
        spriteRenderer.color = data.spriteColor;
        spriteRenderer.sortingLayerName = "Enemies";
        
        var animator = enemyGO.AddComponent<Animator>();
        if (data.animatorController != null)
        {
            animator.runtimeAnimatorController = data.animatorController;
        }
        
        // Добавляем конкретную реализацию врага
        if (data.useSimpleAI && !data.useNavigationAI)
        {
            var enemy = enemyGO.AddComponent<SimplePatrolEnemy>();
            ApplyEnemyData(enemy, data);
        }
        else if (data.useNavigationAI)
        {
            // Здесь можно добавить врага с навигацией
            Debug.LogWarning("Navigation AI not implemented yet");
        }
        
        // Теги и слои
        enemyGO.tag = "Enemy";
        enemyGO.layer = LayerMask.NameToLayer("Enemy");
        
        return enemyGO;
    }
    
    private static void ApplyEnemyData(SimplePatrolEnemy enemy, EnemyData data)
    {
        // Можно использовать рефлексию или вручную установить значения
        // В реальном проекте лучше использовать сериализацию
    }
}