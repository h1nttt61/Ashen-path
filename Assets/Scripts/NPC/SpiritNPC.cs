using System.Collections;
using UnityEngine;

public class SpiritNPC : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject visualModel;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool isFading = false;
    private float fadeSpeed = 1.5f;

    public void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Update()
    {
        if (!isFading && visualModel.activeSelf)
        {
            FacePlayer();
        }
    }

    public void StartFadeOut()
    {
        if (!isFading) StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        isFading = true;
        Color c = spriteRenderer.color;

        while (c.a > 0)
        {
            c.a -= Time.deltaTime * fadeSpeed;
            spriteRenderer.color = c;
            yield return null;
        }

        gameObject.SetActive(false); 
    }

    private void FacePlayer()
    {
        if (Player.Instance == null || spriteRenderer == null) return;
        float direction = Player.Instance.transform.position.x - transform.position.x;
        spriteRenderer.flipX = direction < 0;
    }
}