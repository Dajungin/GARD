
using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private int maxHP = 100;

    [Header("사망 설정")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelay = 0f;

    private int currentHP;
    private bool isDead;
    private bool isRegistered;

    private MonsterSpawner spawner;
    private bool hasNotifiedSpawner;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public bool IsDead => isDead;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        maxHP = Mathf.Max(1, maxHP);
        currentHP = maxHP;

        isDead = false;
        isRegistered = false;
        hasNotifiedSpawner = false;
    }

    private void OnEnable()
    {
        RegisterToStageManager();
    }

    private void OnDisable()
    {
        UnregisterFromStageManager();
    }

    // StageEnemyManager에 몬스터 등록
    public void RegisterToStageManager()
    {
        if (isDead || isRegistered)
            return;

        StageEnemyManager manager = StageEnemyManager.Instance;

        if (manager == null)
        {
            Debug.LogError(
                $"[EnemyHealth] StageEnemyManager가 없습니다: {name}"
            );

            return;
        }

        manager.RegisterEnemy(this);
        isRegistered = true;
    }

    // 몬스터 등록 해제
    private void UnregisterFromStageManager()
    {
        if (!isRegistered)
            return;

        StageEnemyManager manager = StageEnemyManager.Instance;

        if (manager != null)
        {
            manager.UnregisterEnemy(this);
        }

        isRegistered = false;
    }

    // MonsterSpawner에서 호출
    public void SetSpawner(MonsterSpawner newSpawner)
    {
        spawner = newSpawner;

        // 비활성 프리팹 등으로 OnEnable 등록이 안 된 경우 보완
        RegisterToStageManager();

        // 스포너에 등록 완료 알림은 한 번만 전달
        if (hasNotifiedSpawner)
            return;

        hasNotifiedSpawner = true;

        if (spawner != null)
        {
            spawner.Registered();
        }
    }

    // 일반 공격
    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHP = Mathf.Max(0, currentHP - damage);

        Debug.Log(
            $"[{name}] 피해: {damage} / 남은 체력: {currentHP}/{maxHP}"
        );

        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // 공격자가 Transform으로 전달되는 경우
    public void TakeDamage(int damage, Transform attacker)
    {
        if (isDead || damage <= 0)
            return;

        string attackerName =
            attacker != null ? attacker.name : "알 수 없는 공격자";

        Debug.Log(
            $"[{name}] 공격자: {attackerName}"
        );

        TakeDamage(damage);
    }

    // 사망 처리
    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log($"[{name}] 사망!");

        OnDeath?.Invoke();

        // 사망 즉시 매니저에서 제거
        UnregisterFromStageManager();

        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // 체력 회복
    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
            return;

        currentHP = Mathf.Min(maxHP, currentHP + amount);

        OnHealthChanged?.Invoke(currentHP, maxHP);

        Debug.Log(
            $"[{name}] 회복: {amount} / 현재 체력: {currentHP}/{maxHP}"
        );
    }
}
