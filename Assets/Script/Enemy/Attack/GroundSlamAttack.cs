
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSlamAttack : EnemyAttack
{
    [Header("점프 설정")]
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpHorizontalSpeed = 3f;
    [SerializeField] private float fallSpeed = 15f;

    [Header("착지 확인")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("충격파 설정")]
    [SerializeField] private float shockwaveRange = 3f;
    [SerializeField] private float shockwaveHeight = 1f;
    [SerializeField] private int damage = 1;

    [Header("공격 설정")]
    [SerializeField] private float prepareTime = 0.6f;
    [SerializeField] private float landingDelay = 0.5f;

    private Rigidbody2D rb;
    private Coroutine attackCoroutine;

    private float originalGravityScale;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            originalGravityScale = rb.gravityScale;
    }

    protected override void Attack(Transform target)
    {
        if (rb == null)
        {
            Debug.LogWarning(
                "GroundSlamAttack: Rigidbody2D가 없습니다.",
                this
            );
            return;
        }

        if (groundCheck == null)
        {
            Debug.LogWarning(
                "GroundSlamAttack: GroundCheck가 연결되지 않았습니다.",
                this
            );
            return;
        }

        if (attackCoroutine != null)
            return;

        attackCoroutine = StartCoroutine(GroundSlamCoroutine(target));
    }

    private IEnumerator GroundSlamCoroutine(Transform target)
    {
        IsAttacking = true;

        // 기존 중력 설정 저장
        originalGravityScale = rb.gravityScale;

        // 공격 준비
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(prepareTime);

        if (target == null)
        {
            EndAttack();
            yield break;
        }

        // 플레이어 방향 계산
        float directionX =
            Mathf.Sign(target.position.x - transform.position.x);

        if (directionX == 0)
            directionX = 1f;

        // 점프 시작
        float gravity = Mathf.Abs(Physics2D.gravity.y * originalGravityScale);

        float jumpVelocity = Mathf.Sqrt(
            2f * gravity * jumpHeight
        );

        rb.linearVelocity = new Vector2(
            directionX * jumpHorizontalSpeed,
            jumpVelocity
        );

        // 위로 올라가는 동안 대기
        while (rb.linearVelocity.y > 0f)
        {
            yield return null;
        }

        // 빠르게 낙하
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            -fallSpeed
        );

        // 착지 대기
        while (!CheckGrounded())
        {
            yield return null;
        }

        // 착지
        rb.linearVelocity = Vector2.zero;

        // 충격파 공격
        CreateShockwaveDamage();

        // 착지 후 잠깐 대기
        yield return new WaitForSeconds(landingDelay);

        EndAttack();
    }

    private bool CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    private void CreateShockwaveDamage()
    {
        Vector2 center = new Vector2(
            transform.position.x,
            groundCheck.position.y + shockwaveHeight * 0.5f
        );

        Vector2 size = new Vector2(
            shockwaveRange * 2f,
            shockwaveHeight
        );

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            center,
            size,
            0f
        );

        HashSet<PlayerHealth> damagedPlayers =
            new HashSet<PlayerHealth>();

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player"))
                continue;

            PlayerHealth playerHealth =
                hit.GetComponentInParent<PlayerHealth>();

            if (playerHealth == null)
                continue;

            // 한 번의 충격파에 중복 피해 방지
            if (damagedPlayers.Add(playerHealth))
            {
                playerHealth.TakeDamage(damage);
            }
        }

        Debug.Log("몬스터 점프 내려찍기! 충격파 발생");
    }

    private void EndAttack()
    {
        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
        }

        IsAttacking = false;
        attackCoroutine = null;
    }

    private void OnDisable()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
            rb.linearVelocity = Vector2.zero;
        }

        IsAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            new Vector3(
                transform.position.x,
                groundCheck.position.y + shockwaveHeight * 0.5f,
                transform.position.z
            ),
            new Vector3(
                shockwaveRange * 2f,
                shockwaveHeight,
                0f
            )
        );

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(
            groundCheck.position,
            groundCheck.position +
            Vector3.down * groundCheckDistance
        );
    }
}
