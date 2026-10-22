
using UnityEngine;

public static class BossDamageHelper
{
    public static bool TryDamageBoss(
        Collider2D hitCollider,
        int damage,
        GameObject attacker)
    {
        if (hitCollider == null || damage <= 0)
            return false;

        // 충돌한 오브젝트 또는 부모에서 보스 체력 찾기
        BossHealth boss = hitCollider.GetComponentInParent<BossHealth>();

        if (boss == null)
        {
            Debug.LogWarning(
                $"공격 대상 '{hitCollider.name}'에서 BossHealth를 찾지 못했습니다."
            );
            return false;
        }

        Debug.Log(
            $"보스 공격 판정 성공! 공격자: {attacker.name}, 피해량: {damage}"
        );

        boss.TakeDamage(damage);
        return true;
    }
}
