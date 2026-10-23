
using UnityEngine;
using UnityEngine.InputSystem;

public class ShotgunWeapon : WeaponBase
{
    [Header("샷건 설정")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform gunPoint;

    [Header("탄환 설정")]
    [SerializeField, Min(1)] private int pelletCount = 3;
    [SerializeField, Min(0f)] private float spreadAngle = 20f;
    [SerializeField, Min(0f)] private float projectileSpeed = 14f;
    [SerializeField, Min(0.1f)] private float projectileLifeTime = 2f;

    [Header("거리별 피해량")]
    [SerializeField, Min(0)] private int closeDamage = 30;
    [SerializeField, Min(0)] private int farDamage = 10;
    [SerializeField, Min(0f)] private float closeDistance = 2f;
    [SerializeField, Min(0.1f)] private float farDistance = 8f;

    [Header("조준 카메라")]
    [SerializeField] private Camera aimCamera;

    private void Awake()
    {
        if (aimCamera == null)
            aimCamera = Camera.main;
    }


    protected override void Attack()
    {
        if (projectilePrefab == null || gunPoint == null)
        {
            Debug.LogWarning(
                "ShotgunWeapon: Projectile Prefab 또는 Gun Point가 없습니다.",
                this
            );
            return;
        }

        if (Mouse.current == null)
            return;

        if (aimCamera == null)
            aimCamera = Camera.main;

        if (aimCamera == null)
        {
            Debug.LogWarning("ShotgunWeapon: 카메라를 찾지 못했습니다.", this);
            return;
        }

        // New Input System으로 마우스 위치 가져오기
        Vector3 mousePosition = Mouse.current.position.ReadValue();

        float depth = Mathf.Abs(
            aimCamera.transform.position.z - gunPoint.position.z
        );

        mousePosition.z = depth;

        Vector3 mouseWorldPosition =
            aimCamera.ScreenToWorldPoint(mousePosition);

        Vector2 direction =
            (Vector2)(mouseWorldPosition - gunPoint.position);

        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();

        // 탄환 3발을 부채꼴로 발사
        for (int i = 0; i < pelletCount; i++)
        {
            float angle = pelletCount == 1
                ? 0f
                : Mathf.Lerp(
                    -spreadAngle * 0.5f,
                    spreadAngle * 0.5f,
                    i / (float)(pelletCount - 1)
                );

            Vector2 pelletDirection = RotateDirection(direction, angle);

            GameObject pellet = Instantiate(
                projectilePrefab,
                gunPoint.position,
                Quaternion.identity
            );

            ShotgunPellet pelletScript =
                pellet.GetComponent<ShotgunPellet>();

            if (pelletScript == null)
            {
                Debug.LogError(
                    "샷건 탄환 프리팹에 ShotgunPellet이 없습니다.",
                    pellet
                );

                Destroy(pellet);
                continue;
            }

            pelletScript.Initialize(
                pelletDirection,
                projectileSpeed,
                projectileLifeTime,
                closeDamage,
                farDamage,
                closeDistance,
                farDistance,
                gunPoint.position
            );
        }
    }

    private Vector2 RotateDirection(Vector2 direction, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;

        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        ).normalized;
    }
}
