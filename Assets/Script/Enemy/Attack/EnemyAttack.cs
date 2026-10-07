using UnityEngine;

public abstract class EnemyAttack : MonoBehaviour
{
    [Header("공격 기본 설정")]
    [SerializeField] protected float attackRange = 4f;

    [SerializeField] protected float attackCooldown = 2f;

    protected float cooldownTimer;

    public float AttackRange => attackRange;

    public bool CanAttack =>
        cooldownTimer <= 0f;

    public bool IsAttacking { get; protected set; }

    protected virtual void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    // =========================================================
    // 공격 요청
    // =========================================================

    public bool TryAttack(Transform target)
    {
        if (!CanAttack)
            return false;

        if (target == null)
            return false;

        cooldownTimer = attackCooldown;

        Attack(target);

        return true;
    }

    // =========================================================
    // 실제 공격
    // =========================================================

    protected abstract void Attack(Transform target);
}