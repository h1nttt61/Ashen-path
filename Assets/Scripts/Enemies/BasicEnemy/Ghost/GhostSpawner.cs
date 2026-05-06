using System.Collections;
using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private float spawnRate = 3f;
    [SerializeField] private int maxEnemiesInZone = 3;
    [SerializeField] private Transform[] spawnPoints;

    private int totalSpawnedCount = 0;
    private bool isPlayerInZone = false;
    private bool isPausedBySpirit = false;
    private Coroutine spawnCoroutine;

    public void DeactivateSpawner(float duration)
    {
        StartCoroutine(DisableRoutine(duration));
    }

    private IEnumerator DisableRoutine(float duration)
    {
        isPausedBySpirit = true;

        GhostAI[] activeGhost = FindObjectsOfType<GhostAI>();
        foreach (GhostAI ghost in activeGhost)
        {
            Destroy(ghost.gameObject);
        }

        yield return new WaitForSeconds(duration);
        isPausedBySpirit = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            if (spawnCoroutine == null)
            {
                spawnCoroutine = StartCoroutine(SpawnRoutine());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (totalSpawnedCount < maxEnemiesInZone)
        {
            if (isPlayerInZone && !isPausedBySpirit)
            {
                SpawnGhost();
            }

            yield return new WaitForSeconds(spawnRate);
        }
        
        spawnCoroutine = null;
    }

    private void SpawnGhost()
    {
        if (spawnPoints.Length == 0 || totalSpawnedCount >= maxEnemiesInZone) return;

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedPoint = spawnPoints[randomIndex];

        Instantiate(ghostPrefab, selectedPoint.position, Quaternion.identity);
        
        totalSpawnedCount++;
    }
}
