
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

        hitCollider.isTrigger = true;
    }

    public void Initialize(Vector2 direction, float speed)
    {
        moveDirection = direction.normalized;
        moveSpeed = Mathf.Max(0f, speed);

        if (moveDirection.sqrMagnitude <= 0f)
        {
            Debug.LogWarning("마법탄의 이동 방향이 0입니다.", this);
            Destroy(gameObject);
            return;
        }

        float angle =
            Mathf.Atan2(moveDirection.y, moveDirection.x)
            * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        initialized = true;

        Debug.Log(
            $"마법탄 발사! 방향: {moveDirection}, 속도: {moveSpeed}",
            this
        );

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (!initialized)
            return;

        Vector2 movement =
            moveDirection * moveSpeed * Time.fixedDeltaTime;

        rb.position += movement;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized)
            return;

        // 플레이어 또는 플레이어의 자식 오브젝트인지 확인
        PlayerHealth player =
            other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            // 같은 플레이어와 중복 충돌하는 것을 방지
            if (!hitPlayers.Add(player))
                return;

            // 플레이어의 회피 무적 상태 확인
            PlayerMove playerMove =
                player.GetComponent<PlayerMove>();

            if (playerMove == null || !playerMove.IsInvincible)
            {
                player.TakeDamage(damage);
            }

            // 무적 중이어도 마법탄은 충돌하면 사라짐
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
