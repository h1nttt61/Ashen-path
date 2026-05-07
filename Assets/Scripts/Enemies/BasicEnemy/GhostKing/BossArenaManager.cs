using UnityEngine;
using System.Collections;

public class BossArenaManager : MonoBehaviour
{
    [Header("Object")]
    public GameObject bossPrefab;
    public Camera mainCamera;         
    public Transform bossSpawnPoint;  
    public Transform player;         

    [Header("Door")]
    public Transform door;
    public Vector3 doorOpenPos;       
    public Vector3 doorClosedPos;     

    [Header("Settings")]
    public float moveSpeed = 3f;      
    public GameObject particles;

    private GhostKing spawnedBoss;
    private bool activated = false;

    public void StartIntro()
    {
        if (activated) return;
        activated = true;
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        GameObject bossObj = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
        spawnedBoss = bossObj.GetComponent<GhostKing>();
        spawnedBoss.enabled = false;

        SpriteRenderer sr = spawnedBoss.GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a = 0;
        sr.color = c;

        float t = 0;
        Vector3 camStart = mainCamera.transform.position;
        Vector3 camTarget = new Vector3(bossSpawnPoint.position.x, bossSpawnPoint.position.y, camStart.z);

        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            mainCamera.transform.position = Vector3.Lerp(camStart, camTarget, t);
            door.localPosition = Vector3.Lerp(doorOpenPos, doorClosedPos, t);
            yield return null;
        }

        float alpha = 0;
        float particleTimer = 0;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * 0.5f; 
            c.a = alpha;
            sr.color = c;

            particleTimer += Time.deltaTime;
            float angle = particleTimer * 200f; 
            float radius = 3f * (1f - alpha + 0.2f); 

            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            GameObject p = Instantiate(particles, bossSpawnPoint.position + offset, Quaternion.identity);
            Destroy(p, 0.6f);

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        t = 0;
        Vector3 camReturnStart = mainCamera.transform.position;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            Vector3 playerPos = new Vector3(player.position.x, player.position.y, camReturnStart.z);
            mainCamera.transform.position = Vector3.Lerp(camReturnStart, playerPos, t);
            yield return null;
        }

        spawnedBoss.enabled = true;
        spawnedBoss.StartFight();
    }
}