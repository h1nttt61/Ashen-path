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

        GameObject enemyGO = new GameObject($"Enemy_{data.enemyName}");
        enemyGO.transform.position = position;
        
        if (parent != null)
        {
            enemyGO.transform.SetParent(parent);
        }
        
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
        

        if (data.useSimpleAI && !data.useNavigationAI)
        {
            var enemy = enemyGO.AddComponent<SimplePatrolEnemy>();
            ApplyEnemyData(enemy, data);
        }
        else if (data.useNavigationAI)
        {
            Debug.LogWarning("Navigation AI not implemented yet");
        }

        enemyGO.tag = "Enemy";
        enemyGO.layer = LayerMask.NameToLayer("Enemy");
        
        return enemyGO;
    }
    
    private static void ApplyEnemyData(SimplePatrolEnemy enemy, EnemyData data)
    {
        //in future
    }
}