using UnityEngine;

public class ShadowPlayer : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color color;
    [SerializeField] private float fadeSpeed = 2f; 

    public void Init(Sprite senderSprite, Vector3 position, Quaternion rotation, Vector3 scale, Color ghostColor, float speed)
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = senderSprite;
        sr.sortingOrder = 5; 

        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;

        color = ghostColor;
        fadeSpeed = speed;
        sr.color = color;

        Destroy(gameObject, 2f);
    }

    private void Update()
    {
        color.a -= fadeSpeed * Time.deltaTime;
        sr.color = color;

        if (color.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}
