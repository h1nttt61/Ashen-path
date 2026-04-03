using System.Collections;
using UnityEngine;

public class SpiritNPC : MonoBehaviour
{
    [Header("Timings")]
    [SerializeField] private float delay = 10f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("References")]
    [SerializeField] private GameObject visualModel;
    [SerializeField] private NPCDialog npcDialog;
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private float stopDistance = 3f;
    private bool isChasing = false;

    private bool isMoving = false;
    private Vector3 targetPosition;
    private bool hasFinishedLife = false;
   

    public void Start()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Update()
    {
        if (hasFinishedLife)
        {
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

            if (transform.position.y > 50f) Destroy(gameObject);
        }
        else
        {
            if (visualModel.activeSelf)
            {
                FacePlayer();
            }
        }
    }

    public void FinalizeSpirit()
    {
        hasFinishedLife = true;
    }

    public void ResumeChase()
    {
        if (!hasFinishedLife && !isChasing)
        {
            StartCoroutine(ChaseAndRestartDialog());
        }
    }

    private IEnumerator ChaseAndRestartDialog()
    {
        isChasing = true;

        if (npcDialog != null && npcDialog.dialogPanel != null)
        {
            npcDialog.dialogPanel.SetActive(true);
            npcDialog.textDisplay.text = "��, ����! � ��� �� ��������!";
        }

        while (Player.Instance != null && Vector3.Distance(transform.position, Player.Instance.transform.position) > stopDistance)
        {
            float directionOffset = Player.Instance.transform.position.x > transform.position.x ? -1.5f : 1.5f;
            targetPosition = Player.Instance.transform.position + new Vector3(directionOffset, 1.5f, 0);

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * 2.5f * Time.deltaTime);
            yield return null;
        }

        isChasing = false;

        yield return new WaitForSeconds(0.5f);

        if (npcDialog != null)
        {
            npcDialog.StartForcedDialog();
        }
    }

    private void FacePlayer()
    {
        if (Player.Instance == null || spriteRenderer == null) return;

        float direction = Player.Instance.transform.position.x - transform.position.x;

        if (direction > 0.1f)
        {
            spriteRenderer.flipX = true;
        }
        else if (direction < -0.1f)
        {
            spriteRenderer.flipX = false;
        }
    }
}
