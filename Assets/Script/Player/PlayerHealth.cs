using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("플레이어 체력")]
    [SerializeField] private int maxHP = 3;

    [Header("피격 후 무적 시간")]
    [SerializeField] private float invincibleTime = 2f;

    private int currentHP;
    private bool isInvincible;

    public int CurrentHP => currentHP;
    public bool IsDead => currentHP <= 0;
    public bool IsInvincible => isInvincible;

    private void Awake()
    {
        currentHP = maxHP;
    }

    // =========================================================
    // 데미지 받기
    // =========================================================

    public void TakeDamage(int damage)
    {
        if (isInvincible)
            return;

        if (IsDead)
            return;

        // 음수 데미지 방지
        damage = Mathf.Max(0, damage);

        currentHP -= damage;

        // HP가 0 아래로 내려가지 않도록
        currentHP = Mathf.Max(0, currentHP);

        Debug.Log(
            "플레이어 피격! 데미지 : "
            + damage
            + " / 현재 HP : "
            + currentHP
        );

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibilityCoroutine());
    }

    // =========================================================
    // 무적
    // =========================================================

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        Debug.Log("플레이어 무적 시작");

        // Enemy 레이어와의 충돌을 잠시 무시
        int enemyLayer =
            LayerMask.NameToLayer("Enemy");

        if (enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(
                gameObject.layer,
                enemyLayer,
                true
            );
        }

        yield return new WaitForSeconds(invincibleTime);

        if (enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(
                gameObject.layer,
                enemyLayer,
                false
            );
        }

        isInvincible = false;

        Debug.Log("플레이어 무적 종료");
    }

    // =========================================================
    // 기존 일반 충돌
    // =========================================================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy"))
            return;

        // 일반 충돌에서는 기본 데미지 1
        TakeDamage(1);
    }

    // =========================================================
    // 사망
    // =========================================================

    private void Die()
    {
        Debug.Log("플레이어 사망");

        gameObject.SetActive(false);
    }
}