
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("무기 기본 설정")]
    [SerializeField] protected int damage = 20;

    [SerializeField] protected float cooldown = 0.5f;

    protected float cooldownTimer;

    // 기관단총처럼 버튼을 누르고 있는 상태인지 확인
    protected bool triggerHeld;

    // 현재 공격 가능한 상태인지 확인
    public bool CanAttack => cooldownTimer <= 0f;

    protected virtual void Update()
    {
        // 쿨타임 감소
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    // 한 번 공격 요청
    public void TryAttack()
    {
        if (!CanAttack)
            return;

        // 쿨타임 설정
        cooldownTimer = Mathf.Max(0f, cooldown);

        // 실제 무기 공격 실행
        Attack();
    }

    // 버튼을 누르고 있는 상태 전달
    // 기관단총에서 사용
    public virtual void SetTriggerHeld(bool held)
    {
        triggerHeld = held;
    }

    // 각 무기가 자신만의 공격 방식을 구현
    protected abstract void Attack();
}
