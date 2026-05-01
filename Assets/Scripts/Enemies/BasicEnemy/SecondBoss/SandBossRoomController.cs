using System.Collections;
using UnityEngine;

public class SandBossRoomController : MonoBehaviour
{
    [SerializeField] private BossRoomController.DoorData[] doors;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private SandStormIntro stormEffect;
    [SerializeField] private Camera mainCamera;

    private bool bossFightStarted = false;

    public void StartBossFight()
    {
        if (bossFightStarted) return;
        bossFightStarted = true;
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        foreach (var door in doors) StartCoroutine(MoveDoor(door.doorTransform, door.closedPosition));

        Vector3 startCamPos = mainCamera.transform.position;
        Vector3 targetCamPos = new Vector3(spawnPoint.position.x, spawnPoint.position.y, startCamPos.z);
        yield return StartCoroutine(LerpCamera(startCamPos, targetCamPos));

        yield return StartCoroutine(stormEffect.PlayVortex(spawnPoint.position, 2.0f));

        GameObject boss = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);
        SandBossAI ai = boss.GetComponent<SandBossAI>();

        yield return StartCoroutine(ai.FadeIn(1.5f));

        Vector3 playerPos = new Vector3(Player.Instance.transform.position.x, Player.Instance.transform.position.y, startCamPos.z);
        yield return StartCoroutine(LerpCamera(mainCamera.transform.position, playerPos));

        ai.Activate();
    }

    private IEnumerator LerpCamera(Vector3 from, Vector3 to)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 1.5f;
            mainCamera.transform.position = Vector3.Lerp(from, to, t);
            yield return null;
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