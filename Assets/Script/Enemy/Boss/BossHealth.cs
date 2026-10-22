
using System;
using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("보스 체력")]
    [SerializeField, Min(1)] private int maxHP = 1000;
    [SerializeField, Range(0.1f, 0.9f)] private float phaseTwoPercent = 0.5f;

    [Header("페이즈 전환")]
    [SerializeField, Min(0f)] private float phaseTransitionDuration = 2f;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public bool IsDead => currentHP <= 0;
    public bool IsPhaseTwo { get; private set; }
    public bool IsTransitioning { get; private set; }

    public event Action OnPhaseTwoStarted;
    public event Action OnBossDied;

    private int currentHP;
    private bool deathNotified;
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || IsTransitioning || damage <= 0)
            return;

        currentHP = Mathf.Max(0, currentHP - damage);

        Debug.Log($"보스 피격! 피해량: {damage}, 현재 HP: {currentHP}/{maxHP}");

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

        // 여기서 애니메이션이나 이펙트를 재생할 수 있어.
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
            StopCoroutine(transitionCoroutine);

        IsTransitioning = false;

        Debug.Log("보스 처치!");
        OnBossDied?.Invoke();

        // 보스 사망 애니메이션이 있다면 이 부분을
        // 애니메이션 종료 후 Destroy로 바꿔도 돼.
        Destroy(gameObject);
    }
}
