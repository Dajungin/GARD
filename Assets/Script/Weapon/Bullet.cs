
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float speed;
    private float lifeTime;
    private int damage;

    private Vector2 direction;

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetLifeTime(float newLifeTime)
    {
        lifeTime = newLifeTime;
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 보스 본체 또는 자식 오브젝트에서 BossHealth 찾기
        BossHealth bossHealth =
            collision.GetComponentInParent<BossHealth>();

        if (bossHealth != null)
        {
            Debug.Log($"총알이 보스에게 적중! 피해량: {damage}");

            bossHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 일반 몬스터 본체 또는 자식 오브젝트에서 EnemyHealth 찾기
        EnemyHealth enemyHealth =
            collision.GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null)
        {
            Debug.Log($"총알이 적에게 적중! 피해량: {damage}");

            enemyHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
