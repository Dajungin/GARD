
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

        currentMeleeWeapon = dagger;
        currentRangedWeapon = pistol;

        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        if (smg != null)
            smg.SetTriggerHeld(false);
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        // 카메라가 교체되거나 활성 카메라가 바뀌어도 갱신
        if (Camera.main != null)
            mainCamera = Camera.main;

        UpdateMouseDirection();
        HandleWeaponSelection();
        HandleAttackInput();
    }

    private void HandleWeaponSelection()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SelectMeleeWeapon(dagger);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SelectMeleeWeapon(axe);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SelectRangedWeapon(pistol);

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            SelectRangedWeapon(rayGun);

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
            SelectRangedWeapon(smg);
    }

    private void SelectMeleeWeapon(WeaponBase weapon)
    {
        if (weapon != null)
            currentMeleeWeapon = weapon;
    }

    private void SelectRangedWeapon(WeaponBase weapon)
    {
        if (weapon == null)
            return;

        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        if (smg != null)
            smg.SetTriggerHeld(false);

        currentRangedWeapon = weapon;
    }

    private void HandleAttackInput()
    {
        bool leftHeld = Mouse.current.leftButton.isPressed;
        bool rightPressed =
            Mouse.current.rightButton.wasPressedThisFrame;

        // 오른쪽 클릭: 근접 무기
        if (rightPressed && currentMeleeWeapon != null)
            currentMeleeWeapon.TryAttack();

        // 레일건: 충전 발사
        if (currentRangedWeapon == rayGun)
        {
            if (rayGun != null)
                rayGun.SetTriggerHeld(leftHeld);

            return;
        }

        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        // 기관단총: 연사
        if (currentRangedWeapon == smg)
        {
            if (smg != null)
                smg.SetTriggerHeld(leftHeld);

            return;
        }

        if (smg != null)
            smg.SetTriggerHeld(false);

        // 권총 등: 클릭 시 한 발 발사
        if (Mouse.current.leftButton.wasPressedThisFrame &&
            currentRangedWeapon != null)
        {
            currentRangedWeapon.TryAttack();
        }
    }

    private void UpdateMouseDirection()
    {
        if (mainCamera == null)
            return;

        Vector3 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        // 플레이어와 카메라 사이의 깊이를 설정
        mouseScreenPosition.z = Mathf.Abs(
            mainCamera.transform.position.z - transform.position.z
        );

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        float directionX =
            mouseWorldPosition.x - transform.position.x;

        // 플레이어와 마우스가 거의 같은 X 위치면 방향 유지
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

        // 캐릭터 스프라이트 좌우 반전
        if (playerSprite != null)
            playerSprite.flipX = !faceRight;

        // 무기와 총구도 캐릭터 방향에 맞게 반전
        if (weaponRoot != null)
        {
            Vector3 scale = weaponRoot.localScale;

            scale.x = Mathf.Abs(scale.x) *
                      (faceRight ? 1f : -1f);

            weaponRoot.localScale = scale;
        }
    }

    private void OnDisable()
    {
        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        if (smg != null)
            smg.SetTriggerHeld(false);
    }
}
