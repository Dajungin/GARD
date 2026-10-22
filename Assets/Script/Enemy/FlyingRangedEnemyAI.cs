
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FlyingRangedEnemyAI : MonoBehaviour
{
    private enum EnemyState
    {
        Patrol, // 정해진 구간 순찰
        Windup, // 정지 후 공격 준비
        Chase   // 플레이어 추적 및 공격
    }

    [Header("플레이어 탐지")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float sightRange = 7f;
    [SerializeField] private float loseSightRange = 10f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("순찰 설정")]
    [SerializeField] private float patrolRange = 4f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolHeight = 2f;
    [SerializeField] private float floatAmplitude = 0.25f;
    [SerializeField] private float floatFrequency = 2f;

    [Header("추적 설정")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float stopDistance = 3f;

    [Header("공격 준비")]
    [SerializeField] private float attackWindupTime = 0.7f;

    [Header("그래픽")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private FlyingEnemyAttack attack;

    private EnemyState currentState = EnemyState.Patrol;

    private Vector2 patrolCenter;
    private float patrolTargetX;
    private float windupTimer;

    private bool movingRight = true;
    private bool hasSeenPlayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        attack = GetComponent<FlyingEnemyAttack>();

        // 비행 몬스터이므로 중력의 영향을 받지 않게 설정
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // 현재 위치를 순찰 중심으로 설정
        patrolCenter = rb.position;
        patrolTargetX = patrolCenter.x + patrolRange;

        // Inspector에서 플레이어를 지정하지 않았다면 Player 태그로 검색
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

        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrolDetection();
                break;

            case EnemyState.Windup:
                UpdateWindup();
                break;

            case EnemyState.Chase:
                UpdateChase();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (player == null)
            return;

        switch (currentState)
        {
            case EnemyState.Patrol:
                MovePatrol();
                break;

            case EnemyState.Windup:
                // 공격 준비 중에는 움직이지 않음
                break;

            case EnemyState.Chase:
                MoveChase();
                break;
        }
    }

    // 순찰 중 플레이어 발견 확인
    private void UpdatePatrolDetection()
    {
        if (CanSeePlayer(sightRange))
        {
            hasSeenPlayer = true;
            BeginWindup();
        }
    }

    // 정지 후 공격 준비
    private void UpdateWindup()
    {
        // 공격 준비 중 시야를 잃으면 추적 상태로 전환
        if (!CanSeePlayer(loseSightRange))
        {
            currentState = EnemyState.Chase;
            return;
        }

        windupTimer -= Time.deltaTime;

        if (windupTimer <= 0f)
        {
            // 공격 스크립트에 발사 요청
            if (attack != null)
            {
                attack.TryFireAt(player);
            }

            // 공격 후 플레이어 추적
            currentState = EnemyState.Chase;
        }
    }

    // 플레이어 추적 및 공격 기회 확인
    private void UpdateChase()
    {
        // 거리가 멀어졌거나 장애물에 가려져 시야를 잃으면 순찰 복귀
        if (hasSeenPlayer && !CanSeePlayer(loseSightRange))
        {
            hasSeenPlayer = false;
            ReturnToPatrol();
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        // 공격 범위 안이고 공격 쿨다운이 끝났으면 정지 후 공격 준비
        if (distance <= attackRange &&
            attack != null &&
            attack.IsReady)
        {
            BeginWindup();
        }
    }

    // 공격 준비 상태로 변경
    private void BeginWindup()
    {
        currentState = EnemyState.Windup;
        windupTimer = attackWindupTime;
    }

    // 좌우 순찰 및 위아래 부유 움직임
    private void MovePatrol()
    {
        float floatingY =
            patrolCenter.y +
            patrolHeight +
            Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        float nextX = Mathf.MoveTowards(
            rb.position.x,
            patrolTargetX,
            patrolSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(new Vector2(nextX, floatingY));

        // 목표 지점에 도착하면 반대쪽으로 이동
        if (Mathf.Abs(nextX - patrolTargetX) < 0.05f)
        {
            movingRight = !movingRight;

            patrolTargetX = patrolCenter.x +
                (movingRight ? patrolRange : -patrolRange);
        }

        UpdateFacing(nextX - rb.position.x);
    }

    // 플레이어를 향해 날아가되 너무 가까워지면 정지
    private void MoveChase()
    {
        if (player == null)
            return;

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = player.position;

        float distance = Vector2.Distance(
            currentPosition,
            targetPosition
        );

        // 공격 거리를 유지
        if (distance <= stopDistance)
            return;

        Vector2 direction =
            (targetPosition - currentPosition).normalized;

        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            chaseSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);

        UpdateFacing(direction.x);
    }

    // 플레이어가 시야 안에 있는지 확인
    private bool CanSeePlayer(float range)
    {
        if (player == null)
            return false;

        Vector2 origin = rb.position;
        Vector2 targetPosition = player.position;

        // 탐지 범위를 벗어나면 발견하지 못함
        if (Vector2.Distance(origin, targetPosition) > range)
            return false;

        // 장애물 레이어가 설정되어 있으면 벽에 가려지는지 검사
        if (obstacleLayer.value != 0)
        {
            RaycastHit2D hit = Physics2D.Linecast(
                origin,
                targetPosition,
                obstacleLayer
            );

            if (hit.collider != null)
                return false;
        }

        return true;
    }

    // 순찰 상태로 복귀
    private void ReturnToPatrol()
    {
        currentState = EnemyState.Patrol;

        // 현재 위치를 새로운 순찰 중심으로 설정
        patrolCenter = rb.position;

        movingRight = true;
        patrolTargetX = patrolCenter.x + patrolRange;
    }

    // 이동 방향에 따라 스프라이트 좌우 반전
    private void UpdateFacing(float directionX)
    {
        if (spriteRenderer == null ||
            Mathf.Abs(directionX) < 0.001f)
        {
            return;
        }

        spriteRenderer.flipX = directionX < 0f;
    }

    // Scene 뷰에서 순찰 범위와 탐지 범위를 표시
    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying
            ? (Vector3)patrolCenter
            : transform.position;

        // 플레이어 탐지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            sightRange
        );

        // 시야 상실 거리
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            transform.position,
            loseSightRange
        );

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );

        // 순찰 구간
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            new Vector3(
                center.x - patrolRange,
                center.y + patrolHeight,
                0f
            ),
            new Vector3(
                center.x + patrolRange,
                center.y + patrolHeight,
                0f
            )
        );
    }
}
