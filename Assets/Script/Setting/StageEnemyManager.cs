
using UnityEngine;
using System;

public class StageEnemyManager : MonoBehaviour
{
    public static StageEnemyManager Instance { get; private set; }

    [Header("스테이지 적 설정")]
    [SerializeField] private int totalEnemies;

    public int RemainingEnemies { get; private set; }

    public bool AllEnemiesDefeated => RemainingEnemies <= 0;

    public event Action OnAllEnemiesDefeated;

    private bool completionNotified;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("StageEnemyManager가 중복으로 존재합니다.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;

        RemainingEnemies = Mathf.Max(0, totalEnemies);
    }

    private void Start()
    {
        CheckAllEnemiesDefeated();
    }

    public void EnemyDied()
    {
        if (RemainingEnemies <= 0)
        {
            return;
        }

        RemainingEnemies--;

        Debug.Log($"남은 적: {RemainingEnemies}");

        CheckAllEnemiesDefeated();
    }

    private void CheckAllEnemiesDefeated()
    {
        if (RemainingEnemies > 0 || completionNotified)
        {
            return;
        }

        completionNotified = true;

        Debug.Log("모든 적 처치! 포탈이 활성화됩니다.");

        OnAllEnemiesDefeated?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
