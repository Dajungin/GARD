
using UnityEngine;

public class FlyingEnemyAttack : MonoBehaviour
{
    [Header("공격 설정")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [SerializeField] private int damage = 1;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float projectileLifeTime = 4f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("발사 방향")]
    [SerializeField] private bool aimAtCurrentPlayerPosition = true;

    private float cooldownTimer;

    public bool IsReady => cooldownTimer <= 0f;

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public bool TryFireAt(Transform target)
    {
        if (!IsReady || target == null)
            return false;

        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning(
                "FlyingEnemyAttack: 투사체 프리팹 또는 FirePoint가 연결되지 않았습니다.",
                this
            );

            return false;
        }

        Vector2 direction;

        if (aimAtCurrentPlayerPosition)
        {
            direction = (Vector2)(target.position - firePoint.position);
        }
        else
        {
            direction = Vector2.right;
        }

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        direction.Normalize();

        GameObject projectileObject = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        EnemyProjectile projectile =
            projectileObject.GetComponent<EnemyProjectile>();

        if (projectile == null)
        {
            Debug.LogError(
                "투사체 프리팹에 EnemyProjectile 컴포넌트가 없습니다.",
                projectileObject
            );

            Destroy(projectileObject);
            return false;
        }

        projectile.Initialize(
            direction,
            projectileSpeed,
            damage,
            projectileLifeTime
        );

        cooldownTimer = Mathf.Max(0.01f, attackCooldown);

        return true;
    }
}
