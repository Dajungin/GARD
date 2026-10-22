
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

    // 이미 피해를 준 적에게 중복 피해를 주지 않도록 기록
    private readonly HashSet<EnemyHealth> damagedEnemies =
        new HashSet<EnemyHealth>();

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

        // 투사체 물리 설정
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = moveDirection * moveSpeed;
        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        // 회전 방향 설정
        if (moveDirection.sqrMagnitude > 0f)
        {
            float angle =
                Mathf.Atan2(moveDirection.y, moveDirection.x)
                * Mathf.Rad2Deg;

            transform.rotation =
                Quaternion.Euler(0f, 0f, angle);
        }

        previousPosition = rb.position;
        hasPreviousPosition = true;
        initialized = true;

        // 수명 종료 시 자동 파괴
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (!initialized || !hasPreviousPosition || isDestroyed)
            return;

        Vector2 currentPosition = rb.position;

        Vector2 movement = currentPosition - previousPosition;
        float distance = movement.magnitude;

        // 이전 위치에서 현재 위치까지 지형 충돌 검사
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
                DestroyProjectile();
                return;
            }
        }

        previousPosition = currentPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized || isDestroyed)
            return;

        // 1. 지형에 닿으면 파괴
        if (IsTerrain(other.gameObject))
        {
            DestroyProjectile();
            return;
        }

        // 2. 적인지 확인
        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy == null || enemy.IsDead)
            return;

        // 3. 이미 피해를 준 적이라면 무시
        if (!damagedEnemies.Add(enemy))
            return;

        // 4. 적에게 피해 적용
        enemy.TakeDamage(damage);

        // 5. 관통 기능이 없으면 적에게 맞은 뒤 파괴
        if (!canPierceEnemies)
        {
            DestroyProjectile();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!initialized || isDestroyed)
            return;

        // Trigger가 아닌 충돌 방식으로 설정된 지형도 처리
        if (IsTerrain(collision.gameObject))
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
