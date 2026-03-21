using System.Collections;
using UnityEngine;

public class SpiritNPC : MonoBehaviour
{
    [Header("Timings")]
    [SerializeField] private float delay = 30f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("References")]
    [SerializeField] private GameObject visualModel;
    [SerializeField] private NPCDialog npcDialog;

    private bool isMoving = false;
    private Vector3 targetPosition;
    private bool hasFinishedLife = false;

    public void Start()
    {
        visualModel.SetActive(false);
        StartCoroutine(ArrivalRoutine());
    }

    private IEnumerator ArrivalRoutine()
    {

        yield return new WaitForSeconds(delay);

        if (Player.Instance != null && npcDialog != null)
        {

            bool alreadyHasDash = npcDialog.giveDashOnEnd && Player.Instance.isDashUnlocked;
            bool alreadyHasWallJump = npcDialog.giveWallJumpOnEnd && Player.Instance.isWallJumpUnlocked;

            if (alreadyHasDash || alreadyHasWallJump)
            {
                gameObject.SetActive(false);
                yield break;
            }

            transform.position = Player.Instance.transform.position + new Vector3(-10f, 5f, 0);
            visualModel.SetActive(true);

            targetPosition = Player.Instance.transform.position + new Vector3(-2f, 1.5f, 0);
            isMoving = true;

            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
            isMoving = false;
            if (npcDialog != null)
            {
                npcDialog.StartForcedDialog();
            }
        }
    }

    public void Update()
    {
        if (hasFinishedLife)
        {
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

            if (transform.position.y > 50f) Destroy(gameObject);
        }
    }

    public void FinalizeSpirit()
    {
        hasFinishedLife = true;
    }
}
