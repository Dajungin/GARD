
using UnityEngine;
using UnityEngine.InputSystem;

public class RayGunWeapon : WeaponBase
{
    [Header("충전 설정")]
    [SerializeField, Min(0.1f)]
    private float chargeTime = 2f;

    [Header("발사 설정")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform gunPoint;
    [SerializeField, Min(0f)]
    private float projectileSpeed = 15f;
    [SerializeField, Min(0.1f)]
    private float projectileLifeTime = 3f;

    [Header("조준 설정")]
    [SerializeField] private Camera aimCamera;

    [Header("애니메이션")]
    [SerializeField] private Animator playerAnimator;

    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;

    private float currentChargeTime;
    private bool charging;
    private bool previousTriggerHeld;

    private static readonly int WeaponTypeHash =
        Animator.StringToHash("WeaponType");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    public float ChargeProgress =>
        charging
            ? Mathf.Clamp01(currentChargeTime / chargeTime)
            : 0f;

    protected override void Update()
    {
        base.Update();

        if (Mouse.current == null)
            return;

        // 마우스 버튼을 놓으면 충전 취소
        if (!triggerHeld)
        {
            currentChargeTime = 0f;
            charging = false;
            previousTriggerHeld = false;
            return;
        }

        // 버튼을 처음 누른 순간
        if (!previousTriggerHeld)
        {
            previousTriggerHeld = true;

            if (!CanAttack)
                return;

            charging = true;
            currentChargeTime = 0f;

            // 레일건 애니메이션 시작
            PlayRailgunAnimation();

            if (showDebugLogs)
                Debug.Log("레일건 충전 시작", this);
        }

        if (!charging)
            return;

        currentChargeTime += Time.deltaTime;

        // 충전 완료
        if (currentChargeTime >= chargeTime)
        {
            currentChargeTime = chargeTime;
            charging = false;

            if (showDebugLogs)
                Debug.Log("레일건 충전 완료! 발사!", this);

            // 실제 투사체 발사
            TryAttack();
        }
    }

    public override void SetTriggerHeld(bool held)
    {
        triggerHeld = held;

        // 버튼을 놓으면 다음 충전을 준비
        if (!held)
        {
            currentChargeTime = 0f;
            charging = false;
            previousTriggerHeld = false;
        }
    }

    private void PlayRailgunAnimation()
    {
        if (playerAnimator == null)
            return;

        // 3번이 아니라 Animator 내부 무기 구분값 3
        playerAnimator.SetInteger(WeaponTypeHash, 3);

        playerAnimator.ResetTrigger(AttackHash);
        playerAnimator.SetTrigger(AttackHash);
    }

    protected override void Attack()
    {
        if (projectilePrefab == null || gunPoint == null)
        {
            Debug.LogWarning(
                "레일건: Projectile Prefab 또는 Gun Point가 없습니다.",
                this
            );
            return;
        }

        Camera cam = aimCamera != null
            ? aimCamera
            : Camera.main;

        if (cam == null || Mouse.current == null)
            return;

        Vector3 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        mouseScreenPosition.z =
            Mathf.Abs(
                cam.transform.position.z - gunPoint.position.z
            );

        Vector3 mouseWorldPosition =
            cam.ScreenToWorldPoint(mouseScreenPosition);

        Vector2 direction =
            (Vector2)(mouseWorldPosition - gunPoint.position);

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        direction.Normalize();

        GameObject projectileObject = Instantiate(
            projectilePrefab,
            gunPoint.position,
            Quaternion.identity
        );

        WeaponProjectile projectile =
            projectileObject.GetComponent<WeaponProjectile>();

        if (projectile == null)
        {
            Debug.LogError(
                "레일건 투사체에 WeaponProjectile이 없습니다.",
                projectileObject
            );

            Destroy(projectileObject);
            return;
        }

        projectile.Initialize(
            direction,
            projectileSpeed,
            damage,
            projectileLifeTime,
            true
        );

        if (showDebugLogs)
        {
            Debug.Log(
                $"레일건 발사 성공! 방향: {direction}",
                projectileObject
            );
        }
    }

    private void OnDisable()
    {
        triggerHeld = false;
        charging = false;
        currentChargeTime = 0f;
        previousTriggerHeld = false;
    }
}
