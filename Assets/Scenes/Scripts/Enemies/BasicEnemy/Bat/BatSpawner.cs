using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BatSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject batPrefab;
    [SerializeField] private float respawnDelay = 60f;
    [SerializeField] private int batsPerPoint = 1;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private List<GameObject> activeBats = new List<GameObject>();
    private bool isRespawning = false;

    void Start()
    {
        SpawnAllBats();
    }

    void Update()
    {
        activeBats.RemoveAll(bat => bat == null);

        if (activeBats.Count == 0 && !isRespawning)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    private void SpawnAllBats()
    {
        foreach (Transform point in spawnPoints)
        {
            for (int i = 0; i < batsPerPoint; i++)
            {
                GameObject bat = Instantiate(batPrefab, point.position, Quaternion.identity);
                activeBats.Add(bat);
            }
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);

        SpawnAllBats();
        isRespawning = false;
    }
}