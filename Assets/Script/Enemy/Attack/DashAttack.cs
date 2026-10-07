using System.Collections;
using UnityEngine;

public class DashAttack : EnemyAttack
{
    [Header("돌진 설정")]
    [SerializeField] private float dashSpeed = 12f;

    [SerializeField] private float dashDuration = 0.5f;

    [SerializeField] private int damage = 1;

    [Header("플레이어 피격 판정")]
    [SerializeField] private float hitDistance = 0.7f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Attack(Transform target)
    {
        StartCoroutine(Dash(target));
    }

    private IEnumerator Dash(Transform target)
    {
        IsAttacking = true;

        // 공격 시작 순간의 플레이어 방향을 저장
        Vector2 dashDirection =
            (target.position - transform.position).normalized;

        float timer = 0f;

        bool hasHitPlayer = false;

        while (timer < dashDuration)
        {
            // 저장된 방향으로만 이동
            rb.linearVelocity =
                dashDirection * dashSpeed;

            // 플레이어가 돌진 경로에 들어왔는지 확인
            if (!hasHitPlayer)
            {
                float distance =
                    Vector2.Distance(
                        transform.position,
                        target.position
                    );

                if (distance <= hitDistance)
                {
                    PlayerHealth playerHealth =
                        target.GetComponent<PlayerHealth>();

                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage);

                        hasHitPlayer = true;
                    }
                }
            }

            timer += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;

        IsAttacking = false;
    }
}