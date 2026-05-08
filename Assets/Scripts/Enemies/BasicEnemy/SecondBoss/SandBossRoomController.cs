using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandBossRoomController : MonoBehaviour
{
    [SerializeField] private BossRoomController.DoorData[] doors;
    [Header("Mass Battle Settings")]
    [SerializeField] private List<GameObject> enemyPrefabs; 
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform[] spawnPoints;
    private List<GameObject> activeEnemies = new List<GameObject>();

    [Header("Sequence Link")]
    [SerializeField] private FinalSpiritSequence finalSequence;

    private bool battleStarted = false;

    public void StartBossFight() 
    {
        if (battleStarted) return;
        battleStarted = true;
        StartCoroutine(MassBattleSequence());
    }

    IEnumerator MassBattleSequence()
    {
        foreach (var door in doors) StartCoroutine(MoveDoor(door.doorTransform, door.closedPosition));

        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            if (i < spawnPoints.Length) 
            {
                GameObject enemy = Instantiate(enemyPrefabs[i], spawnPoints[i].position, Quaternion.identity);
                activeEnemies.Add(enemy);
            }
            else
            {
                Debug.LogWarning("Точек спавна меньше, чем префабов врагов!");
            }
        }

        while (IsAnyEnemyAlive())
        {
            yield return new WaitForSeconds(0.5f);
        }

        OnBossDefeated();
    }

    private bool IsAnyEnemyAlive()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i); 
            }
        }
        return activeEnemies.Count > 0;
    }

    public void OnBossDefeated()
    {
        SaveManager.SaveSandBossStatus(true);
        SaveManager.SaveGame();

        if (finalSequence != null)
        {
            finalSequence.StartSequence(); 
        }
    }

    private IEnumerator MoveDoor(Transform door, Vector3 target)
    {
        while (Vector3.Distance(door.position, target) > 0.01f)
        {
            door.position = Vector3.MoveTowards(door.position, target, 5f * Time.deltaTime);
            yield return null;
        }
    }
}