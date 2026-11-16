
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("플레이어 체력")]
    [SerializeField] private int maxHP = 3;

    [Header("피격 후 무적 시간")]
    [SerializeField] private float invincibleTime = 2f;

    [Header("시작 위치")]
    [SerializeField] private Transform restartPoint;

    private static bool resetAfterSceneLoad;

    private int currentHP;
    private bool isInvincible;
    private bool gameOverStarted;

    private Rigidbody2D rb;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public bool IsDead => currentHP <= 0;
    public bool IsInvincible => isInvincible;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        currentHP = maxHP;

        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 재시작 요청
    public static void RequestResetAfterSceneLoad()
    {
        resetAfterSceneLoad = true;
    }

    // 게임 씬이 다시 로드되면 실행
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!resetAfterSceneLoad)
            return;

        resetAfterSceneLoad = false;

        ResetForRestart();

        Debug.Log("[PlayerHealth] 플레이어 초기화 완료");
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || IsDead || gameOverStarted)
            return;

        damage = Mathf.Max(0, damage);

        if (damage <= 0)
            return;

        currentHP = Mathf.Max(0, currentHP - damage);

        Debug.Log("플레이어 HP : " + currentHP);

        if (IsDead)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibilityCoroutine());
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        int enemyLayer = LayerMask.NameToLayer("Enemy");

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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }

    private void Die()
    {
        if (gameOverStarted)
            return;

        gameOverStarted = true;

        Time.timeScale = 0f;

        SoundSettingsUI gameOverUI =
            FindFirstObjectByType<SoundSettingsUI>(
                FindObjectsInactive.Include
            );

        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }
        else
        {
            Debug.LogError(
                "[PlayerHealth] SoundSettingsUI를 찾을 수 없습니다."
            );
        }
    }

    public void ResetForRestart()
    {
        StopAllCoroutines();

        currentHP = maxHP;
        isInvincible = false;
        gameOverStarted = false;

        if (restartPoint != null)
        {
            transform.position = restartPoint.position;
            transform.rotation = restartPoint.rotation;
        }
        else
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(
                gameObject.layer,
                enemyLayer,
                false
            );
        }

        Debug.Log("[PlayerHealth] HP 초기화: " + currentHP);
    }

}
