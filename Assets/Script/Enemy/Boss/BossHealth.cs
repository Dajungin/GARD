
using System;
using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("보스 체력")]
    [SerializeField, Min(1)] private int maxHP = 1000;

    [SerializeField, Range(0.1f, 0.9f)]
    private float phaseTwoPercent = 0.5f;

    [Header("페이즈 전환")]
    [SerializeField, Min(0f)]
    private float phaseTransitionDuration = 2f;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public bool IsDead => currentHP <= 0;
    public bool IsPhaseTwo { get; private set; }
    public bool IsTransitioning { get; private set; }

    public event Action OnPhaseTwoStarted;
    public event Action OnBossDied;
    public event Action<int, int> OnHealthChanged;

    private int currentHP;
    private bool deathNotified;
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        currentHP = maxHP;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || IsTransitioning || damage <= 0)
            return;

        currentHP = Mathf.Max(0, currentHP - damage);

        Debug.Log(
            $"보스 피격! 피해량: {damage}, HP: {currentHP}/{maxHP}"
        );

        OnHealthChanged?.Invoke(currentHP, maxHP);

        // 체력이 설정한 비율 이하라면 2페이즈 시작
        if (!IsPhaseTwo &&
            currentHP <= Mathf.CeilToInt(maxHP * phaseTwoPercent))
        {
            IsPhaseTwo = true;

            transitionCoroutine = StartCoroutine(PhaseTransition());

            OnPhaseTwoStarted?.Invoke();
        }

        if (currentHP <= 0)
            Die();
    }

    private IEnumerator PhaseTransition()
    {
        IsTransitioning = true;

        Debug.Log("보스 2페이즈 전환!");

        if (phaseTransitionDuration > 0f)
            yield return new WaitForSeconds(phaseTransitionDuration);

        IsTransitioning = false;
        transitionCoroutine = null;
    }

    private void Die()
    {
        if (deathNotified)
            return;

        deathNotified = true;

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        IsTransitioning = false;

        Debug.Log("보스 처치!");
        OnBossDied?.Invoke();

        Destroy(gameObject);
    }
}
