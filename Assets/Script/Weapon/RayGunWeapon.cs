
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
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float projectileLifeTime = 3f;

    [Header("조준 설정")]
    [SerializeField] private Camera aimCamera;

    private float currentChargeTime;
    private bool charging;
    private bool firedThisHold;
    private bool previousTriggerHeld;

    public float ChargeProgress =>
        charging ? Mathf.Clamp01(currentChargeTime / chargeTime) : 0f;

    protected override void Update()
    {
        base.Update();

        if (Mouse.current == null)
            return;

        // 버튼을 놓으면 충전을 취소하고 다시 충전할 수 있게 한다.
        if (!triggerHeld)
        {
            currentChargeTime = 0f;
            charging = false;
            firedThisHold = false;
            previousTriggerHeld = false;
            return;
        }

        // 버튼을 누른 첫 프레임에 충전을 시작한다.
        if (!previousTriggerHeld)
        {
            previousTriggerHeld = true;
            firedThisHold = false;

            if (CanAttack)
            {
                charging = true;
                currentChargeTime = 0f;
            }
        }

        // 버튼을 계속 누르고 있는 동안 충전한다.
        if (!charging || firedThisHold)
            return;

        currentChargeTime += Time.deltaTime;

        // 충전 시간이 끝나면 발사한다.
        if (currentChargeTime >= chargeTime)
        {
            currentChargeTime = chargeTime;
            charging = false;
            firedThisHold = true;

            TryAttack();
        }
    }

    protected override void Attack()
    {
        if (projectilePrefab == null || gunPoint == null)
        {
            Debug.LogWarning(
                "레일건: Projectile Prefab 또는 Gun Point가 연결되지 않았습니다.",
                this
            );
            return;
        }

        Camera cam = aimCamera != null ? aimCamera : Camera.main;

        if (cam == null)
        {
            Debug.LogWarning("레일건: 조준용 카메라를 찾을 수 없습니다.", this);
            return;
        }

        Vector3 mouseScreenPosition =
            Mouse.current.position.ReadValue();

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
                "레일건 투사체에 WeaponProjectile 컴포넌트가 없습니다.",
                projectileObject
            );

            Destroy(projectileObject);
            return;
        }

        // 마지막 true는 적 관통 기능을 의미한다.
        projectile.Initialize(
            direction,
            projectileSpeed,
            damage,
            projectileLifeTime,
            true
        );
    }
}
