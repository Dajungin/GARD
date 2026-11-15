
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviour
{
    [Header("무기 설정")]
    [SerializeField] private WeaponBase dagger;
    [SerializeField] private WeaponBase axe;
    [SerializeField] private WeaponBase pistol;
    [SerializeField] private RayGunWeapon rayGun;
    [SerializeField] private SMGWeapon smg;
    [SerializeField] private ShotgunWeapon shotgun;

    [Header("플레이어 설정")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Transform weaponRoot;
    [SerializeField] private Animator animator;

    [Header("카메라")]
    [SerializeField] private Camera aimCamera;

    [Header("Animator 무기 번호")]
    [SerializeField] private int pistolAnimationType = 0;
    [SerializeField] private int smgAnimationType = 1;
    [SerializeField] private int shotgunAnimationType = 2;
    [SerializeField] private int railGunAnimationType = 3;
    [SerializeField] private int daggerAnimationType = 4;
    [SerializeField] private int axeAnimationType = 5;

    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;

    private WeaponBase currentMeleeWeapon;

    private enum WeaponMode
    {
        Dagger,
        Axe,
        Pistol,
        RailGun,
        SMG,
        Shotgun
    }

    private WeaponMode currentWeaponMode = WeaponMode.Dagger;

    private static readonly int WeaponTypeHash =
        Animator.StringToHash("WeaponType");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private static readonly int IsSMGFiringHash =
        Animator.StringToHash("IsSMGFiring");

    private bool animatorReady;

    private void Awake()
    {
        FindAnimator();

        if (aimCamera == null)
            aimCamera = Camera.main;

        currentMeleeWeapon = dagger;

        ValidateAnimator();

        SetWeaponMode(WeaponMode.Dagger);

        StopSpecialWeapons();
    }

    private void Update()
    {
        RefreshCamera();

        if (Mouse.current == null)
            return;

        UpdateMouseDirection();
        HandleWeaponSelection();
        HandleAttackInput();
    }

    // =====================================
    // Animator 찾기
    // =====================================

    private void FindAnimator()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (animator == null)
            animator = GetComponentInParent<Animator>();

        if (animator == null)
        {
            animatorReady = false;

            Debug.LogError(
                "PlayerWeapon: 플레이어 Animator를 찾지 못했습니다. " +
                "Inspector의 Animator 슬롯에 직접 연결해주세요.",
                this
            );

            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            animatorReady = false;

            Debug.LogError(
                "PlayerWeapon: Animator Controller가 연결되지 않았습니다!",
                animator
            );

            return;
        }

        animatorReady = true;

        if (showDebugLogs)
        {
            Debug.Log(
                "Animator 연결 성공: " +
                animator.gameObject.name +
                " / Controller: " +
                animator.runtimeAnimatorController.name,
                animator
            );
        }
    }

    private void ValidateAnimator()
    {
        if (!animatorReady)
            return;

        bool hasWeaponType = false;
        bool hasAttack = false;
        bool hasSMG = false;

        foreach (AnimatorControllerParameter parameter
                 in animator.parameters)
        {
            if (parameter.nameHash == WeaponTypeHash &&
                parameter.type == AnimatorControllerParameterType.Int)
            {
                hasWeaponType = true;
            }

            if (parameter.nameHash == AttackHash &&
                parameter.type == AnimatorControllerParameterType.Trigger)
            {
                hasAttack = true;
            }

            if (parameter.nameHash == IsSMGFiringHash &&
                parameter.type == AnimatorControllerParameterType.Bool)
            {
                hasSMG = true;
            }
        }

        if (!hasWeaponType)
            Debug.LogError("Animator에 WeaponType (Int)가 없습니다!", animator);

        if (!hasAttack)
            Debug.LogError("Animator에 Attack (Trigger)가 없습니다!", animator);

        if (!hasSMG)
            Debug.LogWarning("Animator에 IsSMGFiring (Bool)가 없습니다. SMG 애니메이션을 사용할 경우 추가해주세요.", animator);

        if (hasWeaponType && hasAttack && showDebugLogs)
            Debug.Log("Animator 필수 파라미터 확인 완료!", animator);
    }

    private void RefreshCamera()
    {
        if (aimCamera == null)
            aimCamera = Camera.main;
    }

    // =====================================
    // 무기 선택
    // =====================================

    private void HandleWeaponSelection()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SetWeaponMode(WeaponMode.Dagger);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SetWeaponMode(WeaponMode.Axe);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SetWeaponMode(WeaponMode.Pistol);

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            SetWeaponMode(WeaponMode.RailGun);

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
            SetWeaponMode(WeaponMode.SMG);

        if (Keyboard.current.digit6Key.wasPressedThisFrame)
            SetWeaponMode(WeaponMode.Shotgun);
    }

    private void SetWeaponMode(WeaponMode mode)
    {
        StopSpecialWeapons();

        currentWeaponMode = mode;

        int animationType = 0;

        switch (mode)
        {
            case WeaponMode.Dagger:
                currentMeleeWeapon = dagger;
                animationType = daggerAnimationType;
                break;

            case WeaponMode.Axe:
                currentMeleeWeapon = axe;
                animationType = axeAnimationType;
                break;

            case WeaponMode.Pistol:
                currentMeleeWeapon = null;
                animationType = pistolAnimationType;
                break;

            case WeaponMode.RailGun:
                currentMeleeWeapon = null;
                animationType = railGunAnimationType;
                break;

            case WeaponMode.SMG:
                currentMeleeWeapon = null;
                animationType = smgAnimationType;
                break;

            case WeaponMode.Shotgun:
                currentMeleeWeapon = null;
                animationType = shotgunAnimationType;
                break;
        }

        SetAnimatorWeaponType(animationType);

        if (showDebugLogs)
            Debug.Log("무기 선택: " + mode + " / WeaponType: " + animationType, this);
    }

    // =====================================
    // Animator 파라미터 제어
    // =====================================

    private void SetAnimatorWeaponType(int weaponType)
    {
        if (!animatorReady)
            return;

        animator.SetInteger(WeaponTypeHash, weaponType);
    }

    private void PlayAttackAnimation(int weaponType)
    {
        if (!animatorReady)
        {
            Debug.LogError(
                "공격 애니메이션 실패: Animator가 준비되지 않았습니다.",
                this
            );
            return;
        }

        animator.SetInteger(WeaponTypeHash, weaponType);
        animator.ResetTrigger(AttackHash);
        animator.SetTrigger(AttackHash);

        if (showDebugLogs)
        {
            Debug.Log(
                "공격 애니메이션 요청 / WeaponType: " + weaponType +
                " / 현재 Animator 상태: " +
                animator.GetCurrentAnimatorStateInfo(0).shortNameHash,
                animator
            );
        }
    }

    // =====================================
    // 공격 입력
    // =====================================

    private void HandleAttackInput()
    {
        bool leftPressed =
            Mouse.current.leftButton.wasPressedThisFrame;

        bool leftHeld =
            Mouse.current.leftButton.isPressed;

        bool leftReleased =
            Mouse.current.leftButton.wasReleasedThisFrame;

        bool rightPressed =
            Mouse.current.rightButton.wasPressedThisFrame;

        // 근접 무기: 오른쪽 클릭
        if (rightPressed)
            HandleMeleeAttack();

        // 원거리 무기: 왼쪽 클릭
        switch (currentWeaponMode)
        {
            case WeaponMode.Pistol:

                if (leftPressed)
                {
                    PlayAttackAnimation(pistolAnimationType);

                    if (pistol != null)
                        pistol.TryAttack();
                }
                break;

            case WeaponMode.Shotgun:

                if (leftPressed)
                {
                    PlayAttackAnimation(shotgunAnimationType);

                    if (shotgun != null)
                        shotgun.TryAttack();
                }
                break;

            case WeaponMode.RailGun:

                if (rayGun != null)
                {
                    if (leftPressed || leftHeld)
                        rayGun.SetTriggerHeld(leftHeld);

                    if (leftReleased)
                        rayGun.SetTriggerHeld(false);
                }
                break;

            case WeaponMode.SMG:

                if (smg != null)
                {
                    if (leftPressed || leftHeld)
                        smg.SetTriggerHeld(leftHeld);

                    if (leftReleased)
                        smg.SetTriggerHeld(false);
                }
                break;
        }
    }

    // =====================================
    // 근접 공격
    // =====================================

    private void HandleMeleeAttack()
    {
        if (currentMeleeWeapon == null)
            return;

        int animationType =
            currentWeaponMode == WeaponMode.Dagger
                ? daggerAnimationType
                : axeAnimationType;

        PlayAttackAnimation(animationType);

        currentMeleeWeapon.TryAttack();
    }

    // =====================================
    // 마우스 방향
    // =====================================

    private void UpdateMouseDirection()
    {
        if (playerSprite == null || aimCamera == null)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        float depth = Mathf.Abs(
            aimCamera.transform.position.z - transform.position.z
        );

        Vector3 mouseWorldPosition =
            aimCamera.ScreenToWorldPoint(
                new Vector3(
                    mousePosition.x,
                    mousePosition.y,
                    depth
                )
            );

        bool mouseOnRight =
            mouseWorldPosition.x >= transform.position.x;

        playerSprite.flipX = !mouseOnRight;

        if (weaponRoot != null)
        {
            Vector3 scale = weaponRoot.localScale;

            scale.x = Mathf.Abs(scale.x) *
                (mouseOnRight ? 1f : -1f);

            weaponRoot.localScale = scale;
        }
    }

    // =====================================
    // 특수 무기 초기화
    // =====================================

    private void StopSpecialWeapons()
    {
        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        if (smg != null)
            smg.SetTriggerHeld(false);
    }

    private void OnDisable()
    {
        StopSpecialWeapons();
    }
}
