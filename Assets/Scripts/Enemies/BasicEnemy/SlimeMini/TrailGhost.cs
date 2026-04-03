using UnityEngine;

public class TrailGhost : MonoBehaviour
{
    private SpriteRenderer sp;
    private float alpha;
    [SerializeField] private float speed = 3f;

    public void Init(Sprite srToCopy)
    {
        sp = GetComponent<SpriteRenderer>();
        sp.sprite = srToCopy;
        sp.color = new Color(1f, 1f, 1f, 0.5f);
        alpha = sp.color.a;
    }

    private void Update()
    {
        alpha -= Time.deltaTime * speed;
        sp.color = new Color(sp.color.r, sp.color.g, sp.color.b, alpha);
        if (alpha <= 0f) Destroy(gameObject);
    }
}
