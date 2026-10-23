
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
        RefreshCamera();

        currentMeleeWeapon = dagger;
        currentRangedWeapon = pistol;

        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        if (smg != null)
            smg.SetTriggerHeld(false);
    }

    private void OnEnable()
    {
        RefreshCamera();
    }

    private void Update()
    {
        // 씬 전환으로 카메라가 교체되었으면 다시 찾음
        RefreshCamera();

        if (Mouse.current == null)
            return;

        UpdateMouseDirection();
        HandleWeaponSelection();
        HandleAttackInput();
    }

    private void RefreshCamera()
    {
        if (mainCamera == null || !mainCamera.isActiveAndEnabled)
            mainCamera = Camera.main;
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
        currentMeleeWeapon = weapon;
    }

    private void SelectRangedWeapon(WeaponBase weapon)
    {
        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        if (smg != null)
            smg.SetTriggerHeld(false);

        currentRangedWeapon = weapon;
    }

    private void HandleAttackInput()
    {
        bool leftHeld = Mouse.current.leftButton.isPressed;
        bool rightPressed = Mouse.current.rightButton.wasPressedThisFrame;

        // 오른쪽 클릭: 근접 공격
        if (rightPressed && currentMeleeWeapon != null)
            currentMeleeWeapon.TryAttack();

        // 레일건 충전
        if (currentRangedWeapon == rayGun)
        {
            if (rayGun != null)
                rayGun.SetTriggerHeld(leftHeld);

            return;
        }

        if (rayGun != null)
            rayGun.SetTriggerHeld(false);

        // 기관단총 연사
        if (currentRangedWeapon == smg)
        {
            if (smg != null)
                smg.SetTriggerHeld(leftHeld);

            return;
        }

        if (smg != null)
            smg.SetTriggerHeld(false);

        // 권총 등 일반 원거리 공격
        if (Mouse.current.leftButton.wasPressedThisFrame &&
            currentRangedWeapon != null)
        {
            currentRangedWeapon.TryAttack();
        }
    }

    private void UpdateMouseDirection()
    {
        if (mainCamera == null || playerSprite == null)
            return;

        Vector2 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        // 카메라와 플레이어 사이의 깊이를 지정하여 월드 좌표로 변환
        float depth = Mathf.Abs(
            mainCamera.transform.position.z - transform.position.z
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

        // 원본 스프라이트가 오른쪽을 바라보는 기준
        if (playerSprite != null)
            playerSprite.flipX = !faceRight;

        // 무기 위치도 플레이어 방향에 맞춰 반전
        if (weaponRoot != null)
        {
            Vector3 scale = weaponRoot.localScale;

            scale.x = Mathf.Abs(scale.x) * (faceRight ? 1f : -1f);

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
