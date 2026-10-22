
using UnityEngine;

public class MonsterAttackHitbox : MonoBehaviour
{
    [Header("공격 설정")]
    [SerializeField] private int damage = 1;

    private Collider2D hitboxCollider;
    private bool hasHitPlayer;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();

        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false;
        }
    }

    public void BeginAttack()
    {
        hasHitPlayer = false;

        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = true;
        }
    }

    public void EndAttack()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitPlayer)
        {
            return;
        }

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
        {
            return;
        }

        hasHitPlayer = true;
        playerHealth.TakeDamage(damage);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 공격 판정이 활성화된 뒤 플레이어가 이미 겹쳐 있는 경우도 처리합니다.
        if (hasHitPlayer)
        {
            return;
        }

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
        {
            return;
        }

        hasHitPlayer = true;
        playerHealth.TakeDamage(damage);
    }
}
