using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("무기 기본 설정")]
    [SerializeField] protected int damage = 20;

    [SerializeField]
    protected float cooldown = 0.5f;

    protected float cooldownTimer;

    public bool CanAttack => cooldownTimer <= 0f;

    protected virtual void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public void TryAttack()
    {
        if (!CanAttack)
            return;

        cooldownTimer = cooldown;

        Attack();
    }

    protected abstract void Attack();
}