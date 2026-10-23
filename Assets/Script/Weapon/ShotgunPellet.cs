
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ShotgunPellet : MonoBehaviour
{
    [Header("지형 충돌")]
    [SerializeField] private LayerMask terrainLayer;

    private Rigidbody2D rb;

    private int closeDamage;
    private int farDamage;

    private float closeDistance;
    private float farDistance;
    private float lifeTime;

    private Vector2 originPosition;
    private bool initialized;
    private bool hitTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(
        Vector2 direction,
        float speed,
        float lifetime,
        int nearDamage,
        int distantDamage,
        float nearDistance,
        float distantDistance,
        Vector2 startPosition)
    {
        originPosition = startPosition;

        closeDamage = Mathf.Max(0, nearDamage);
        farDamage = Mathf.Max(0, distantDamage);

        closeDistance = Mathf.Max(0f, nearDistance);
        farDistance = Mathf.Max(closeDistance + 0.01f, distantDistance);

        lifeTime = Mathf.Max(0.1f, lifetime);

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * speed;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (direction.sqrMagnitude > 0f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x)
                          * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        initialized = true;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized || hitTarget)
            return;

        // 지형에 닿으면 소멸
        if (IsTerrain(other.gameObject))
        {
            Destroy(gameObject);
            return;
        }

        // 보스 확인
        BossHealth boss = other.GetComponentInParent<BossHealth>();

        if (boss != null)
        {
            if (boss.IsDead)
                return;

            int damage = CalculateDamage(boss.transform.position);

            boss.TakeDamage(damage);

            hitTarget = true;
            Destroy(gameObject);
            return;
        }

        // 일반 적 확인
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            if (enemy.IsDead)
                return;

            int damage = CalculateDamage(enemy.transform.position);

            enemy.TakeDamage(damage);

            hitTarget = true;
            Destroy(gameObject);
        }
    }

    private int CalculateDamage(Vector2 targetPosition)
    {
        float distance = Vector2.Distance(
            originPosition,
            targetPosition
        );

        if (distance <= closeDistance)
            return closeDamage;

        if (distance >= farDistance)
            return farDamage;

        float t = Mathf.InverseLerp(
            closeDistance,
            farDistance,
            distance
        );

        return Mathf.RoundToInt(
            Mathf.Lerp(closeDamage, farDamage, t)
        );
    }

    private bool IsTerrain(GameObject target)
    {
        return (terrainLayer.value & (1 << target.layer)) != 0;
    }
}
