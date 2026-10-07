using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("적 체력")]
    [SerializeField] private int maxHP = 100;

    private int currentHP;

    public int CurrentHP => currentHP;
    public bool IsDead => currentHP <= 0;

    private void Awake()
    {
        currentHP = maxHP;
    }

    // =========================================================
    // 데미지 받기
    // =========================================================

    public void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        damage = Mathf.Max(0, damage);

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

    private void Die()
    {
        Debug.Log(gameObject.name + " 사망");

        Destroy(gameObject);
    }
}