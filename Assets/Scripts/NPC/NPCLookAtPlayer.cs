using UnityEngine;

public class NPCLookAtPlayer : MonoBehaviour
{
    private SpriteRenderer sr;
    private Animator anim;

    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (Player.Instance == null || sr == null) return;

        float direction = Player.Instance.transform.position.x - transform.position.x;

        if (direction > 0.1f) sr.flipX = false;
        else if (direction < -0.1f) sr.flipX = true;
    }
}