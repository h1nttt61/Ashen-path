using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlimeSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject slimePrefab;
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

        SlimeAI[] activeSlimes = FindObjectsOfType<SlimeAI>();
        foreach (SlimeAI slime in activeSlimes)
        {
            Destroy(slime.gameObject);
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
                SpawnSlime();
            }

            yield return new WaitForSeconds(spawnRate);
        }
        
        spawnCoroutine = null;
    }

    private void SpawnSlime()
    {
        if (spawnPoints.Length == 0 || totalSpawnedCount >= maxEnemiesInZone) return;

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedPoint = spawnPoints[randomIndex];

        Instantiate(slimePrefab, selectedPoint.position, Quaternion.identity);
        
        totalSpawnedCount++;
    }
}