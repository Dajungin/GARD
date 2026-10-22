
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class BossShockwave : MonoBehaviour
{
    [Header("충격파 설정")]
    [SerializeField, Min(0f)] private float speed = 6f;
    [SerializeField, Min(0f)] private float phaseTwoSpeed = 8f;
    [SerializeField, Min(0)] private int damage = 1;
    [SerializeField, Min(0.1f)] private float lifetime = 3f;

    private Rigidbody2D rb;
    private Collider2D hitCollider;

    private int direction = 1;
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

    public void Initialize(int moveDirection, bool phaseTwo)
    {
        direction = moveDirection < 0 ? -1 : 1;
        initialized = true;

        float moveSpeed = phaseTwo ? phaseTwoSpeed : speed;

        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            0f
        );

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (!initialized)
            return;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth player =
            other.GetComponentInParent<PlayerHealth>();

        if (player == null)
            return;

        // 같은 플레이어에게 중복 피해를 주지 않음
        if (!hitPlayers.Add(player))
            return;

        // 플레이어의 회피 무적 상태 확인
        PlayerMove playerMove =
            player.GetComponent<PlayerMove>();

        if (playerMove == null || !playerMove.IsInvincible)
        {
            player.TakeDamage(damage);
        }
    }
}
