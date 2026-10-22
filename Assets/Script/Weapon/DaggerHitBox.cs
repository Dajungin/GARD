
using System.Collections.Generic;
using UnityEngine;

public class DaggerHitBox : MonoBehaviour
{
    [Header("단검 공격 설정")]
    [SerializeField, Min(0)] private int damage = 20;

    // 한 번의 휘두르기에서 이미 맞은 대상 기록
    private readonly HashSet<BossHealth> hitBosses = new();
    private readonly HashSet<EnemyHealth> hitEnemies = new();

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    private void OnEnable()
    {
        // 단검을 새로 휘두를 때마다 초기화
        hitBosses.Clear();
        hitEnemies.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (damage <= 0)
            return;

        // 보스 본체 또는 부모 오브젝트에서 BossHealth 찾기
        BossHealth boss =
            collision.GetComponentInParent<BossHealth>();

        if (boss != null)
        {
            // 같은 휘두르기에서 보스에게 중복 피해 방지
            if (!hitBosses.Add(boss))
                return;

            Debug.Log($"단검이 보스에게 적중! 피해량: {damage}");

            boss.TakeDamage(damage);
            return;
        }

        // 일반 적 본체 또는 부모 오브젝트에서 EnemyHealth 찾기
        EnemyHealth enemy =
            collision.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            // 같은 휘두르기에서 일반 적에게 중복 피해 방지
            if (!hitEnemies.Add(enemy))
                return;

            Debug.Log($"단검이 적에게 적중! 피해량: {damage}");

            enemy.TakeDamage(damage);
        }
    }
}
