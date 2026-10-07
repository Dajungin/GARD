using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyState
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("플레이어")]
    [SerializeField] private Transform player;

    [Header("공격")]
    [SerializeField] private EnemyAttack enemyAttack;

    [Header("순찰 구역")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    [Header("순찰 설정")]
    [SerializeField] private float patrolSpeed = 2f;

    [Header("추적 설정")]
    [SerializeField] private float chaseSpeed = 4f;

    [Header("시야 설정")]
    [SerializeField] private float viewDistance = 6f;

    [SerializeField] private float viewAngle = 90f;

    [Header("벽 설정")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("스프라이트")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;

    private EnemyState currentState;

    private bool movingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentState = EnemyState.Patrol;

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        CheckState();
    }

    private void FixedUpdate()
    {
        if (player == null)
            return;

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                Chase();
                break;

            case EnemyState.Attack:
                StopMovement();
                break;
        }
    }

    // =========================================================
    // 상태 확인
    // =========================================================

    private void CheckState()
    {
        if (enemyAttack == null)
        {
            if (CanSeePlayer())
            {
                currentState = EnemyState.Chase;
            }
            else
            {
                currentState = EnemyState.Patrol;
            }

            return;
        }

        // 공격 중이면 AI가 다른 행동을 하지 않음
        if (enemyAttack.IsAttacking)
        {
            currentState = EnemyState.Attack;
            return;
        }

        float distanceToPlayer =
            Vector2.Distance(
                transform.position,
                player.position
            );

        // 공격 범위에 들어왔는지 확인
        if (distanceToPlayer <= enemyAttack.AttackRange)
        {
            if (CanSeePlayer())
            {
                bool attacked =
                    enemyAttack.TryAttack(player);

                if (attacked)
                {
                    currentState = EnemyState.Attack;
                    return;
                }
            }
        }

        // 플레이어가 보이면 추적
        if (CanSeePlayer())
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            // 플레이어를 놓치면 순찰
            currentState = EnemyState.Patrol;
        }
    }

    // =========================================================
    // 순찰
    // =========================================================

    private void Patrol()
    {
        if (leftPoint == null || rightPoint == null)
        {
            StopMovement();
            return;
        }

        Vector2 velocity =
            rb.linearVelocity;

        if (movingRight)
        {
            velocity.x = patrolSpeed;

            if (transform.position.x >=
                rightPoint.position.x)
            {
                movingRight = false;
            }
        }
        else
        {
            velocity.x = -patrolSpeed;

            if (transform.position.x <=
                leftPoint.position.x)
            {
                movingRight = true;
            }
        }

        rb.linearVelocity = velocity;

        Flip(movingRight);
    }

    // =========================================================
    // 추적
    // =========================================================

    private void Chase()
    {
        if (player == null)
            return;

        float directionX =
            player.position.x -
            transform.position.x;

        if (directionX > 0f)
        {
            rb.linearVelocity =
                new Vector2(
                    chaseSpeed,
                    rb.linearVelocity.y
                );

            Flip(true);
        }
        else if (directionX < 0f)
        {
            rb.linearVelocity =
                new Vector2(
                    -chaseSpeed,
                    rb.linearVelocity.y
                );

            Flip(false);
        }
        else
        {
            StopMovement();
        }
    }

    // =========================================================
    // 이동 정지
    // =========================================================

    private void StopMovement()
    {
        rb.linearVelocity =
            new Vector2(
                0f,
                rb.linearVelocity.y
            );
    }

    // =========================================================
    // 플레이어 시야
    // =========================================================

    private bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector2 directionToPlayer =
            player.position - transform.position;

        float distanceToPlayer =
            directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
            return false;

        directionToPlayer.Normalize();

        Vector2 forward =
            movingRight
            ? Vector2.right
            : Vector2.left;

        float angle =
            Vector2.Angle(
                forward,
                directionToPlayer
            );

        if (angle > viewAngle * 0.5f)
            return false;

        RaycastHit2D hit =
            Physics2D.Raycast(
                transform.position,
                directionToPlayer,
                distanceToPlayer,
                obstacleLayer
            );

        if (hit.collider != null)
            return false;

        return true;
    }

    // =========================================================
    // 방향 전환
    // =========================================================

    private void Flip(bool faceRight)
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.flipX = !faceRight;
    }

    // =========================================================
    // Scene 표시
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        // 시야 거리
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            viewDistance
        );

        // 순찰 범위
        if (leftPoint != null &&
            rightPoint != null)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawLine(
                leftPoint.position,
                rightPoint.position
            );
        }

        // 시야
        Vector2 forward =
            movingRight
            ? Vector2.right
            : Vector2.left;

        Vector2 leftView =
            Quaternion.Euler(
                0f,
                0f,
                viewAngle * 0.5f
            ) * forward;

        Vector2 rightView =
            Quaternion.Euler(
                0f,
                0f,
                -viewAngle * 0.5f
            ) * forward;

        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            transform.position,
            leftView * viewDistance
        );

        Gizmos.DrawRay(
            transform.position,
            rightView * viewDistance
        );
    }
}