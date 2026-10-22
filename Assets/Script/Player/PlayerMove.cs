
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;

    [Header("달리기 설정")]
    [SerializeField, Min(1f)] private float runMultiplier = 1.8f;

    [Header("점프 설정")]
    [SerializeField, Min(0f)] private float jumpForce = 10f;

    [Header("바닥 체크")]
    [SerializeField] private Transform groundCheck;
    [SerializeField, Min(0.01f)] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("회피 설정")]
    [SerializeField, Min(0.1f)] private float dodgeSpeed = 12f;
    [SerializeField, Min(0.05f)] private float dodgeDuration = 0.2f;
    [SerializeField, Min(0f)] private float dodgeCooldown = 1f;
    [SerializeField, Min(0f)] private float dodgeInvincibleDuration = 0.3f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private bool isGrounded;
    private bool isDodging;
    private bool isInvincible;

    private float dodgeTimer;
    private float invincibleTimer;
    private float dodgeCooldownTimer;

    // 다른 스크립트에서 회피 무적 여부를 확인할 때 사용
    public bool IsInvincible => isInvincible;
    public bool IsDodging => isDodging;

    // 회피 중 사용할 방향
    private Vector2 dodgeDirection;

    // 마지막으로 이동한 방향
    private Vector2 lastMoveDirection = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        ReadMovementInput();
        CheckGround();

        // W로 점프
        if (Keyboard.current.wKey.wasPressedThisFrame && isGrounded
            && !isDodging)
        {
            Jump();
        }

        // Shift를 누르고 있는 동안 달리기
        // Space는 회피 전용
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryDodge();
        }

        UpdateTimers();
    }

    private void FixedUpdate()
    {
        if (isDodging)
        {
            // 회피 중에는 일반 이동이 속도를 덮어쓰지 않음
            rb.linearVelocity = dodgeDirection * dodgeSpeed;
            return;
        }

        float currentSpeed = moveSpeed;

        if (Keyboard.current != null &&
            Keyboard.current.leftShiftKey.isPressed)
        {
            currentSpeed *= runMultiplier;
        }

        rb.linearVelocity = new Vector2(
            moveInput.x * currentSpeed,
            rb.linearVelocity.y
        );
    }

    private void ReadMovementInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal += 1f;

        if (Keyboard.current.wKey.isPressed)
            vertical += 1f;

        if (Keyboard.current.sKey.isPressed)
            vertical -= 1f;

        moveInput = new Vector2(horizontal, vertical).normalized;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = moveInput;
        }
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void TryDodge()
    {
        if (isDodging || dodgeCooldownTimer > 0f)
            return;

        // 이동 입력이 있으면 해당 방향, 없으면 마지막 이동 방향
        dodgeDirection = moveInput.sqrMagnitude > 0.01f
            ? moveInput
            : lastMoveDirection;

        dodgeDirection.Normalize();

        isDodging = true;
        isInvincible = true;

        dodgeTimer = dodgeDuration;
        invincibleTimer = dodgeInvincibleDuration;
        dodgeCooldownTimer = dodgeCooldown;

        // 회피 시작 시 기존 속도 제거
        rb.linearVelocity = Vector2.zero;
    }

    private void UpdateTimers()
    {
        if (isDodging)
        {
            dodgeTimer -= Time.deltaTime;

            if (dodgeTimer <= 0f)
            {
                isDodging = false;
            }
        }

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;

            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
            }
        }

        if (dodgeCooldownTimer > 0f)
        {
            dodgeCooldownTimer -= Time.deltaTime;
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            0f
        );

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
