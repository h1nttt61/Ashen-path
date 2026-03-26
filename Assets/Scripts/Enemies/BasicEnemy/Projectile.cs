using UnityEngine;
using UnityEngine.AI;

public class Projectile : MonoBehaviour
{
    [Header("Параметры снаряда")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float lifetime = 7f;
    [SerializeField] private bool destroyOnHit = true;

    private int damage;
    private Vector3 direction;
    private GameObject owner;

    public void Initialize(Vector3 moveDirection, int damageAmount, GameObject source = null)
    {
        direction = moveDirection.normalized;
        damage = damageAmount;
        owner = source;

        // Поворачиваем снаряд в направлении движения
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Уничтожаем снаряд через время
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Движение снаряда
        transform.position += direction * speed * Time.deltaTime * 5;
    }

    void OnTriggerEnter(Collider other)
    {
        // Проверяем, что это не владелец снаряда
        if (owner != null && other.gameObject == owner) return;

        // Проверяем попадание в игрока
        if (other.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(damage, transform);
        }

        // Уничтожаем снаряд
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}