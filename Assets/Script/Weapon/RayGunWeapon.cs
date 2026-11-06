
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
    [SerializeField, Min(0f)] private float projectileSpeed = 15f;
    [SerializeField, Min(0.1f)] private float projectileLifeTime = 3f;

    [Header("조준 설정")]
    [SerializeField] private Camera aimCamera;

    [Header("애니메이션")]
    [SerializeField] private Animator playerAnimator;

    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;

    private float currentChargeTime;
    private bool charging;
    private bool firedThisHold;
    private bool previousTriggerHeld;

    private static readonly int IsRailgunChargingHash =
        Animator.StringToHash("IsRailgunCharging");

    private static readonly int RailgunFireHash =
        Animator.StringToHash("RailgunFire");

    public float ChargeProgress =>
        charging
            ? Mathf.Clamp01(currentChargeTime / chargeTime)
            : 0f;

    protected override void Update()
    {
        base.Update();

        if (Mouse.current == null)
            return;

        if (!triggerHeld)
        {
            ResetCharge();
            return;
        }

        // 버튼을 처음 누른 순간
        if (!previousTriggerHeld)
        {
            previousTriggerHeld = true;
            firedThisHold = false;

            if (CanAttack)
            {
                charging = true;
                currentChargeTime = 0f;

                SetChargingAnimation(true);

                if (showDebugLogs)
                    Debug.Log("레일건 충전 시작", this);
            }
            else
            {
                if (showDebugLogs)
                    Debug.Log("레일건 공격 대기 중", this);

                return;
            }
        }

        if (!charging || firedThisHold)
            return;

        currentChargeTime += Time.deltaTime;

        // 충전 완료
        if (currentChargeTime >= chargeTime)
        {
            currentChargeTime = chargeTime;
            charging = false;
            firedThisHold = true;

            SetChargingAnimation(false);

            if (showDebugLogs)
                Debug.Log("레일건 충전 완료! 발사!", this);

            TryAttack();
        }
    }

    private void SetChargingAnimation(bool value)
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool(
                IsRailgunChargingHash,
                value
            );
        }
    }

    private void PlayFireAnimation()
    {
        if (playerAnimator == null)
            return;

        playerAnimator.ResetTrigger(RailgunFireHash);
        playerAnimator.SetTrigger(RailgunFireHash);
    }

    private void ResetCharge()
    {
        currentChargeTime = 0f;
        charging = false;
        firedThisHold = false;
        previousTriggerHeld = false;

        SetChargingAnimation(false);
    }

    protected override void Attack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning(
                "레일건: Projectile Prefab이 연결되지 않았습니다.",
                this
            );
            return;
        }

        if (gunPoint == null)
        {
            Debug.LogWarning(
                "레일건: Gun Point가 연결되지 않았습니다.",
                this
            );
            return;
        }

        Camera cam = aimCamera != null ? aimCamera : Camera.main;

        if (cam == null || Mouse.current == null)
            return;

        Vector3 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        mouseScreenPosition.z =
            Mathf.Abs(cam.transform.position.z - gunPoint.position.z);

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

        // 실제 투사체 생성에 성공했을 때 발사 애니메이션 실행
        PlayFireAnimation();

        if (showDebugLogs)
        {
            Debug.Log(
                $"레일건 발사 성공! 방향: {direction}",
                projectileObject
            );
        }
    }

    public override void SetTriggerHeld(bool held)
    {
        triggerHeld = held;

        if (!held)
            ResetCharge();
    }

    private void OnDisable()
    {
        triggerHeld = false;
        ResetCharge();
    }
}
