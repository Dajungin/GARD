using UnityEngine;
using UnityEngine.InputSystem;

public class GunWeapon : WeaponBase
{
    [Header("권총 설정")]
    [SerializeField] private GameObject bulletPrefab;

    [SerializeField]
    private Transform gunPoint;

    [SerializeField]
    private float bulletSpeed = 15f;

    [SerializeField]
    private float bulletLifeTime = 3f;

    protected override void Attack()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning(
                "Bullet Prefab이 연결되지 않았습니다."
            );

            return;
        }

        if (gunPoint == null)
        {
            Debug.LogWarning(
                "Gun Point가 연결되지 않았습니다."
            );

            return;
        }

        if (Mouse.current == null)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(
                mouseScreenPosition
            );

        mouseWorldPosition.z = 0f;

        Vector2 direction =
            mouseWorldPosition -
            gunPoint.position;

        direction.Normalize();

        GameObject bulletObject =
            Instantiate(
                bulletPrefab,
                gunPoint.position,
                Quaternion.identity
            );

        Bullet bullet =
            bulletObject.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.SetDirection(direction);
            bullet.SetSpeed(bulletSpeed);
            bullet.SetLifeTime(bulletLifeTime);
            bullet.SetDamage(damage);
        }
    }
}