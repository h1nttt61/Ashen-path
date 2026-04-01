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

    [Header("Intro Settings")]
    [SerializeField] private GameObject smallSlimePrefab;
    [SerializeField] private int slimeCount = 12;
    [SerializeField] private float gatherDuration = 2.0f;

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera; 
    [SerializeField] private float cameraMoveSpeed = 2f;
    [SerializeField] private float bossCameraYOffset = 2.5f;

    private BossAI spawnedBoss;
    private bool bossFightStarted = false;

    private void Start()
    {
        if (SaveManager.IsBossDefeated())
        {
            this.enabled = false;
            if (entryTrigger != null) entryTrigger.SetActive(false);
            return;
        }
        Player.OnHealthChanged += CheckPlayerDeath;
        foreach (var door in doors)
        {
            door.doorTransform.position = door.openPosition;
        }
    }

    private void Update()
    {
        if (spawnedBoss != null)
        {
            if (spawnedBoss.curState == BossAI.BossState.Dead)
                Debug.Log("Босс умер");
            else
                Debug.Log("Босс жив");
        }
        Debug.Log($"{bossFightStarted} {spawnedBoss}");
        if (bossFightStarted && spawnedBoss != null && spawnedBoss.curState == BossAI.BossState.Dead)
        {
            EndFight();
        }
    }

    public void StartBossFight(){
        if (bossFightStarted) return;
        bossFightStarted = true;

        if (entryTrigger != null) entryTrigger.SetActive(false);
        StartCoroutine(SequenceStart());
    }

    private IEnumerator SequenceStart()
    {
        foreach (var door in doors)
        {
            StartCoroutine(MoveDoor(door.doorTransform, door.closedPosition, () => {
                CameraShake.Instance.Shake(0.2f, 0.1f);
            }));
        }

        Vector3 originalCamPos = mainCamera.transform.position;
        Vector3 targetCamPos = new Vector3(spawnPoint.position.x, spawnPoint.position.y + bossCameraYOffset, originalCamPos.z);

        float camT = 0;
        while (camT < 1f)
        {
            camT += Time.deltaTime * cameraMoveSpeed;
            mainCamera.transform.position = Vector3.Lerp(originalCamPos, targetCamPos, camT);
            yield return null;
        }

        GameObject bossInstance = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);
        BossAI bossScript = bossInstance.GetComponent<BossAI>();
        SpriteRenderer bossSR = bossInstance.GetComponent<SpriteRenderer>();

        bossInstance.transform.localScale = Vector3.one * 1.5f;
        Color startColor = bossSR.color;
        bossSR.color = new Color(startColor.r, startColor.g, startColor.b, 0.5f);

        for (int i = 0; i < slimeCount; i++)
        {
            float randomX = Random.Range(-6f, 6f);
            Vector3 spawnPos = spawnPoint.position + new Vector3(randomX, 0, 0);

            GameObject s = Instantiate(smallSlimePrefab, spawnPos, Quaternion.identity);
            if (s.TryGetComponent(out IntroSlime intro))
            {
                intro.StartGathering(spawnPoint.position, gatherDuration);
            }
        }

        yield return new WaitForSeconds(gatherDuration);

        float elapsed = 0;
        float growDuration = 1.5f;
        Vector3 mediumScale = new Vector3(0.1f, 0.1f, 1f);
        Vector3 finalScale = new Vector3(0.4345f, 0.3435f, 1f);
        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / growDuration;

            bossInstance.transform.localScale = Vector3.Lerp(mediumScale, finalScale, percent);
            bossSR.color = Color.Lerp(new Color(startColor.r, startColor.g, startColor.b, 0.5f), startColor, percent);

            yield return null;
        }

        CameraShake.Instance.Shake(0.6f, 0.4f);
        float returnThreshold = 0.1f; 
        bool cameraReturned = false;

        while (!cameraReturned)
        {
            Vector3 playerPos = Player.Instance.transform.position;
            Vector3 targetPos = new Vector3(playerPos.x, playerPos.y, mainCamera.transform.position.z);

            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * cameraMoveSpeed);

            if (Vector3.Distance(mainCamera.transform.position, targetPos) < returnThreshold)
            {
                cameraReturned = true;
            }
            yield return null;
        }

        bossScript.ActivateBoss();
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
        Debug.Log("Босс мертв");
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