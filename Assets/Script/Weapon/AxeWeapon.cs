
using System.Collections.Generic;
using UnityEngine;

public class AxeWeapon : WeaponBase
{
    [Header("도끼 공격 설정")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1.2f;
    [SerializeField] private LayerMask enemyLayer;

    protected override void Attack()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("AxeWeapon: Attack Point가 연결되지 않았습니다.", this);
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer
        );

        // 적이 여러 Collider를 가지고 있어도 중복 피해 방지
        HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();

            if (enemy == null || enemy.IsDead)
                continue;

            if (damagedEnemies.Add(enemy))
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
