
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("적 체력")]
    [SerializeField] private int maxHP = 100;

    private int currentHP;
    private bool isDead;

    public int CurrentHP => currentHP;
    public bool IsDead => isDead;

    private void Awake()
    {
        maxHP = Mathf.Max(1, maxHP);
        currentHP = maxHP;
    }

    // =========================================
    // 적 활성화 시 등록
    // =========================================

    private void OnEnable()
    {
        if (StageEnemyManager.Instance != null)
        {
            StageEnemyManager.Instance.RegisterEnemy(this);
        }
    }

    // =========================================
    // 적 비활성화 시 등록 해제
    // =========================================

    private void OnDisable()
    {
        if (StageEnemyManager.Instance != null)
        {
            StageEnemyManager.Instance.UnregisterEnemy(this);
        }
    }

    // =========================================
    // 데미지 받기
    // =========================================

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        Debug.Log(
            gameObject.name
            + " 피격! 데미지 : "
            + damage
            + " / 현재 HP : "
            + currentHP
        );

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // =========================================
    // 사망 처리
    // =========================================

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log(gameObject.name + " 사망");

        // 기존 사망 처리 유지
        Destroy(gameObject);
    }
}
