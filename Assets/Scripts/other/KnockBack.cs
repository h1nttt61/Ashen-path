using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockBack : MonoBehaviour
{
    public static KnockBack Instance { get; private set; } 
    [SerializeField] private float knockBackForce = 1f;
    [SerializeField] private float knockBackMovingTimerMax = 0.3f;

    private float _knockBackMovingTimer;

    private Rigidbody2D _rigidbody2d;

    public bool isGettingKnock
    {
        get; private set;
    }

    private void Awake()
    {
        Instance = this;
        _rigidbody2d = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _knockBackMovingTimer -= Time.deltaTime;

        if (_knockBackMovingTimer < 0)
            StopKnockBackMovement();
    }

    public void GetKnockedBack(Transform damageSource)
    {
        isGettingKnock = true;
        _knockBackMovingTimer = knockBackMovingTimerMax;
        Vector2 difference = (transform.position - damageSource.position).normalized * knockBackForce / _rigidbody2d.mass;
        _rigidbody2d.AddForce(difference, ForceMode2D.Impulse);
    }

    private void StopKnockBackMovement()
    {
        _rigidbody2d.linearVelocity = Vector2.zero;
        isGettingKnock = false;
    }
}
