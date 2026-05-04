using System.Collections;
using UnityEngine;

public class FinalSpiritSequence : MonoBehaviour
{
    [Header("Основные ссылки")]
    [SerializeField] private SpiritNPCTwo spirit;
    [SerializeField] private StaircaseSequence staircase;
    [SerializeField] private GameObject blackBackground;

    [Header("Точки движения Духа")]
    [SerializeField] private Transform stairPoint; 
    [SerializeField] private Transform wallPoint;  

    [Header("Настройки Стены")]
    [SerializeField] private int blocksToDestroy = 5; 

    [Header("Диалоги (Phrases)")]
    [TextArea(3, 10)]
    [SerializeField] private string[] startPhrases; 
    [TextArea(3, 10)]
    [SerializeField] private string[] wallPhrases;  

    private int currentBrokenBlocks = 0;

    private void Start()
    {
        DestructibleBlock.OnAnyBlockDestroyed += () => currentBrokenBlocks++;
        gameObject.SetActive(false);
    }

    public void StartSequence()
    {
        gameObject.SetActive(true);
        StartCoroutine(MasterRoutine());
    }

    private IEnumerator MasterRoutine()
    {
        spirit.gameObject.SetActive(true);
        spirit.transform.position = Player.Instance.transform.position + new Vector3(-3, 2, 0);
        yield return spirit.StartCoroutine("Fade", 1f);

        SetPlayerLock(true); 

        yield return StartCoroutine(ShowDialog(startPhrases));

        yield return StartCoroutine(MoveSpirit(stairPoint.position));
        yield return staircase.RevealStaircase();
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(MoveSpirit(wallPoint.position));
        yield return StartCoroutine(ShowDialog(wallPhrases));

        SetPlayerLock(false); 

        yield return new WaitUntil(() => currentBrokenBlocks >= blocksToDestroy);

        if (blackBackground != null) blackBackground.SetActive(false);

        SaveManager.SaveSandBossStatus(true); 
        SaveManager.SaveGame();

        yield return spirit.StartCoroutine("Fade", 0f);
        spirit.gameObject.SetActive(false);
    }

    private IEnumerator MoveSpirit(Vector3 target)
    {
        float speed = 7f;
        while (Vector3.Distance(spirit.transform.position, target) > 0.1f)
        {
            spirit.transform.position = Vector3.MoveTowards(spirit.transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator ShowDialog(string[] lines)
    {
        NPCDialog dialog = spirit.GetComponent<NPCDialog>();
        if (dialog != null)
        {
            dialog.lines = lines;
            yield return dialog.DisplayFullDialog();
        }
    }

    private void SetPlayerLock(bool locked)
    {
        if (Player.Instance == null) return;
        Player.Instance.movement.enabled = !locked;
        Player.Instance.combat.enabled = !locked; 
        Player.Instance.rb.bodyType = locked ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;
    }
}