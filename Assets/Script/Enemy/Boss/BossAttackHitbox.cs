
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossAttackHitbox : MonoBehaviour
{
    [Header("돌진 공격 설정")]
    [SerializeField, Min(0)] private int damage = 1;

    private Collider2D attackCollider;
    private readonly HashSet<PlayerHealth> hitPlayers =
        new HashSet<PlayerHealth>();

    private void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        attackCollider.isTrigger = true;
        attackCollider.enabled = false;
    }

    public void SetAttackActive(bool active)
    {
        if (attackCollider == null)
            return;

        if (active)
        {
            // 새로운 공격이 시작될 때 피격 기록 초기화
            hitPlayers.Clear();
            attackCollider.enabled = true;
        }
        else
        {
            attackCollider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!attackCollider.enabled)
            return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player == null || hitPlayers.Contains(player))
            return;

        hitPlayers.Add(player);
        player.TakeDamage(damage);
    }
}
