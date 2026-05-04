using System.Collections;
using UnityEngine;
using UnityEngine.AI;
public class StaircaseSequence : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float delayBetweenSteps = 0.25f;
    [SerializeField] private Vector3 slideOffset = new Vector3(0, -10f, 0); 
    [SerializeField] private float moveSpeed = 4f;

    private Vector3[] finalPositions;
    private Transform[] steps;

    private void Awake()
    {
        PrepareSteps();
    }
     
    private void OnValidate()
    {
    }

    private void PrepareSteps()
    {
        int childCount = transform.childCount;
        steps = new Transform[childCount];
        finalPositions = new Vector3[childCount];

        for (int i = 0; i < childCount; i++)
        {
            steps[i] = transform.GetChild(i);
            finalPositions[i] = steps[i].localPosition;

            steps[i].localPosition = finalPositions[i] + slideOffset;

            steps[i].gameObject.SetActive(false);
        }
    }

    public IEnumerator RevealStaircase()
    {
        if (steps == null) PrepareSteps();

        for (int i = 0; i < steps.Length; i++)
        {
            steps[i].gameObject.SetActive(true); 
            StartCoroutine(MoveStep(steps[i], finalPositions[i]));

            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(0.2f, 0.3f); 
            
            yield return new WaitForSeconds(delayBetweenSteps);
        }
    }

    private IEnumerator MoveStep(Transform step, Vector3 targetPos)
    {
        Vector3 startPos = step.localPosition;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            step.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        step.localPosition = targetPos;
    }
}