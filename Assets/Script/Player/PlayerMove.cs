using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("점프 설정")]
    [SerializeField] private float jumpForce = 10f;

    [Header("바닥 체크")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;

    // WASD 이동 방향
    private Vector2 moveInput;

    // 바닥에 있는지 여부
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // -------------------------
        // WASD 입력
        // -------------------------

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed)
        {
            horizontal = -1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            horizontal = 1f;
        }

        if (Keyboard.current.wKey.isPressed)
        {
            vertical = 1f;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            vertical = -1f;
        }

        moveInput = new Vector2(horizontal, vertical);

        // 대각선 이동 시 속도가 빨라지는 것을 방지
        moveInput = moveInput.normalized;

        // -------------------------
        // 바닥 체크
        // -------------------------

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // -------------------------
        // 점프
        // -------------------------

        if (Keyboard.current.wKey.wasPressedThisFrame && isGrounded)
        {
            Jump();
        }

        // Space도 점프 가능
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        // 좌우 이동
        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void Jump()
    {
        // 기존 낙하 속도 초기화
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            0f
        );

        // 위쪽으로 점프
        rb.AddForce(
            Vector2.up * jumpForce,
            ForceMode2D.Impulse
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}