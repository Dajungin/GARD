
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviour
{
    [Header("근접 무기")]
    [SerializeField] private WeaponBase dagger;
    [SerializeField] private WeaponBase axe;

    [Header("원거리 무기")]
    [SerializeField] private WeaponBase pistol;
    [SerializeField] private RayGunWeapon rayGun;
    [SerializeField] private SMGWeapon smg;

    [Header("플레이어 방향")]
    [SerializeField] private SpriteRenderer playerSprite;

    [Header("무기 위치")]
    [SerializeField] private Transform weaponRoot;

    private WeaponBase currentMeleeWeapon;
    private WeaponBase currentRangedWeapon;

    private bool facingRight = true;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        // 시작할 때 기본 무기 선택
        currentMeleeWeapon = dagger;
        currentRangedWeapon = pistol;

        // 레일건과 기관단총의 입력 초기화
        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        if (smg != null)
            smg.SetTriggerHeld(false);
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        UpdateMouseDirection();
        HandleWeaponSelection();
        HandleAttackInput();
    }

    // 숫자키로 무기 선택
    private void HandleWeaponSelection()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SelectMeleeWeapon(dagger);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SelectMeleeWeapon(axe);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            SelectRangedWeapon(pistol);
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            SelectRangedWeapon(rayGun);
        }

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            SelectRangedWeapon(smg);
        }
    }

    // 근접 무기 변경
    private void SelectMeleeWeapon(WeaponBase weapon)
    {
        currentMeleeWeapon = weapon;
    }

    // 원거리 무기 변경
    private void SelectRangedWeapon(WeaponBase weapon)
    {
        // 무기 변경 전에 레일건 충전 취소
        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        // 기관단총 연사도 중단
        if (smg != null)
            smg.SetTriggerHeld(false);

        currentRangedWeapon = weapon;
    }

    // 마우스 입력 처리
    private void HandleAttackInput()
    {
        bool leftHeld = Mouse.current.leftButton.isPressed;
        bool rightPressed =
            Mouse.current.rightButton.wasPressedThisFrame;

        // 오른쪽 클릭: 현재 근접 무기
        if (rightPressed && currentMeleeWeapon != null)
        {
            currentMeleeWeapon.TryAttack();
        }

        // 레일건: 버튼을 누르고 있는 동안 충전
        if (currentRangedWeapon == rayGun)
        {
            if (rayGun != null)
                rayGun.SetTriggerHeld(leftHeld);

            return;
        }

        // 레일건을 사용하지 않을 때는 충전 입력 해제
        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        // 기관단총: 버튼을 누르고 있는 동안 연사
        if (currentRangedWeapon == smg)
        {
            if (smg != null)
                smg.SetTriggerHeld(leftHeld);

            return;
        }

        // 기관단총을 사용하지 않을 때는 연사 입력 해제
        if (smg != null)
            smg.SetTriggerHeld(false);

        // 권총 등 일반 원거리 무기: 클릭 순간 한 발 발사
        if (Mouse.current.leftButton.wasPressedThisFrame &&
            currentRangedWeapon != null)
        {
            currentRangedWeapon.TryAttack();
        }
    }

    // 마우스 위치에 따라 플레이어 방향 변경
    private void UpdateMouseDirection()
    {
        if (mainCamera == null)
            return;

        Vector3 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(
                mouseScreenPosition
            );

        float directionX =
            mouseWorldPosition.x - transform.position.x;

        if (directionX > 0f)
        {
            SetFacingDirection(true);
        }
        else if (directionX < 0f)
        {
            SetFacingDirection(false);
        }
    }

    private void SetFacingDirection(bool faceRight)
    {
        if (facingRight == faceRight)
            return;

        facingRight = faceRight;

        if (playerSprite != null)
        {
            playerSprite.flipX = !faceRight;
        }

        if (weaponRoot != null)
        {
            Vector3 scale = weaponRoot.localScale;

            scale.x =
                Mathf.Abs(scale.x) *
                (faceRight ? 1f : -1f);

            weaponRoot.localScale = scale;
        }
    }

    private void OnDisable()
    {
        // 오브젝트 비활성화 시 충전과 연사 입력 해제
        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        if (smg != null)
            smg.SetTriggerHeld(false);
    }
}
