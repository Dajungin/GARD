
using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class StageEnemyManager : MonoBehaviour
{
    public static StageEnemyManager Instance { get; private set; }

    // 현재 살아 있는 몬스터 목록
    private readonly HashSet<EnemyHealth> aliveEnemies =
        new HashSet<EnemyHealth>();

    private bool isSpawning;
    private bool isSpawningFinished;
    private bool completionNotified;

    public int RemainingEnemies => aliveEnemies.Count;

    public bool IsSpawningFinished => isSpawningFinished;

    // 생성이 끝났고 살아 있는 몬스터가 없을 때만 true
    public bool AllEnemiesDefeated =>
        isSpawningFinished &&
        !isSpawning &&
        aliveEnemies.Count == 0;

    public event Action OnAllEnemiesDefeated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "[StageEnemyManager] 중복된 매니저가 있습니다."
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;

        isSpawning = false;
        isSpawningFinished = false;
        completionNotified = false;

        aliveEnemies.Clear();

        Debug.Log("[StageEnemyManager] 초기화 완료");
    }

    // 몬스터 생성 시작
    public void BeginEnemySpawning()
    {
        isSpawning = true;
        isSpawningFinished = false;
        completionNotified = false;

        Debug.Log("[StageEnemyManager] 몬스터 생성 시작");
    }

    // 몬스터 등록
    public void RegisterEnemy(EnemyHealth enemy)
    {
        if (enemy == null || enemy.IsDead)
            return;

        if (aliveEnemies.Add(enemy))
        {
            Debug.Log(
                $"[StageEnemyManager] 몬스터 등록: {enemy.name} / 현재 생존 수: {aliveEnemies.Count}"
            );
        }
    }

    // 몬스터 사망 또는 비활성화 시 등록 해제
    public void UnregisterEnemy(EnemyHealth enemy)
    {
        if (enemy == null)
            return;

        if (aliveEnemies.Remove(enemy))
        {
            Debug.Log(
                $"[StageEnemyManager] 몬스터 등록 해제: {enemy.name} / 남은 수: {aliveEnemies.Count}"
            );
        }

        CheckAllEnemiesDefeated();
    }

    // 모든 몬스터 생성 완료
    public void FinishEnemySpawning()
    {
        if (isSpawningFinished)
            return;

        isSpawning = false;
        isSpawningFinished = true;

        Debug.Log(
            $"[StageEnemyManager] 몬스터 생성 완료 / 현재 생존 수: {aliveEnemies.Count}"
        );

        CheckAllEnemiesDefeated();
    }

    // 모든 몬스터 처치 여부 확인
    private void CheckAllEnemiesDefeated()
    {
        if (!isSpawningFinished || isSpawning)
            return;

        if (aliveEnemies.Count > 0)
            return;

        if (completionNotified)
            return;

        completionNotified = true;

        Debug.Log("[StageEnemyManager] 모든 몬스터 처치 완료!");

        OnAllEnemiesDefeated?.Invoke();
    }

    // 재시작 초기화
    public void ResetForRestart()
    {
        aliveEnemies.Clear();

        isSpawning = false;
        isSpawningFinished = false;
        completionNotified = false;

        Debug.Log("[StageEnemyManager] 재시작 초기화 완료");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
