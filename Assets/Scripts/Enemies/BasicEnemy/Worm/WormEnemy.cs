using UnityEngine;

public class WormEnemy : MonoBehaviour
{
    private enum WormState { Hidden, Appearing, Staying, Disappearing }
    [SerializeField] private WormState currentState = WormState.Hidden;

    [Header("Settings")]
    [SerializeField] private bool isCeiling = false;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float jumpDelay = 0.5f;
    [SerializeField] private float outDuration = 0.8f;

    [Header("References")]
    private Animator anim;
    private BoxCollider2D boxCollider;
    private Player player;

    private float timer;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        player = Player.Instance;

        boxCollider.enabled = false;
        currentState = WormState.Hidden;
        timer = jumpDelay;
        if (isCeiling)
        {
            transform.localScale = new Vector3(transform.localScale.x, -Mathf.Abs(transform.localScale.y), transform.localScale.z);
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        switch (currentState)
        {
            case WormState.Hidden:
                if (distance < detectionRange)
                {
                    timer -= Time.fixedDeltaTime;
                    if (timer <= 0)
                    {
                        GoOut();
                    }
                }
                break;

            case WormState.Staying:
                timer -= Time.fixedDeltaTime;
                if (timer <= 0)
                {
                    GoIn();
                }
                break;

            case WormState.Disappearing:
                timer -= Time.fixedDeltaTime;
                if (timer <= 0)
                {
                    currentState = WormState.Hidden;
                    timer = jumpDelay;
                    boxCollider.enabled = false;
                }
                break;
        }
    }

    private void GoOut()
    {
        currentState = WormState.Staying;
        timer = outDuration;
        anim.SetTrigger("getOut");
        boxCollider.enabled = true;
    }

    private void GoIn()
    {
        currentState = WormState.Disappearing;
        timer = 0.6f;
        anim.SetTrigger("getBack");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (player.movement.isSuperDashing)
            {
                Die();
            }
            else
            {
                int damage = player.maxHealth / 2;
                player.TakeDamage(damage, transform);
            }
        }
    }

    public void Die() => Destroy(gameObject);
}