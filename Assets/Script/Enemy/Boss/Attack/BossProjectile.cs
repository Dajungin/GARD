
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BossProjectile : MonoBehaviour
{
    [Header("마법탄 설정")]
    [SerializeField, Min(0)] private int damage = 1;
    [SerializeField, Min(0.1f)] private float lifetime = 4f;

    private Rigidbody2D rb;
    private Collider2D hitCollider;
    private bool initialized;
    private readonly HashSet<PlayerHealth> hitPlayers =
        new HashSet<PlayerHealth>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hitCollider = GetComponent<Collider2D>();

        hitCollider.isTrigger = true;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    public void Initialize(Vector2 direction, float speed)
    {
        direction = direction.normalized;

        rb.linearVelocity = direction * speed;
        initialized = true;

        // 스프라이트가 오른쪽을 향하도록 제작됐다는 기준
        if (direction.sqrMagnitude > 0f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x)
                          * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized)
            return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            if (hitPlayers.Contains(player))
                return;

            hitPlayers.Add(player);
            player.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        // 플레이어가 아닌 지형 등에 맞아도 소멸하도록 설정
        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
