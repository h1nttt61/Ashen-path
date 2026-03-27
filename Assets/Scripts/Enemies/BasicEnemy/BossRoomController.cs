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
        Player.OnHealthChanged += CheckPlayerDeath;
        foreach (var door in doors)
        {
            door.doorTransform.position = door.openPosition;
        }
    }

    private void Update()
    {
        if (bossFightStarted && spawnedBoss != null && spawnedBoss.curState == BossAI.BossState.Dead)
        {
            EndFight();
        }
    }

    public void StartBossFight()
    {
        Debug.Log("Битва началась!");
        if (bossFightStarted) return;
        bossFightStarted = true;

        if (entryTrigger != null) entryTrigger.SetActive(false);
        StartCoroutine(SequenceStart());
    }

    private IEnumerator SequenceStart()
    {
        bool doorsMoving = true;
        foreach (var door in doors)
        {
            StartCoroutine(MoveDoor(door.doorTransform, door.closedPosition, () => {
                CameraShake.Instance.Shake(0.3f, 0.2f);
            }));
        }

        yield return new WaitForSeconds(1.0f);

        GameObject bossInstance = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);
        spawnedBoss = bossInstance.GetComponent<BossAI>();

        CameraShake.Instance.Shake(0.5f, 0.4f);
    }

    private void CheckPlayerDeath(int currentHealth)
    {
        if (currentHealth <= 0 && bossFightStarted)
        {
            ResetRoom();
        }
    }

    public void ResetRoom()
    {
        StopAllCoroutines();
        bossFightStarted = false;

        if (spawnedBoss != null) Destroy(spawnedBoss.gameObject);

        foreach (var door in doors)
        {
            door.doorTransform.position = door.openPosition;
        }

        if (entryTrigger != null) entryTrigger.SetActive(true);
    }

    private void EndFight()
    {
        bossFightStarted = false;
        foreach (var door in doors)
        {
            StartCoroutine(MoveDoor(door.doorTransform, door.openPosition, null));
        }
        this.enabled = false;
    }

    private IEnumerator MoveDoor(Transform door, Vector3 targetPos, System.Action onComplete)
    {
        while (Vector3.Distance(door.position, targetPos) > 0.01f)
        {
            door.position = Vector3.MoveTowards(door.position, targetPos, doorSpeed * Time.deltaTime);
            yield return null;
        }
        door.position = targetPos;
        onComplete?.Invoke();
    }

    private void OnDestroy() => Player.OnHealthChanged -= CheckPlayerDeath;
}