
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class WeaponProjectile : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("지형 충돌 설정")]
    [SerializeField] private LayerMask terrainLayer;

    [SerializeField, Min(0.01f)]
    private float collisionCheckRadius = 0.05f;

    [Header("투사체 상태")]
    private int damage;
    private float lifeTime;
    private float moveSpeed;

    private Vector2 moveDirection;
    private Vector2 previousPosition;

    private bool canPierceEnemies;
    private bool initialized;
    private bool hasPreviousPosition;
    private bool isDestroyed;

    private readonly HashSet<EnemyHealth> damagedEnemies =
        new HashSet<EnemyHealth>();

    private readonly HashSet<BossHealth> damagedBosses =
        new HashSet<BossHealth>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(
        Vector2 direction,
        float speed,
        int projectileDamage,
        float projectileLifeTime,
        bool pierceEnemies = false)
    {
        damage = Mathf.Max(0, projectileDamage);
        lifeTime = Mathf.Max(0.01f, projectileLifeTime);
        moveSpeed = Mathf.Max(0f, speed);

        canPierceEnemies = pierceEnemies;
        moveDirection = direction.normalized;

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = moveDirection * moveSpeed;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (moveDirection.sqrMagnitude > 0f)
        {
            float angle =
                Mathf.Atan2(moveDirection.y, moveDirection.x)
                * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        previousPosition = rb.position;
        hasPreviousPosition = true;
        initialized = true;

        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (!initialized || !hasPreviousPosition || isDestroyed)
            return;

        Vector2 currentPosition = rb.position;
        Vector2 movement = currentPosition - previousPosition;
        float distance = movement.magnitude;

        if (distance > 0.0001f)
        {
            Vector2 direction = movement / distance;

            RaycastHit2D hit = Physics2D.CircleCast(
                previousPosition,
                collisionCheckRadius,
                direction,
                distance,
                terrainLayer
            );

            if (hit.collider != null)
            {
                // Cast가 보스를 먼저 만났을 경우에는 지형으로 취급하지 않음.
                BossHealth boss =
                    hit.collider.GetComponentInParent<BossHealth>();

                EnemyHealth enemy =
                    hit.collider.GetComponentInParent<EnemyHealth>();

                if (boss == null && enemy == null)
                {
                    DestroyProjectile();
                    return;
                }
            }
        }

        previousPosition = currentPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized || isDestroyed)
            return;

        // 1. 보스 확인
        BossHealth boss = other.GetComponentInParent<BossHealth>();

        if (boss != null)
        {
            if (boss.IsDead || !damagedBosses.Add(boss))
                return;

            boss.TakeDamage(damage);

            if (!canPierceEnemies)
                DestroyProjectile();

            return;
        }

        // 2. 일반 몬스터 확인
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            if (enemy.IsDead || !damagedEnemies.Add(enemy))
                return;

            enemy.TakeDamage(damage);

            if (!canPierceEnemies)
                DestroyProjectile();

            return;
        }

        // 3. 지형에 닿으면 파괴
        if (IsTerrain(other.gameObject))
        {
            DestroyProjectile();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!initialized || isDestroyed)
            return;

        GameObject target = collision.gameObject;

        // 적이나 보스와의 충돌은 Trigger에서 처리
        if (target.GetComponentInParent<BossHealth>() != null ||
            target.GetComponentInParent<EnemyHealth>() != null)
        {
            return;
        }

        if (IsTerrain(target))
        {
            DestroyProjectile();
        }
    }

    private bool IsTerrain(GameObject target)
    {
        int targetLayerMask = 1 << target.layer;
        return (terrainLayer.value & targetLayerMask) != 0;
    }

    private void DestroyProjectile()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;
        rb.linearVelocity = Vector2.zero;

        Destroy(gameObject);
    }
}
