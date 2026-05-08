using System.Collections;
using UnityEngine;

public class FinalSpiritSequence : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private SpiritNPCTwo spirit;
    [SerializeField] private StaircaseSequence staircase;
    [SerializeField] private Transform wallPoint;

    [Header("Dialogs")]
    [TextArea(3, 10)]
    [SerializeField] private string[] startPhrases; 
    [TextArea(3, 10)]
    [SerializeField] private string[] wallPhrases;  

    private void Start()
    {
        if (SaveManager.IsSandBossDefeated())
        {
            staircase.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            if (spirit != null) spirit.gameObject.SetActive(false);
        }
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
        yield return StartCoroutine(spirit.Fade(0f, 1f));

        yield return StartCoroutine(ShowDialog(startPhrases));

        yield return StartCoroutine(staircase.RevealStaircase());

        yield return StartCoroutine(MoveSpirit(wallPoint.position));
        yield return StartCoroutine(ShowDialog(wallPhrases));

        yield return StartCoroutine(spirit.Fade(1f, 0f));
        spirit.gameObject.SetActive(false);
    }

    private IEnumerator MoveSpirit(Vector3 target)
    {
        float speed = 8f;
        while (Vector3.Distance(spirit.transform.position, target) > 0.1f)
        {
            spirit.transform.position = Vector3.MoveTowards(spirit.transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator ShowDialog(string[] lines)
    {
        NPCDialog dialog = spirit.GetComponent<NPCDialog>();
        if (dialog != null) yield return dialog.DisplayFullDialog();
    }
}