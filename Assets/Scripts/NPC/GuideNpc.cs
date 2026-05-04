using System.Collections;
using UnityEngine;

public class GuideNpc : MonoBehaviour
{
    [Header("Monolog after boss")]
    [TextArea(3, 10)]
    [SerializeField] private string[] newLines;

    [Header("Guide")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private Vector3 spawnOffset = new Vector3(1.5f, 2f, 0f);
    [SerializeField] private float typingSpeed = 0.04f;
    [SerializeField] private float timeBetweenLines = 2f;

    [Header("Way")]
    [SerializeField] private Transform[] waypoints;

    private NPCDialog originDialog;
    private SpiritNPC spiritNPC;
    private SpriteRenderer sr;

    private void Awake()
    {
        EnsureReferences();
    }
    private void EnsureReferences()
    {
        if (originDialog == null) originDialog = GetComponent<NPCDialog>();
        if (spiritNPC == null) spiritNPC = GetComponent<SpiritNPC>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void StartSequence()
    {
        Debug.Log("Sequence Started!");
        EnsureReferences();

        if (sr == null)
        {
            return;
        }

        transform.position = Player.Instance.transform.position + spawnOffset;

        Color c = sr.color;
        c.a = 0;
        sr.color = c;

        gameObject.SetActive(true);
        StartCoroutine(FullSequenceRoutine());
    }

    private IEnumerator FullSequenceRoutine()
    {
        yield return StartCoroutine(Fade(0, 1));

        SetPlayerLock(true);

        if (originDialog != null && newLines.Length > 0)
        {
            originDialog.dialogPanel.SetActive(true);

            foreach (string line in newLines)
            {
                originDialog.textDisplay.text = "";
                foreach (char letter in line.ToCharArray())
                {
                    originDialog.textDisplay.text += letter;
                    yield return new WaitForSeconds(typingSpeed);
                }
                yield return new WaitForSeconds(timeBetweenLines);
            }

            originDialog.dialogPanel.SetActive(false);
        }

        SetPlayerLock(false);

        if (waypoints != null && waypoints.Length > 0)
        {
            foreach (Transform point in waypoints)
            {
                while (Vector2.Distance(transform.position, point.position) > 0.1f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, point.position, speed * Time.deltaTime);

                    float dir = point.position.x - transform.position.x;
                    if (Mathf.Abs(dir) > 0.1f) sr.flipX = dir < 0;

                    yield return null;
                }
            }
        }
    }

    private IEnumerator Fade(float start, float end)
    {
        Color c = sr.color;
        float elapsed = 0;
        float duration = 1f / fadeSpeed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(start, end, elapsed / duration);
            sr.color = c;
            yield return null;
        }
    }

    private void SetPlayerLock(bool locked)
    {
        if (Player.Instance == null) return;
        Player.Instance.movement.enabled = !locked;
        Player.Instance.combat.enabled = !locked;
        if (locked) Player.Instance.rb.linearVelocity = Vector2.zero;
    }
}