
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BossProjectile : MonoBehaviour
{
    [Header("마법탄 설정")]
    [SerializeField, Min(0)] private int damage = 1;
    [SerializeField, Min(0.1f)] private float lifetime = 4f;

    private Rigidbody2D rb;
    private Collider2D hitCollider;

    // 발사 순간 결정한 방향을 계속 유지
    private Vector2 moveDirection;
    private float moveSpeed;
    private bool initialized;

    private readonly HashSet<PlayerHealth> hitPlayers =
        new HashSet<PlayerHealth>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hitCollider = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

        hitCollider.isTrigger = true;
    }

    public void Initialize(Vector2 direction, float speed)
    {
        // 방향은 Initialize 호출 시 한 번만 저장
        if (direction.sqrMagnitude <= 0.0001f)
        {
            Debug.LogWarning("마법탄의 이동 방향이 0입니다.", this);
            Destroy(gameObject);
            return;
        }

        moveDirection = direction.normalized;
        moveSpeed = Mathf.Max(0f, speed);

        float angle = Mathf.Atan2(
            moveDirection.y,
            moveDirection.x
        ) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        initialized = true;

        Debug.Log(
            $"마법탄 발사! 고정 방향: {moveDirection}, 속도: {moveSpeed}",
            this
        );

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (!initialized)
            return;

        // 플레이어의 위치를 다시 확인하지 않고 고정 방향으로 이동
        Vector2 movement =
            moveDirection * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized)
            return;

        PlayerHealth player =
            other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            if (!hitPlayers.Add(player))
                return;

            PlayerMove playerMove =
                player.GetComponentInParent<PlayerMove>();

            if (playerMove == null || !playerMove.IsInvincible)
            {
                player.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        // 플레이어가 아닌 지형 등에 충돌하면 소멸
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
