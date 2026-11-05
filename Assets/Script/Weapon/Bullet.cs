
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("총알 설정")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int damage = 20;

    [Header("충돌 설정")]
    [SerializeField] private LayerMask groundLayer;

    private Vector2 direction;
    private bool hasHit;

    // 총알 발사 방향 설정
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // 총알 속도 설정
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    // 총알 지속 시간 설정
    public void SetLifeTime(float newLifeTime)
    {
        lifeTime = newLifeTime;
    }

    // 총알 데미지 설정
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
        if (hasHit)
            return;

        // 이번 프레임에 총알이 이동할 거리
        float moveDistance = speed * Time.deltaTime;

        // 이동 경로에 Ground 레이어가 있는지 검사
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            moveDistance,
            groundLayer
        );

        if (hit.collider != null)
        {
            hasHit = true;
            Destroy(gameObject);
            return;
        }

        // 총알 이동
        transform.position += (Vector3)(direction * moveDistance);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit)
            return;

        // 보스 충돌 확인
        BossHealth bossHealth =
            collision.GetComponentInParent<BossHealth>();

        if (bossHealth != null)
        {
            hasHit = true;

            Debug.Log("총알이 보스에게 명중!");

            bossHealth.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        // 일반 적 충돌 확인
        EnemyHealth enemyHealth =
            collision.GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null)
        {
            hasHit = true;

            Debug.Log("총알이 적에게 명중!");

            // 플레이어 위치를 전달하여 적이 플레이어를 인식하도록 함
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                enemyHealth.TakeDamage(damage, playerObject.transform);
            }
            else
            {
                // 플레이어를 찾지 못해도 데미지는 적용
                enemyHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 총알 이동 방향 확인용
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direction * 1f);
    }
}
