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

    private float timer;
    private bool isPlayerInside = false;
    private int currentEnemiesCount = 0;
    private bool isPausedBySpirit = false;

    private void Update()
    {
        if (isPausedBySpirit || !isPlayerInside || Player.Instance == null) return;

        timer += Time.deltaTime;

        if (timer >= spawnRate)
        {
            if (currentEnemiesCount < maxEnemiesInZone)
            {
                SpawnSlime();
            }
            timer = 0;
        }
    }

    private void SpawnSlime()
    {
        int index = Random.Range(0, spawnPoints.Length);
        GameObject slime = Instantiate(slimePrefab, spawnPoints[index].position, Quaternion.identity);

        currentEnemiesCount++;

    }

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

        currentEnemiesCount = 0; 

        yield return new WaitForSeconds(duration);

        isPausedBySpirit = false;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInside = false;
    }

    public void EnemyDied() => currentEnemiesCount--;
}