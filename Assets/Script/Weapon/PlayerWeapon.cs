
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviour
{
    [Header("애니메이션")]
    [SerializeField] private Animator animator;

    [Header("근접 무기")]
    [SerializeField] private WeaponBase dagger;
    [SerializeField] private WeaponBase axe;

    [Header("원거리 무기")]
    [SerializeField] private WeaponBase pistol;
    [SerializeField] private RayGunWeapon rayGun;
    [SerializeField] private SMGWeapon smg;
    [SerializeField] private ShotgunWeapon shotgun;

    [Header("플레이어 방향")]
    [SerializeField] private SpriteRenderer playerSprite;

    [Header("무기 위치")]
    [SerializeField] private Transform weaponRoot;

    private WeaponBase currentMeleeWeapon;
    private WeaponBase currentRangedWeapon;

    private bool facingRight = true;
    private Camera mainCamera;

    // Animator 파라미터
    private static readonly int WeaponTypeHash =
        Animator.StringToHash("WeaponType");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private void Awake()
    {
        RefreshCamera();

        // 기본 무기 설정
        currentMeleeWeapon = dagger;
        currentRangedWeapon = pistol;

        // Animator 자동 연결
        if (animator == null)
            animator = GetComponent<Animator>();

        // 기본 무기는 권총
        SetWeaponType(0);

        // 레일건 충전 입력 초기화
        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        // 기관단총 연사 입력 초기화
        if (smg != null)
            smg.SetTriggerHeld(false);
    }

    private void OnEnable()
    {
        RefreshCamera();
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

    // 카메라 확인
    private void RefreshCamera()
    {
        if (mainCamera == null || !mainCamera.isActiveAndEnabled)
            mainCamera = Camera.main;
    }

    // =====================================
    // 무기 선택
    // =====================================

    private void HandleWeaponSelection()
    {
        if (Keyboard.current == null)
            return;

        // 1번: 단검
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SelectMeleeWeapon(dagger);

        // 2번: 도끼
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SelectMeleeWeapon(axe);

        // 3번: 권총
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SelectRangedWeapon(pistol);

        // 4번: 레일건
        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            SelectRangedWeapon(rayGun);

        // 5번: 기관단총
        if (Keyboard.current.digit5Key.wasPressedThisFrame)
            SelectRangedWeapon(smg);

        // 6번: 샷건
        if (Keyboard.current.digit6Key.wasPressedThisFrame)
            SelectRangedWeapon(shotgun);
    }

    // 근접 무기 선택
    private void SelectMeleeWeapon(WeaponBase weapon)
    {
        if (weapon == null)
            return;

        // 이전 원거리 무기의 충전 및 연사 해제
        StopSpecialWeapons();

        currentMeleeWeapon = weapon;

        if (weapon == dagger)
            SetWeaponType(4);
        else if (weapon == axe)
            SetWeaponType(5);

        Debug.Log("근접 무기 선택: " + weapon.name);
    }

    // 원거리 무기 선택
    private void SelectRangedWeapon(WeaponBase weapon)
    {
        if (weapon == null)
            return;

        StopSpecialWeapons();

        currentRangedWeapon = weapon;

        if (weapon == pistol)
            SetWeaponType(0);
        else if (weapon == smg)
            SetWeaponType(1);
        else if (weapon == shotgun)
            SetWeaponType(2);
        else if (weapon == rayGun)
            SetWeaponType(3);

        Debug.Log("원거리 무기 선택: " + weapon.name);
    }

    // 레일건 충전과 기관단총 연사 중지
    private void StopSpecialWeapons()
    {
        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        if (smg != null)
            smg.SetTriggerHeld(false);
    }

    // =====================================
    // Animator 제어
    // =====================================

    private void SetWeaponType(int weaponType)
    {
        if (animator != null)
            animator.SetInteger(WeaponTypeHash, weaponType);
    }

    private void PlayAttackAnimation(int weaponType)
    {
        if (animator == null)
            return;

        animator.SetInteger(WeaponTypeHash, weaponType);

        animator.ResetTrigger(AttackHash);
        animator.SetTrigger(AttackHash);
    }

    // =====================================
    // 공격 입력
    // =====================================

    private void HandleAttackInput()
    {
        bool leftHeld =
            Mouse.current.leftButton.isPressed;

        bool leftPressed =
            Mouse.current.leftButton.wasPressedThisFrame;

        bool rightPressed =
            Mouse.current.rightButton.wasPressedThisFrame;

        // -----------------------------
        // 오른쪽 클릭: 근접 무기 공격
        // -----------------------------

        if (rightPressed && currentMeleeWeapon != null)
        {
            if (currentMeleeWeapon.CanAttack)
            {
                if (currentMeleeWeapon == dagger)
                    PlayAttackAnimation(4);

                else if (currentMeleeWeapon == axe)
                    PlayAttackAnimation(5);

                currentMeleeWeapon.TryAttack();
            }
        }

        // -----------------------------
        // 4번: 레일건
        // -----------------------------

        if (currentRangedWeapon == rayGun)
        {
            if (rayGun != null)
                rayGun.SetTriggerHeld(leftHeld);

            return;
        }

        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        // -----------------------------
        // 5번: 기관단총
        // -----------------------------

        if (currentRangedWeapon == smg)
        {
            if (smg != null)
                smg.SetTriggerHeld(leftHeld);

            return;
        }

        if (smg != null)
            smg.SetTriggerHeld(false);

        // -----------------------------
        // 3번: 권총
        // -----------------------------

        if (currentRangedWeapon == pistol && leftPressed)
        {
            if (pistol != null && pistol.CanAttack)
            {
                PlayAttackAnimation(0);
                pistol.TryAttack();
            }

            return;
        }

        // -----------------------------
        // 6번: 샷건
        // -----------------------------

        if (currentRangedWeapon == shotgun && leftPressed)
        {
            if (shotgun != null && shotgun.CanAttack)
            {
                PlayAttackAnimation(2);
                shotgun.TryAttack();
            }
        }
    }

    // =====================================
    // 마우스 방향 전환
    // =====================================

    private void UpdateMouseDirection()
    {
        if (mainCamera == null || playerSprite == null)
            return;

        Vector2 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        float depth = Mathf.Abs(
            mainCamera.transform.position.z -
            transform.position.z
        );

        Vector3 screenPosition = new Vector3(
            mouseScreenPosition.x,
            mouseScreenPosition.y,
            depth
        );

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(screenPosition);

        float directionX =
            mouseWorldPosition.x - transform.position.x;

        if (directionX > 0.01f)
            SetFacingDirection(true);

        else if (directionX < -0.01f)
            SetFacingDirection(false);
    }

    private void SetFacingDirection(bool faceRight)
    {
        if (facingRight == faceRight)
            return;

        facingRight = faceRight;

        // 플레이어 스프라이트 좌우 반전
        if (playerSprite != null)
            playerSprite.flipX = !faceRight;

        // 무기 위치 좌우 반전
        if (weaponRoot != null)
        {
            Vector3 scale = weaponRoot.localScale;

            scale.x = Mathf.Abs(scale.x) *
                      (faceRight ? 1f : -1f);

            weaponRoot.localScale = scale;
        }
    }

    // =====================================
    // 비활성화 처리
    // =====================================

    private void OnDisable()
    {
        StopSpecialWeapons();
    }
}
