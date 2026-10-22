
using System.Collections;
using UnityEngine;

public class DashAttack : EnemyAttack
{
    [Header("공격 준비")]
    [SerializeField] private float windupTime = 0.7f;

    [Header("돌진 설정")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.5f;

    [Header("충돌 피해")]
    [SerializeField] private int damage = 1;

    [Tooltip("콜라이더가 겹치지 않아도 이 거리 이내면 피격 처리합니다.")]
    [SerializeField] private float hitDistance = 0.1f;

    [Header("충돌 무시 설정")]
    [Tooltip("돌진 중 플레이어와 몬스터의 물리적 충돌을 무시합니다.")]
    [SerializeField] private bool ignorePlayerCollisionDuringDash = true;

    [Header("스프라이트")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Collider2D[] enemyColliders;

    private Coroutine dashCoroutine;

    private Collider2D[] ignoredPlayerColliders;
    private bool collisionIgnored;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 몬스터 본체 및 자식 오브젝트의 콜라이더를 가져옵니다.
        enemyColliders = GetComponentsInChildren<Collider2D>(true);

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    protected override void Attack(Transform target)
    {
        if (target == null || dashCoroutine != null)
        {
            return;
        }

        dashCoroutine = StartCoroutine(Dash(target));
    }

    private IEnumerator Dash(Transform target)
    {
        IsAttacking = true;

        // 돌진 시작 시점에 플레이어가 있는 좌우 방향을 결정합니다.
        float directionX = target.position.x - transform.position.x;

        if (Mathf.Abs(directionX) < 0.001f)
        {
            directionX =
                spriteRenderer != null && spriteRenderer.flipX
                ? -1f
                : 1f;
        }

        float dashDirectionX = Mathf.Sign(directionX);

        // 공격 준비 중에는 좌우 이동을 멈춥니다.
        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = dashDirectionX < 0f;
        }

        // 준비 시간 동안 대기합니다.
        if (windupTime > 0f)
        {
            yield return new WaitForSeconds(windupTime);
        }

        if (target == null)
        {
            FinishDash();
            yield break;
        }

        // 플레이어와 몬스터의 충돌을 무시합니다.
        if (ignorePlayerCollisionDuringDash)
        {
            IgnorePlayerCollision(target);
        }

        float timer = 0f;
        bool hasHitPlayer = false;

        // 좌우로만 돌진합니다.
        while (timer < dashDuration)
        {
            if (target == null)
            {
                break;
            }

            rb.linearVelocity = new Vector2(
                dashDirectionX * dashSpeed,
                rb.linearVelocity.y
            );

            // 물리 충돌과 별개로 피해 판정을 확인합니다.
            if (!hasHitPlayer)
            {
                PlayerHealth playerHealth =
                    target.GetComponentInParent<PlayerHealth>();

                if (playerHealth == null)
                {
                    playerHealth =
                        target.GetComponentInChildren<PlayerHealth>();
                }

                if (playerHealth != null &&
                    IsPlayerCloseEnough(target))
                {
                    hasHitPlayer = true;
                    playerHealth.TakeDamage(damage);
                }
            }

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        FinishDash();
    }

    private void IgnorePlayerCollision(Transform target)
    {
        // Player 오브젝트와 자식에 있는 모든 콜라이더를 찾습니다.
        ignoredPlayerColliders =
            target.GetComponentsInChildren<Collider2D>(true);

        collisionIgnored = false;

        foreach (Collider2D enemyCollider in enemyColliders)
        {
            if (enemyCollider == null || !enemyCollider.enabled)
            {
                continue;
            }

            foreach (Collider2D playerCollider in ignoredPlayerColliders)
            {
                if (playerCollider == null || !playerCollider.enabled)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(
                    enemyCollider,
                    playerCollider,
                    true
                );

                collisionIgnored = true;
            }
        }
    }

    private bool IsPlayerCloseEnough(Transform target)
    {
        // 좌우 돌진 중 플레이어와의 거리를 확인합니다.
        // IgnoreCollision 상태에서도 피해 판정이 가능합니다.
        Collider2D[] playerColliders =
            target.GetComponentsInChildren<Collider2D>(true);

        foreach (Collider2D enemyCollider in enemyColliders)
        {
            if (enemyCollider == null || !enemyCollider.enabled)
            {
                continue;
            }

            foreach (Collider2D playerCollider in playerColliders)
            {
                if (playerCollider == null || !playerCollider.enabled)
                {
                    continue;
                }

                ColliderDistance2D distance =
                    Physics2D.Distance(enemyCollider, playerCollider);

                if (distance.isOverlapped ||
                    distance.distance <= hitDistance)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void RestorePlayerCollision()
    {
        if (!collisionIgnored ||
            ignoredPlayerColliders == null)
        {
            return;
        }

        foreach (Collider2D enemyCollider in enemyColliders)
        {
            if (enemyCollider == null)
            {
                continue;
            }

            foreach (Collider2D playerCollider in ignoredPlayerColliders)
            {
                if (playerCollider == null)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(
                    enemyCollider,
                    playerCollider,
                    false
                );
            }
        }

        ignoredPlayerColliders = null;
        collisionIgnored = false;
    }

    private void FinishDash()
    {
        // 플레이어와의 충돌을 다시 활성화합니다.
        RestorePlayerCollision();

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );
        }

        IsAttacking = false;
        dashCoroutine = null;
    }

    private void OnDisable()
    {
        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            dashCoroutine = null;
        }

        RestorePlayerCollision();

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );
        }

        IsAttacking = false;
    }
}
