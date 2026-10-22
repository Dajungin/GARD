
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

    [Tooltip("시야각입니다. 90이면 정면 기준 좌우 45도씩입니다.")]
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

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (enemyAttack == null)
        {
            enemyAttack = GetComponent<EnemyAttack>();
        }
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
        {
            return;
        }

        CheckState();
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

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

    private void CheckState()
    {
        // 공격 도중에는 추적이나 순찰로 상태를 바꾸지 않습니다.
        if (enemyAttack != null && enemyAttack.IsAttacking)
        {
            currentState = EnemyState.Attack;
            return;
        }

        // 공격 컴포넌트가 없다면 시야에 따라 순찰/추적만 합니다.
        if (enemyAttack == null)
        {
            currentState = CanSeePlayer()
                ? EnemyState.Chase
                : EnemyState.Patrol;

            return;
        }

        float distanceToPlayer =
            Vector2.Distance(transform.position, player.position);

        // 공격 범위 안에 플레이어가 있고 시야에 보이면 공격을 시도합니다.
        if (distanceToPlayer <= enemyAttack.AttackRange &&
            CanSeePlayer())
        {
            bool attacked = enemyAttack.TryAttack(player);

            if (attacked || enemyAttack.IsAttacking)
            {
                currentState = EnemyState.Attack;
                return;
            }
        }

        // 공격할 수 없으면 플레이어를 추적하거나 순찰합니다.
        currentState = CanSeePlayer()
            ? EnemyState.Chase
            : EnemyState.Patrol;
    }

    private void Patrol()
    {
        if (leftPoint == null || rightPoint == null)
        {
            StopMovement();
            return;
        }

        Vector2 velocity = rb.linearVelocity;

        if (movingRight)
        {
            velocity.x = patrolSpeed;

            if (transform.position.x >= rightPoint.position.x)
            {
                movingRight = false;
            }
        }
        else
        {
            velocity.x = -patrolSpeed;

            if (transform.position.x <= leftPoint.position.x)
            {
                movingRight = true;
            }
        }

        rb.linearVelocity = velocity;
        Flip(movingRight);
    }

    private void Chase()
    {
        if (player == null)
        {
            StopMovement();
            return;
        }

        float directionX =
            player.position.x - transform.position.x;

        if (directionX > 0.05f)
        {
            rb.linearVelocity = new Vector2(
                chaseSpeed,
                rb.linearVelocity.y
            );

            Flip(true);
        }
        else if (directionX < -0.05f)
        {
            rb.linearVelocity = new Vector2(
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

    private void StopMovement()
    {
        if (rb == null)
        {
            return;
        }

        // 좌우 속도만 멈추고 중력에 의한 Y축 속도는 유지합니다.
        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }

    private bool CanSeePlayer()
    {
        if (player == null)
        {
            return false;
        }

        Vector2 directionToPlayer =
            (Vector2)player.position - rb.position;

        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
        {
            return false;
        }

        if (distanceToPlayer <= 0.001f)
        {
            return true;
        }

        directionToPlayer.Normalize();

        Vector2 forward = movingRight
            ? Vector2.right
            : Vector2.left;

        float angle =
            Vector2.Angle(forward, directionToPlayer);

        if (angle > viewAngle * 0.5f)
        {
            return false;
        }

        // obstacleLayer에 지정한 벽이 시야를 막는지 확인합니다.
        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            directionToPlayer,
            distanceToPlayer,
            obstacleLayer
        );

        if (hit.collider != null)
        {
            return false;
        }

        return true;
    }

    private void Flip(bool faceRight)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        // 원본 스프라이트가 오른쪽을 바라보는 경우를 기준으로 합니다.
        spriteRenderer.flipX = !faceRight;

        // 순찰 방향도 시야 판정에 사용하므로 함께 갱신합니다.
        movingRight = faceRight;
    }

    private void OnDrawGizmosSelected()
    {
        // 시야 거리
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // 순찰 범위
        if (leftPoint != null && rightPoint != null)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawLine(
                leftPoint.position,
                rightPoint.position
            );
        }

        // 시야각
        Vector2 forward = movingRight
            ? Vector2.right
            : Vector2.left;

        Vector2 leftView =
            Quaternion.Euler(0f, 0f, viewAngle * 0.5f)
            * forward;

        Vector2 rightView =
            Quaternion.Euler(0f, 0f, -viewAngle * 0.5f)
            * forward;

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
