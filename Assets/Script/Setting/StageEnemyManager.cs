
using System.Collections.Generic;
using UnityEngine;

// 적보다 먼저 관리자가 초기화되도록 설정
[DefaultExecutionOrder(-1000)]
public class StageEnemyManager : MonoBehaviour
{
    public static StageEnemyManager Instance { get; private set; }

    [Header("적 생성 설정")]
    [Tooltip("모든 적 생성이 끝난 뒤 체크합니다. 처음에는 false로 설정합니다.")]
    [SerializeField] private bool enemySpawningFinished = false;

    // 현재 등록된 적 목록
    private readonly HashSet<EnemyHealth> aliveEnemies =
        new HashSet<EnemyHealth>();

    private bool completionNotified;

    public int RemainingEnemies => aliveEnemies.Count;

    public bool AllEnemiesDefeated =>
        enemySpawningFinished && aliveEnemies.Count == 0;

    public bool EnemySpawningFinished => enemySpawningFinished;

    public event System.Action OnAllEnemiesDefeated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "StageEnemyManager가 중복으로 존재합니다.",
                this
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;

        Debug.Log("StageEnemyManager 준비 완료");
    }

    // =========================================
    // 적 등록
    // =========================================

    public void RegisterEnemy(EnemyHealth enemy)
    {
        if (enemy == null || enemy.IsDead)
            return;

        // 중복 등록 방지
        if (!aliveEnemies.Add(enemy))
            return;

        // 새로운 적이 등장했으므로 완료 상태 초기화
        completionNotified = false;

        Debug.Log(
            $"적 등록 완료! 현재 적 수: {RemainingEnemies}"
        );
    }

    // =========================================
    // 적 등록 해제
    // =========================================

    public void UnregisterEnemy(EnemyHealth enemy)
    {
        if (enemy == null)
            return;

        if (!aliveEnemies.Remove(enemy))
            return;

        Debug.Log(
            $"적 등록 해제! 남은 적 수: {RemainingEnemies}"
        );

        CheckAllEnemiesDefeated();
    }

    // =========================================
    // 적 생성 완료 알림
    // =========================================

    public void FinishEnemySpawning()
    {
        if (enemySpawningFinished)
            return;

        enemySpawningFinished = true;

        Debug.Log(
            $"적 생성 완료! 현재 적 수: {RemainingEnemies}"
        );

        CheckAllEnemiesDefeated();
    }

    // =========================================
    // 모든 적 처치 확인
    // =========================================

    private void CheckAllEnemiesDefeated()
    {
        // 적 생성이 끝나기 전에는 포탈을 열지 않음
        if (!enemySpawningFinished)
            return;

        // 아직 적이 남아 있거나 완료 이벤트가 이미 발생했다면 종료
        if (aliveEnemies.Count > 0 || completionNotified)
            return;

        completionNotified = true;

        Debug.Log("모든 적 처치 완료! 포탈 활성화");

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
