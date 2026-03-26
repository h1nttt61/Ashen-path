using System.Collections;
using UnityEngine;

public class BossRoomController : MonoBehaviour
{
    [System.Serializable]
    public struct DoorData
    {
        public Transform doorTransform;
        public Vector3 openPosition;
        public Vector3 closedPosition;
    }

    [Header("Settings")]
    [SerializeField] private DoorData[] doors;
    [SerializeField] private float doorSpeed = 3f;
    [SerializeField] private GameObject entryTrigger;

    [Header("Spawning")]
    [SerializeField] private GameObject bossPrefab; 
    [SerializeField] private Transform spawnPoint;  

    private BossAI spawnedBoss;
    private bool bossFightStarted = false;

    private void Start()
    {
        foreach (var door in doors)
        {
            door.doorTransform.position = door.openPosition;
        }
    }

   /* private void Update()
    {
        if (bossFightStarted && spawnedBoss != null && spawnedBoss.curState == BossAI.BossState.Dead)
        {
            EndFight();
        }
    }*/

    public void StartBossFight()
    {
        if (bossFightStarted) return;
        bossFightStarted = true;

        if (entryTrigger != null) entryTrigger.SetActive(false);

        GameObject bossInstance = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);
        spawnedBoss = bossInstance.GetComponent<BossAI>();

        foreach (var door in doors)
        {
            StartCoroutine(MoveDoor(door.doorTransform, door.closedPosition));
        }

        Debug.Log("Двери заперты, босс пробудился!");
    }

    private void EndFight()
    {
        bossFightStarted = false;
        foreach (var door in doors)
        {
            StartCoroutine(MoveDoor(door.doorTransform, door.openPosition));
        }

        Debug.Log("Путь свободен.");
        this.enabled = false;
    }

    private IEnumerator MoveDoor(Transform door, Vector3 targetPos)
    {
        while (Vector3.Distance(door.position, targetPos) > 0.01f)
        {
            door.position = Vector3.MoveTowards(door.position, targetPos, doorSpeed * Time.deltaTime);
            yield return null;
        }
        door.position = targetPos;
    }
}