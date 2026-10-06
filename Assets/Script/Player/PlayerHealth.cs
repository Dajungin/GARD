using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("플레이어 목숨")]
    [SerializeField] private int maxLives = 3;

    [Header("무적 시간")]
    [SerializeField] private float invincibleTime = 2f;

    private int currentLives;
    private bool isInvincible;

    private Collider2D playerCollider;

    public int CurrentLives => currentLives;
    public bool IsDead => currentLives <= 0;
    public bool IsInvincible => isInvincible;

    private void Awake()
    {
        currentLives = maxLives;

        playerCollider = GetComponent<Collider2D>();
    }

    public void TakeDamage()
    {
        if (isInvincible)
            return;

        currentLives--;

        Debug.Log("플레이어 피격! 남은 목숨 : " + currentLives);

        if (currentLives <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibilityCoroutine());
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        // 적과의 충돌만 무시
        Physics2D.IgnoreLayerCollision(
            gameObject.layer,
            LayerMask.NameToLayer("Enemy"),
            true
        );

        Debug.Log("플레이어 무적 시작");

        yield return new WaitForSeconds(invincibleTime);

        // 적과의 충돌 다시 허용
        Physics2D.IgnoreLayerCollision(
            gameObject.layer,
            LayerMask.NameToLayer("Enemy"),
            false
        );

        isInvincible = false;

        Debug.Log("플레이어 무적 종료");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage();
        }
    }

    private void Die()
    {
        Debug.Log("플레이어 사망");

        gameObject.SetActive(false);
    }
}