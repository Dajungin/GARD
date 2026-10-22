
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("지형 충돌")]
    [SerializeField] private LayerMask terrainLayer;

    private Rigidbody2D rb;

    private Vector2 direction;
    private float speed;
    private int damage;
    private bool initialized;
    private bool destroyed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        GetComponent<Collider2D>().isTrigger = true;
    }

    public void Initialize(
        Vector2 moveDirection,
        float moveSpeed,
        int projectileDamage,
        float lifeTime)
    {
        direction = moveDirection.normalized;
        speed = Mathf.Max(0f, moveSpeed);
        damage = Mathf.Max(0, projectileDamage);

        rb.linearVelocity = direction * speed;

        if (direction.sqrMagnitude > 0f)
        {
            float angle =
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        initialized = true;

        Destroy(gameObject, Mathf.Max(0.1f, lifeTime));
    }

    private void FixedUpdate()
    {
        if (!initialized || destroyed)
            return;

        // Kinematic Rigidbody2D를 사용해 지정한 방향으로 이동
        rb.MovePosition(
            rb.position + direction * speed * Time.fixedDeltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized || destroyed)
            return;

        // 지형에 맞으면 사라짐
        if (IsInLayerMask(other.gameObject.layer, terrainLayer))
        {
            DestroyProjectile();
            return;
        }

        // 플레이어의 체력 컴포넌트 찾기
        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            DestroyProjectile();
        }
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private void DestroyProjectile()
    {
        if (destroyed)
            return;

        destroyed = true;
        rb.linearVelocity = Vector2.zero;

        Destroy(gameObject);
    }
}
