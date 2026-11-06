
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Search
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

    [Tooltip("공격받은 적이 플레이어를 추적하는 시간입니다.")]
    [SerializeField] private float searchDuration = 5f;

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

    // 공격받았을 때 추적하는지 여부
    private bool isAlerted;

    // 마지막으로 확인한 플레이어 위치
    private Vector2 lastKnownPlayerPosition;

    // 추적 기억 시간
    private float searchTimer;

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
        FindPlayer();
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
        }

        // 공격 중에는 AI 상태 변경을 하지 않음
        if (enemyAttack != null && enemyAttack.IsAttacking)
        {
            currentState = EnemyState.Attack;
            return;
        }

        UpdateSearchTimer();

        if (player != null && CanSeePlayer())
        {
            // 플레이어를 직접 발견하면 위치 갱신
            lastKnownPlayerPosition = player.position;

            // 시야로 발견한 경우에도 기존처럼 추적
            if (!isAlerted)
            {
                currentState = EnemyState.Chase;
            }
        }

        CheckState();
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        // 공격 중에는 GroundSlamAttack 등 공격 스크립트가
        // Rigidbody2D 이동을 직접 제어하도록 함
        if (enemyAttack != null && enemyAttack.IsAttacking)
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

            case EnemyState.Search:
                SearchLastPosition();
                break;
        }
    }

    // =========================================
    // 플레이어 찾기
    // =========================================

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    // =========================================
    // 공격받았을 때 호출할 함수
    // =========================================

    public void AlertFromDamage(Transform attacker)
    {
        if (attacker == null)
            return;

        // 공격한 대상이 플레이어인지 확인
        if (!attacker.CompareTag("Player"))
        {
            Transform playerParent = attacker.root;

            if (!playerParent.CompareTag("Player"))
                return;

            attacker = playerParent;
        }

        // 공격한 플레이어를 타겟으로 지정
        player = attacker;

        // 플레이어 위치 기억
        lastKnownPlayerPosition = player.position;

        // 적을 경계 상태로 변경
        isAlerted = true;

        // 추적 시간 초기화
        searchTimer = searchDuration;

        // 즉시 추적 시작
        currentState = EnemyState.Chase;

        Debug.Log(gameObject.name + " 공격받음! 플레이어 추적 시작!");
    }

    // =========================================
    // 추적 기억 시간
    // =========================================

    private void UpdateSearchTimer()
    {
        if (!isAlerted)
            return;

        // 플레이어를 다시 발견하면 기억 시간 갱신
        if (player != null && CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;
            searchTimer = searchDuration;
            currentState = EnemyState.Chase;
            return;
        }

        // 플레이어를 놓쳤다면 기억 시간 감소
        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            isAlerted = false;
            currentState = EnemyState.Patrol;

            Debug.Log(gameObject.name + " 플레이어 추적 종료");
        }
    }

    // =========================================
    // 상태 확인
    // =========================================

    private void CheckState()
    {
        // 공격 중이면 다른 상태로 바꾸지 않음
        if (enemyAttack != null && enemyAttack.IsAttacking)
        {
            currentState = EnemyState.Attack;
            return;
        }

        // 공격받은 적은 시야와 관계없이 추적
        if (isAlerted)
        {
            if (player != null)
            {
                float distance =
                    Vector2.Distance(transform.position, player.position);

                if (CanSeePlayer() &&
                    enemyAttack != null &&
                    distance <= enemyAttack.AttackRange)
                {
                    bool attacked = enemyAttack.TryAttack(player);

                    if (attacked || enemyAttack.IsAttacking)
                    {
                        currentState = EnemyState.Attack;
                        return;
                    }
                }
            }

            currentState = EnemyState.Chase;
            return;
        }

        if (player == null)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        // 기존 공격 기능
        if (enemyAttack != null)
        {
            float distance =
                Vector2.Distance(transform.position, player.position);

            if (distance <= enemyAttack.AttackRange &&
                CanSeePlayer())
            {
                bool attacked = enemyAttack.TryAttack(player);

                if (attacked || enemyAttack.IsAttacking)
                {
                    currentState = EnemyState.Attack;
                    return;
                }
            }
        }

        // 기존 시야 감지
        currentState = CanSeePlayer()
            ? EnemyState.Chase
            : EnemyState.Patrol;
    }

    // =========================================
    // 순찰
    // =========================================

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

    // =========================================
    // 플레이어 추적
    // =========================================

    private void Chase()
    {
        if (player == null)
        {
            if (isAlerted)
            {
                currentState = EnemyState.Search;
                return;
            }

            StopMovement();
            return;
        }

        float directionX =
            player.position.x - transform.position.x;

        MoveTowards(directionX);
    }

    // =========================================
    // 마지막 위치 탐색
    // =========================================

    private void SearchLastPosition()
    {
        float directionX =
            lastKnownPlayerPosition.x - transform.position.x;

        if (Mathf.Abs(directionX) <= 0.1f)
        {
            StopMovement();
            return;
        }

        MoveTowards(directionX);
    }

    // =========================================
    // 좌우 이동
    // =========================================

    private void MoveTowards(float directionX)
    {
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

    // =========================================
    // 이동 정지
    // =========================================

    private void StopMovement()
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }

    // =========================================
    // 플레이어 시야 확인
    // =========================================

    private bool CanSeePlayer()
    {
        if (player == null || rb == null)
            return false;

        Vector2 directionToPlayer =
            (Vector2)player.position - rb.position;

        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
            return false;

        if (distanceToPlayer <= 0.001f)
            return true;

        directionToPlayer.Normalize();

        Vector2 forward = movingRight
            ? Vector2.right
            : Vector2.left;

        float angle =
            Vector2.Angle(forward, directionToPlayer);

        if (angle > viewAngle * 0.5f)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            directionToPlayer,
            distanceToPlayer,
            obstacleLayer
        );

        if (hit.collider != null)
            return false;

        return true;
    }

    // =========================================
    // 스프라이트 방향 변경
    // =========================================

    private void Flip(bool faceRight)
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.flipX = !faceRight;

        movingRight = faceRight;
    }

    // =========================================
    // 시각화
    // =========================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        if (leftPoint != null && rightPoint != null)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawLine(
                leftPoint.position,
                rightPoint.position
            );
        }

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

        if (isAlerted)
        {
            Gizmos.color = Color.magenta;

            Gizmos.DrawLine(
                transform.position,
                lastKnownPlayerPosition
            );
        }
    }
}
