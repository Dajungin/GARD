
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

    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;

    private float currentChargeTime;
    private bool charging;
    private bool firedThisHold;
    private bool previousTriggerHeld;

    public float ChargeProgress =>
        charging
            ? Mathf.Clamp01(currentChargeTime / chargeTime)
            : 0f;

    protected override void Update()
    {
        base.Update();

        if (Mouse.current == null)
            return;

        // 버튼을 놓으면 다음 충전을 준비한다.
        if (!triggerHeld)
        {
            currentChargeTime = 0f;
            charging = false;
            firedThisHold = false;
            previousTriggerHeld = false;
            return;
        }

        // 버튼을 누르기 시작한 순간
        if (!previousTriggerHeld)
        {
            previousTriggerHeld = true;
            firedThisHold = false;

            if (CanAttack)
            {
                charging = true;
                currentChargeTime = 0f;

                if (showDebugLogs)
                    Debug.Log("레일건 충전 시작", this);
            }
            else
            {
                if (showDebugLogs)
                    Debug.Log("레일건 공격 불가: CanAttack이 false입니다.", this);

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

            if (showDebugLogs)
                Debug.Log("레일건 충전 완료! 발사를 시도합니다.", this);

            TryAttack();
        }
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

        if (cam == null)
        {
            Debug.LogWarning(
                "레일건: 조준용 카메라를 찾을 수 없습니다.",
                this
            );
            return;
        }

        if (Mouse.current == null)
            return;

        Vector3 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        // 2D 게임에서 카메라와 플레이어 사이의 깊이를 지정한다.
        mouseScreenPosition.z =
            Mathf.Abs(cam.transform.position.z - gunPoint.position.z);

        Vector3 mouseWorldPosition =
            cam.ScreenToWorldPoint(mouseScreenPosition);

        Vector2 direction =
            (Vector2)(mouseWorldPosition - gunPoint.position);

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }

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
                "레일건 투사체 프리팹에 WeaponProjectile 컴포넌트가 없습니다.",
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
                $"레일건 발사 성공! 방향: {direction}, 속도: {projectileSpeed}",
                projectileObject
            );
        }
    }
}
