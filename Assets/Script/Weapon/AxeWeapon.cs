
using System.Collections.Generic;
using UnityEngine;

public class AxeWeapon : WeaponBase
{
    [Header("도끼 공격 설정")]
    [SerializeField] private Transform attackPoint;
    [SerializeField, Min(0.01f)] private float attackRadius = 1.2f;
    [SerializeField] private LayerMask enemyLayer;

    protected override void Attack()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning(
                "AxeWeapon: Attack Point가 연결되지 않았습니다.",
                this
            );
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer
        );

        HashSet<EnemyHealth> damagedEnemies =
            new HashSet<EnemyHealth>();

        HashSet<BossHealth> damagedBosses =
            new HashSet<BossHealth>();

        foreach (Collider2D hit in hits)
        {
            // 먼저 보스인지 확인
            BossHealth boss =
                hit.GetComponentInParent<BossHealth>();

            if (boss != null)
            {
                if (boss.IsDead || !damagedBosses.Add(boss))
                    continue;

                boss.TakeDamage(damage);

                Debug.Log(
                    $"도끼가 보스를 공격했습니다! 피해량: {damage}",
                    boss
                );

                continue;
            }

            // 일반 몬스터 확인
            EnemyHealth enemy =
                hit.GetComponentInParent<EnemyHealth>();

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
        Gizmos.DrawWireSphere(
            attackPoint.position,
            attackRadius
        );
    }
}
