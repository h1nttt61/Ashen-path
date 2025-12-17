using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private List<EnemyData> availableEnemies = new List<EnemyData>();
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private int maxEnemies = 10;
    
    [Header("Respawn")]
    [SerializeField] private bool respawnEnemies = true;
    [SerializeField] private float respawnTime = 10f;
    
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Dictionary<GameObject, float> respawnTimers = new Dictionary<GameObject, float>();
    
    private void Start()
    {
        if (spawnOnStart && spawnPoints.Count > 0)
        {
            SpawnInitialEnemies();
        }
    }
    
    private void Update()
    {
        if (respawnEnemies)
        {
            UpdateRespawnTimers();
        }
    }
    
    private void SpawnInitialEnemies()
    {
        foreach (var spawnPoint in spawnPoints)
        {
            if (activeEnemies.Count >= maxEnemies) break;
            
            if (availableEnemies.Count > 0)
            {
                EnemyData randomData = availableEnemies[Random.Range(0, availableEnemies.Count)];
                SpawnEnemy(randomData, spawnPoint.position);
            }
        }
    }
    
    public GameObject SpawnEnemy(EnemyData data, Vector2 position)
    {
        if (activeEnemies.Count >= maxEnemies)
        {
            Debug.LogWarning("Max enemies reached!");
            return null;
        }
        
        GameObject enemy = EnemyFactory.CreateEnemy(data, position, transform);
        if (enemy != null)
        {
            activeEnemies.Add(enemy);

            var baseEnemy = enemy.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.OnDeath += () => OnEnemyDeath(enemy);
            }
        }
        
        return enemy;
    }
    
    private void OnEnemyDeath(GameObject enemy)
    {
        if (respawnEnemies)
        {
            respawnTimers[enemy] = respawnTime;
            enemy.SetActive(false);
        }
        else
        {
            activeEnemies.Remove(enemy);
            Destroy(enemy);
        }
    }
    
    private void UpdateRespawnTimers()
    {
        List<GameObject> toRespawn = new List<GameObject>();

        var keysToCheck = new List<GameObject>(respawnTimers.Keys);

        foreach (var kvp in keysToCheck)
        {
            if (!respawnTimers.ContainsKey(kvp)) continue;

            respawnTimers[kvp] -= Time.deltaTime;
            if (respawnTimers[kvp] <= 0)
            {
                toRespawn.Add(kvp);
            }
        }
        
        foreach (var enemy in toRespawn)
        {
            RespawnEnemy(enemy);
            respawnTimers.Remove(enemy);
        }
    }
    
    private void RespawnEnemy(GameObject enemy)
    {
        enemy.SetActive(true);
        enemy.transform.position = GetRandomSpawnPoint();
        
        var baseEnemy = enemy.GetComponent<BaseEnemy>();
        if (baseEnemy != null)
        {
        }
    }
    
    private Vector2 GetRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0) return transform.position;
        return spawnPoints[Random.Range(0, spawnPoints.Count)].position;
    }
    
    public void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            Destroy(enemy);
        }
        activeEnemies.Clear();
        respawnTimers.Clear();
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.DrawIcon(spawnPoint.position, "EnemySpawn.png", true);
            }
        }
    }
}