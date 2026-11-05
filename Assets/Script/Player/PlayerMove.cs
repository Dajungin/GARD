
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

    [Header("애니메이션")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private bool isGrounded;
    private bool isDodging;
    private bool isInvincible;

    private bool isRunning;

    private float dodgeTimer;
    private float invincibleTimer;
    private float dodgeCooldownTimer;

    public bool IsInvincible => isInvincible;
    public bool IsDodging => isDodging;

    private Vector2 dodgeDirection;
    private Vector2 lastMoveDirection = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Inspector에 Animator를 연결하지 않았다면 자동으로 찾기
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        ReadMovementInput();
        CheckGround();

        // W로 점프
        if (Keyboard.current.wKey.wasPressedThisFrame &&
            isGrounded && !isDodging)
        {
            Jump();
        }

        // Space로 구르기
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryDodge();
        }

        // 애니메이션 갱신
        UpdateAnimation();

        UpdateTimers();
    }

    private void FixedUpdate()
    {
        if (isDodging)
        {
            rb.linearVelocity = dodgeDirection * dodgeSpeed;
            return;
        }

        float currentSpeed = moveSpeed;

        if (Keyboard.current != null &&
            (Keyboard.current.leftShiftKey.isPressed ||
             Keyboard.current.rightShiftKey.isPressed))
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

        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal += 1f;

        // W는 점프 전용, A/D는 좌우 이동 전용
        moveInput = new Vector2(horizontal, 0f);

        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = moveInput.normalized;
        }

        isRunning =
            Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.rightShiftKey.isPressed;
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        // 지면 여부 전달
        animator.SetBool("IsGrounded", isGrounded);

        // 회피 중에는 걷기/달리기 애니메이션을 실행하지 않음
        if (isDodging)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        float animationSpeed = 0f;

        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            animationSpeed = isRunning ? 2f : 1f;
        }

        animator.SetFloat("Speed", animationSpeed);
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

        dodgeDirection = moveInput.sqrMagnitude > 0.01f
            ? moveInput.normalized
            : lastMoveDirection;

        isDodging = true;
        isInvincible = true;

        dodgeTimer = dodgeDuration;
        invincibleTimer = dodgeInvincibleDuration;
        dodgeCooldownTimer = dodgeCooldown;

        rb.linearVelocity = Vector2.zero;

        // 구르기 애니메이션 실행
        if (animator != null)
        {
            animator.ResetTrigger("Roll");
            animator.SetTrigger("Roll");
        }
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

    // 나중에 대시 기능을 만들 때 호출할 애니메이션 함수
    public void PlayDashAnimation()
    {
        if (animator == null)
            return;

        animator.ResetTrigger("Dash");
        animator.SetTrigger("Dash");
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
