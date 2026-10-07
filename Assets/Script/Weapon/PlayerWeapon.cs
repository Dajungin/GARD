using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviour
{
    [Header("무기")]
    [SerializeField] private WeaponBase dagger;
    [SerializeField] private WeaponBase pistol;

    [Header("플레이어 방향")]
    [SerializeField] private SpriteRenderer playerSprite;

    [Header("무기 위치")]
    [SerializeField] private Transform weaponRoot;

    private bool facingRight = true;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        UpdateMouseDirection();

        // 왼쪽 클릭 → 권총
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (pistol != null)
            {
                pistol.TryAttack();
            }
        }

        // 오른쪽 클릭 → 단검
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (dagger != null)
            {
                dagger.TryAttack();
            }
        }
    }

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
            mouseWorldPosition.x -
            transform.position.x;

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
            Vector3 scale =
                weaponRoot.localScale;

            scale.x =
                Mathf.Abs(scale.x) *
                (faceRight ? 1f : -1f);

            weaponRoot.localScale = scale;
        }
    }
}