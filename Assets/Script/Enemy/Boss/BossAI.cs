
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BossHealth))]
public class BossAI : MonoBehaviour
{
    [Header("플레이어")]
    [SerializeField] private Transform target;
    [SerializeField, Min(0f)] private float activationDistance = 12f;

    [Header("이동")]
    [SerializeField, Min(0f)] private float approachSpeed = 2f;
    [SerializeField, Min(0f)] private float stopDistance = 4f;

    [Header("돌진 공격")]
    [SerializeField] private BossAttackHitbox attackHitbox;
    [SerializeField, Min(0f)] private float dashTelegraph = 0.7f;
    [SerializeField, Min(0.01f)] private float dashDuration = 0.4f;
    [SerializeField, Min(0f)] private float dashSpeed = 12f;
    [SerializeField, Min(0f)] private float dashRecovery = 1f;

    [Header("별의 심판 - 충격파")]
    [SerializeField] private BossShockwave shockwavePrefab;
    [SerializeField] private Transform shockwaveSpawnPoint;
    [SerializeField, Min(0f)] private float slamTelegraph = 0.8f;
    [SerializeField, Min(0f)] private float secondWaveDelay = 0.5f;

    [Header("별빛 연사 - 마법탄")]
    [SerializeField] private BossProjectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField, Min(0f)] private float volleyTelegraph = 0.5f;
    [SerializeField, Min(0.01f)] private float projectileSpeed = 8f;
    [SerializeField, Min(0f)] private float projectileSpreadAngle = 12f;
    [SerializeField, Min(0f)] private float projectileInterval = 0.08f;

    [Header("공통")]
    [SerializeField, Min(0f)] private float attackCooldown = 0.5f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private BossHealth bossHealth;
    private Coroutine attackRoutine;

    private bool isAttacking;
    private float cooldownTimer;
    private int facingDirection = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bossHealth = GetComponent<BossHealth>();

        if (attackHitbox != null)
            attackHitbox.SetAttackActive(false);
    }

    private void OnEnable()
    {
        if (bossHealth != null)
            bossHealth.OnPhaseTwoStarted += HandlePhaseTwo;
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                target = playerObject.transform;
        }
    }

    private void OnDisable()
    {
        if (bossHealth != null)
            bossHealth.OnPhaseTwoStarted -= HandlePhaseTwo;

        if (attackHitbox != null)
            attackHitbox.SetAttackActive(false);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        if (bossHealth.IsDead || bossHealth.IsTransitioning)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            target.position
        );

        if (distance > activationDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        FaceTarget();

        if (isAttacking)
            return;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (distance > stopDistance)
        {
            Vector2 direction =
                ((Vector2)target.position - rb.position).normalized;

            rb.linearVelocity =
                new Vector2(direction.x * approachSpeed, 0f);

            return;
        }

        rb.linearVelocity = Vector2.zero;

        // 가까우면 돌진, 그 외에는 충격파와 마법탄을 섞어 사용
        int choice = Random.Range(0, 3);

        if (distance <= stopDistance * 0.65f || choice == 0)
            StartAttack(DashAttack());
        else if (choice == 1)
            StartAttack(ShockwaveAttack());
        else
            StartAttack(ProjectileVolley());
    }

    private void StartAttack(IEnumerator routine)
    {
        if (isAttacking)
            return;

        attackRoutine = StartCoroutine(routine);
    }

    private void FaceTarget()
    {
        if (target == null)
            return;

        if (target.position.x >= transform.position.x)
            facingDirection = 1;
        else
            facingDirection = -1;

        if (spriteRenderer != null)
            spriteRenderer.flipX = facingDirection < 0;

        if (attackHitbox != null)
        {
            Vector3 p = attackHitbox.transform.localPosition;
            p.x = Mathf.Abs(p.x) * facingDirection;
            attackHitbox.transform.localPosition = p;
        }
    }

    // ==========================================
    // 돌진 공격
    // ==========================================

    private IEnumerator DashAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(dashTelegraph);

        if (bossHealth.IsDead || bossHealth.IsTransitioning)
        {
            FinishAttack();
            yield break;
        }

        // 돌진 시작 시점의 방향으로 고정
        int dashDirection = facingDirection;

        if (attackHitbox != null)
            attackHitbox.SetAttackActive(true);

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            if (bossHealth.IsDead || bossHealth.IsTransitioning)
                break;

            rb.linearVelocity =
                new Vector2(dashDirection * dashSpeed, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        if (attackHitbox != null)
            attackHitbox.SetAttackActive(false);

        if (!bossHealth.IsDead && !bossHealth.IsTransitioning)
            yield return new WaitForSeconds(dashRecovery);

        FinishAttack();
    }

    // ==========================================
    // 별의 심판 - 충격파
    // ==========================================

    private IEnumerator ShockwaveAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(slamTelegraph);

        if (!bossHealth.IsDead && !bossHealth.IsTransitioning)
        {
            SpawnShockwaves();

            // 2페이즈에서는 두 번째 충격파 추가 발사
            if (bossHealth.IsPhaseTwo)
            {
                yield return new WaitForSeconds(secondWaveDelay);

                if (!bossHealth.IsDead && !bossHealth.IsTransitioning)
                    SpawnShockwaves();
            }
        }

        if (!bossHealth.IsDead && !bossHealth.IsTransitioning)
            yield return new WaitForSeconds(attackCooldown);

        FinishAttack();
    }

    private void SpawnShockwaves()
    {
        if (shockwavePrefab == null)
        {
            Debug.LogWarning(
                "BossAI: Shockwave Prefab이 연결되지 않았어.",
                this
            );

            return;
        }

        Vector3 spawnPosition = shockwaveSpawnPoint != null
            ? shockwaveSpawnPoint.position
            : transform.position;

        SpawnOneShockwave(spawnPosition, -1);
        SpawnOneShockwave(spawnPosition, 1);
    }

    private void SpawnOneShockwave(Vector3 position, int direction)
    {
        BossShockwave wave = Instantiate(
            shockwavePrefab,
            position,
            Quaternion.identity
        );

        wave.Initialize(direction, bossHealth.IsPhaseTwo);
    }

    // ==========================================
    // 별빛 연사 - 마법탄
    // ==========================================

    private IEnumerator ProjectileVolley()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        // 발사 전 예고
        yield return new WaitForSeconds(volleyTelegraph);

        if (bossHealth.IsDead || bossHealth.IsTransitioning)
        {
            FinishAttack();
            yield break;
        }

        if (target == null || projectilePrefab == null)
        {
            Debug.LogWarning(
                "BossAI: Target 또는 Projectile Prefab이 연결되지 않았어.",
                this
            );

            FinishAttack();
            yield break;
        }

        Vector3 spawnPosition = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position;

        // 발사 직전에 플레이어 방향을 한 번만 계산
        Vector2 lockedAimDirection =
            ((Vector2)target.position -
             (Vector2)spawnPosition).normalized;

        if (lockedAimDirection.sqrMagnitude <= 0.0001f)
        {
            lockedAimDirection = new Vector2(facingDirection, 0f);
        }

        int count = bossHealth.IsPhaseTwo ? 5 : 3;
        float spread = projectileSpreadAngle;

        for (int i = 0; i < count; i++)
        {
            if (bossHealth.IsDead || bossHealth.IsTransitioning)
                break;

            // 고정 조준 방향을 기준으로 좌우 분산
            float angle = count == 1
                ? 0f
                : Mathf.Lerp(
                    -spread,
                    spread,
                    i / (float)(count - 1)
                );

            Vector2 direction =
                RotateDirection(lockedAimDirection, angle);

            BossProjectile projectile = Instantiate(
                projectilePrefab,
                spawnPosition,
                Quaternion.identity
            );

            if (projectile == null)
                continue;

            // 핵심 수정:
            // 현재 보스의 Collider와만 충돌을 무시한다.
            projectile.IgnoreCollisionWithBoss(gameObject);

            // 발사 방향과 속도를 전달
            projectile.Initialize(direction, projectileSpeed);

            if (projectileInterval > 0f && i < count - 1)
                yield return new WaitForSeconds(projectileInterval);
        }

        if (!bossHealth.IsDead && !bossHealth.IsTransitioning)
            yield return new WaitForSeconds(attackCooldown);

        FinishAttack();
    }

    private Vector2 RotateDirection(Vector2 direction, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;

        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        ).normalized;
    }

    // ==========================================
    // 페이즈 전환
    // ==========================================

    private void HandlePhaseTwo()
    {
        Debug.Log("보스 2페이즈: 강화된 공격 패턴!", this);
    }

    // ==========================================
    // 공격 종료
    // ==========================================

    private void FinishAttack()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (attackHitbox != null)
            attackHitbox.SetAttackActive(false);

        isAttacking = false;
        cooldownTimer = attackCooldown;
        attackRoutine = null;
    }
}
