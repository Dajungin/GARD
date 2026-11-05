
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private int maxHP = 100;

    private int currentHP;
    private bool isDead;

    private EnemyAI enemyAI;

    public int CurrentHP => currentHP;
    public bool IsDead => isDead;

    private void Awake()
    {
        maxHP = Mathf.Max(1, maxHP);
        currentHP = maxHP;

        enemyAI = GetComponent<EnemyAI>();
    }

    private void OnEnable()
    {
        if (StageEnemyManager.Instance != null)
        {
            StageEnemyManager.Instance.RegisterEnemy(this);
        }
    }

    private void OnDisable()
    {
        if (StageEnemyManager.Instance != null)
        {
            StageEnemyManager.Instance.UnregisterEnemy(this);
        }
    }

    // 기존 방식: 데미지만 전달
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, null);
    }

    // 새로운 방식: 데미지와 공격자 위치 전달
    public void TakeDamage(int damage, Transform attacker)
    {
        if (isDead || damage <= 0)
            return;

        // 공격받으면 플레이어를 인식하도록 EnemyAI에 전달
        if (enemyAI != null && attacker != null)
        {
            enemyAI.AlertFromDamage(attacker);
        }

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        Debug.Log("적이 피해를 입었습니다! 현재 체력: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("적이 사망했습니다.");

        Destroy(gameObject);
    }
}
