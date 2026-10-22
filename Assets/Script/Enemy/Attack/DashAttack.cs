
using System.Collections;
using UnityEngine;

public class DashAttack : EnemyAttack
{
    [Header("공격 준비")]
    [SerializeField] private float windupTime = 0.7f;

    [Header("돌진 설정")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.5f;

    [Header("충돌 설정")]
    [Tooltip("몬스터 몸통의 Collider를 연결합니다.")]
    [SerializeField] private Collider2D bodyCollider;

    [Tooltip("자식 오브젝트의 공격 판정 Hitbox를 연결합니다.")]
    [SerializeField] private MonsterAttackHitbox attackHitbox;

    [Header("스프라이트")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;

    private Coroutine dashCoroutine;
    private Collider2D[] playerColliders;

    private bool bodyCollisionIgnored;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (bodyCollider == null)
        {
            bodyCollider = GetComponent<Collider2D>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (attackHitbox == null)
        {
            attackHitbox =
                GetComponentInChildren<MonsterAttackHitbox>(true);
        }

        if (attackHitbox != null)
        {
            attackHitbox.EndAttack();
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

        // 공격 시작 시점의 플레이어 위치를 기준으로 좌우 방향을 정합니다.
        float directionX = target.position.x - transform.position.x;

        if (Mathf.Abs(directionX) < 0.001f)
        {
            directionX =
                spriteRenderer != null && spriteRenderer.flipX
                ? -1f
                : 1f;
        }

        float dashDirectionX = Mathf.Sign(directionX);

        // 준비 중에는 수평 이동을 멈춥니다.
        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = dashDirectionX < 0f;
        }

        // 준비 시간
        if (windupTime > 0f)
        {
            yield return new WaitForSeconds(windupTime);
        }

        if (target == null)
        {
            FinishDash();
            yield break;
        }

        // 플레이어의 몸통과 몬스터 몸통 사이의 물리 충돌만 무시합니다.
        IgnoreBodyCollision(target);

        // 공격 판정은 돌진 시작 시 활성화합니다.
        if (attackHitbox != null)
        {
            attackHitbox.BeginAttack();
        }

        float timer = 0f;

        while (timer < dashDuration)
        {
            if (target == null)
            {
                break;
            }

            // 좌우 방향으로만 돌진하고 Y축 속도는 유지합니다.
            rb.linearVelocity = new Vector2(
                dashDirectionX * dashSpeed,
                rb.linearVelocity.y
            );

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        FinishDash();
    }

    private void IgnoreBodyCollision(Transform target)
    {
        if (bodyCollider == null)
        {
            return;
        }

        playerColliders =
            target.GetComponentsInChildren<Collider2D>(true);

        bodyCollisionIgnored = false;

        foreach (Collider2D playerCollider in playerColliders)
        {
            if (playerCollider == null ||
                !playerCollider.enabled ||
                playerCollider.isTrigger)
            {
                continue;
            }

            Physics2D.IgnoreCollision(
                bodyCollider,
                playerCollider,
                true
            );

            bodyCollisionIgnored = true;
        }
    }

    private void RestoreBodyCollision()
    {
        if (!bodyCollisionIgnored ||
            bodyCollider == null ||
            playerColliders == null)
        {
            return;
        }

        foreach (Collider2D playerCollider in playerColliders)
        {
            if (playerCollider == null)
            {
                continue;
            }

            Physics2D.IgnoreCollision(
                bodyCollider,
                playerCollider,
                false
            );
        }

        playerColliders = null;
        bodyCollisionIgnored = false;
    }

    private void FinishDash()
    {
        // 공격 판정을 먼저 끕니다.
        if (attackHitbox != null)
        {
            attackHitbox.EndAttack();
        }

        // 몸통 충돌을 복구합니다.
        RestoreBodyCollision();

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

        if (attackHitbox != null)
        {
            attackHitbox.EndAttack();
        }

        RestoreBodyCollision();

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
