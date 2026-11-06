
using UnityEngine;
using UnityEngine.InputSystem;

public class SMGWeapon : WeaponBase
{
    [Header("기관단총 설정")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform gunPoint;
    [SerializeField] private float projectileSpeed = 16f;
    [SerializeField] private float projectileLifeTime = 2f;

    [Header("애니메이션")]
    [SerializeField] private Animator playerAnimator;

    private bool isFiring;

    private static readonly int IsSMGFiringHash =
        Animator.StringToHash("IsSMGFiring");

    private bool IsSMGFiring
    {
        set
        {
            if (playerAnimator != null)
                playerAnimator.SetBool(IsSMGFiringHash, value);
        }
    }

    protected override void Update()
    {
        base.Update();

        // 누르고 있는 동안 연사
        if (triggerHeld && CanAttack)
        {
            TryAttack();
        }
    }

    public override void SetTriggerHeld(bool held)
    {
        triggerHeld = held;

        // 버튼을 누르면 연사 애니메이션 시작
        // 버튼을 놓으면 연사 애니메이션 종료
        if (isFiring != held)
        {
            isFiring = held;
            IsSMGFiring = held;
        }
    }

    protected override void Attack()
    {
        if (projectilePrefab == null || gunPoint == null)
        {
            Debug.LogWarning(
                "SMGWeapon: Projectile Prefab 또는 Gun Point가 없습니다.",
                this
            );
            return;
        }

        if (Mouse.current == null || Camera.main == null)
            return;

        Vector3 mousePosition = Mouse.current.position.ReadValue();

        float depth = Mathf.Abs(
            Camera.main.transform.position.z - gunPoint.position.z
        );

        mousePosition.z = depth;

        Vector3 mouseWorldPosition =
            Camera.main.ScreenToWorldPoint(mousePosition);

        mouseWorldPosition.z = 0f;

        Vector2 direction =
            (Vector2)(mouseWorldPosition - gunPoint.position);

        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();

        GameObject projectile = Instantiate(
            projectilePrefab,
            gunPoint.position,
            Quaternion.identity
        );

        WeaponProjectile projectileScript =
            projectile.GetComponent<WeaponProjectile>();

        if (projectileScript == null)
        {
            Debug.LogError(
                "기관단총 투사체에 WeaponProjectile이 없습니다.",
                projectile
            );

            Destroy(projectile);
            return;
        }

        projectileScript.Initialize(
            direction,
            projectileSpeed,
            damage,
            projectileLifeTime
        );
    }

    private void OnDisable()
    {
        triggerHeld = false;
        isFiring = false;
        IsSMGFiring = false;
    }
}
