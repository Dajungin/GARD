
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

    // 같은 플레이어를 여러 번 공격하지 않도록 기록
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

        Debug.Log("마법탄 생성됨: " + gameObject.name, this);
    }

    /// <summary>
    /// 현재 보스의 Collider와 마법탄의 충돌을 무시한다.
    /// Boss 태그를 새로 만들 필요가 없다.
    /// </summary>
    public void IgnoreCollisionWithBoss(GameObject bossObject)
    {
        if (bossObject == null || hitCollider == null)
        {
            Debug.LogWarning("보스 오브젝트 또는 마법탄 Collider가 없습니다.", this);
            return;
        }

        Collider2D[] bossColliders =
            bossObject.GetComponentsInChildren<Collider2D>(true);

        int ignoredCount = 0;

        foreach (Collider2D bossCollider in bossColliders)
        {
            if (bossCollider == null)
                continue;

            // 마법탄 자신의 Collider는 제외
            if (bossCollider == hitCollider)
                continue;

            Physics2D.IgnoreCollision(hitCollider, bossCollider, true);
            ignoredCount++;
        }

        Debug.Log(
            "마법탄이 현재 보스의 Collider " + ignoredCount +
            "개와 충돌하지 않도록 설정되었습니다.",
            this
        );
    }

    /// <summary>
    /// 마법탄의 발사 방향과 속도를 설정한다.
    /// </summary>
    public void Initialize(Vector2 direction, float speed)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            Debug.LogWarning("마법탄의 이동 방향이 0이어서 삭제합니다.", this);
            Destroy(gameObject);
            return;
        }

        moveDirection = direction.normalized;
        moveSpeed = Mathf.Max(0f, speed);

        float angle =
            Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        initialized = true;

        Debug.Log(
            "마법탄 발사! 방향: " + moveDirection +
            ", 속도: " + moveSpeed,
            this
        );

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (!initialized)
            return;

        rb.MovePosition(
            rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized)
            return;

        // 플레이어 또는 플레이어의 자식 Collider 확인
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            // 이미 맞은 플레이어라면 중복 처리하지 않음
            if (!hitPlayers.Add(player))
                return;

            PlayerMove playerMove =
                player.GetComponentInParent<PlayerMove>();

            // 무적 상태가 아니면 피해 적용
            if (playerMove == null || !playerMove.IsInvincible)
            {
                player.TakeDamage(damage);

                Debug.Log(
                    "마법탄이 플레이어에게 피해를 줌: " + damage,
                    this
                );
            }
            else
            {
                Debug.Log("플레이어가 무적 상태라 피해를 받지 않습니다.", this);
            }

            Destroy(gameObject);
            return;
        }

        // 플레이어가 아닌 일반 지형에 닿으면 삭제
        // 다른 Trigger는 통과
        if (!other.isTrigger)
        {
            Debug.Log(
                "마법탄이 지형 '" + other.name +
                "'과 충돌하여 삭제됩니다.",
                this
            );

            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (initialized)
        {
            Debug.Log(
                "마법탄 삭제됨: " + gameObject.name +
                ", 마지막 위치: " + transform.position,
                this
            );
        }
    }
}
