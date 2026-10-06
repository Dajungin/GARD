using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("적 HP")]
    [SerializeField] private int maxHP = 100;

    private int currentHP;

    public int CurrentHP => currentHP;
    public bool IsDead => currentHP <= 0;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        currentHP -= damage;

        Debug.Log("적 HP : " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("적 사망");

        Destroy(gameObject);
    }
}