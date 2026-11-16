
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

    [Header("순찰 설정")]
    [Tooltip("몬스터의 좌우 순찰 속도")]
    [SerializeField] private float patrolSpeed = 2f;

    [Tooltip("최초 생성 위치에서 좌우로 이동할 거리")]
    [SerializeField, Min(0.1f)] private float patrolDistance = 3f;

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

    // 현재 바라보는 방향
    private bool movingRight = true;

    // 몬스터가 처음 생성된 위치
    private Vector2 spawnPosition;

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
        // 몬스터가 생성된 위치를 순찰 기준점으로 저장
        spawnPosition = rb != null ? rb.position : transform.position;

        currentState = EnemyState.Patrol;

        movingRight = true;

        FindPlayer();

        // 처음 생성된 순간 오른쪽을 바라보도록 설정
        Flip(true);
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

            // 공격받은 상태가 아니라면 일반 추적
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

        // 공격 중에는 공격 스크립트가 Rigidbody2D 이동을 제어
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
        // 최초 생성 위치를 기준으로 좌우 이동 범위 계산
        float leftLimit = spawnPosition.x - patrolDistance;
        float rightLimit = spawnPosition.x + patrolDistance;

        float currentX = rb.position.x;

        // 오른쪽 끝에 도달하면 왼쪽으로 전환
        if (movingRight && currentX >= rightLimit)
        {
            movingRight = false;
        }
        // 왼쪽 끝에 도달하면 오른쪽으로 전환
        else if (!movingRight && currentX <= leftLimit)
        {
            movingRight = true;
        }

        // 이동 방향에 따른 속도 설정
        float direction = movingRight ? 1f : -1f;

        rb.linearVelocity = new Vector2(
            direction * patrolSpeed,
            rb.linearVelocity.y
        );

        // 스프라이트 방향 변경
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
        // 시야 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // 순찰 범위
        Vector2 center = Application.isPlaying
            ? spawnPosition
            : (Vector2)transform.position;

        float distance = Mathf.Max(0.1f, patrolDistance);

        Gizmos.color = Color.cyan;

        Vector3 left = new Vector3(
            center.x - distance,
            center.y,
            transform.position.z
        );

        Vector3 right = new Vector3(
            center.x + distance,
            center.y,
            transform.position.z
        );

        Gizmos.DrawLine(left, right);
        Gizmos.DrawWireSphere(left, 0.15f);
        Gizmos.DrawWireSphere(right, 0.15f);

        // 시야 방향
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

        // 공격받은 상태의 마지막 플레이어 위치
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
