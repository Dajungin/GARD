
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("스폰 영역")]
    [SerializeField] private BoxCollider2D spawnArea;

    [Header("몬스터 프리팹")]
    [SerializeField] private GameObject[] monsterPrefabs;

    [Header("생성 설정")]
    [SerializeField] private int totalSpawnCount = 20;

    [Tooltip("몬스터가 다른 오브젝트와 겹치지 않도록 검사하는 반지름")]
    [SerializeField] private float spawnCheckRadius = 0.5f;

    [Tooltip("몬스터 한 마리당 위치 탐색 최대 횟수")]
    [SerializeField] private int maxAttemptsPerMonster = 50;

    [Header("겹침 검사 레이어")]
    [SerializeField] private LayerMask blockingLayers;

    private int spawnedCount;
    private int registeredCount;

    private bool hasSpawned;

    private StageEnemyManager enemyManager;

    private void Start()
    {
        SpawnMonsters();
    }

    public void SpawnMonsters()
    {
        if (hasSpawned)
            return;

        if (spawnArea == null ||
            monsterPrefabs == null ||
            monsterPrefabs.Length == 0 ||
            totalSpawnCount <= 0)
        {
            Debug.LogError(
                "[MonsterSpawner] 환경 설정 오류"
            );

            return;
        }

        enemyManager = StageEnemyManager.Instance;

        if (enemyManager == null)
        {
            Debug.LogError(
                "[MonsterSpawner] StageEnemyManager가 없습니다."
            );

            return;
        }

        hasSpawned = true;

        spawnedCount = 0;
        registeredCount = 0;

        // 반드시 몬스터 생성 전에 호출
        enemyManager.BeginEnemySpawning();

        Debug.Log("[MonsterSpawner] 몬스터 생성 시작");

        for (int i = 0; i < totalSpawnCount; i++)
        {
            GameObject selectedMonster = GetRandomMonster();

            if (selectedMonster == null)
            {
                Debug.LogWarning(
                    "사용 가능한 몬스터 프리팹이 없습니다."
                );

                continue;
            }

            if (!TryGetSpawnPosition(out Vector2 spawnPosition))
            {
                Debug.LogWarning(
                    $"{i + 1}번째 몬스터의 위치를 찾지 못했습니다."
                );

                break;
            }

            GameObject monsterInstance = Instantiate(
                selectedMonster,
                spawnPosition,
                Quaternion.identity
            );

            if (monsterInstance == null)
                continue;

            EnemyHealth enemyHealth =
                monsterInstance.GetComponent<EnemyHealth>();

            if (enemyHealth == null)
            {
                enemyHealth =
                    monsterInstance.GetComponentInChildren<EnemyHealth>(true);
            }

            if (enemyHealth == null)
            {
                Debug.LogError(
                    $"[MonsterSpawner] {monsterInstance.name}에 EnemyHealth가 없습니다."
                );

                Destroy(monsterInstance);
                continue;
            }

            // 생성된 정상 몬스터 수 증가
            spawnedCount++;

            // 몬스터 스포너 연결 및 등록
            enemyHealth.SetSpawner(this);

            Debug.Log(
                $"[MonsterSpawner] 몬스터 생성: {spawnedCount}/{totalSpawnCount}"
            );
        }

        Debug.Log(
            $"[MonsterSpawner] 생성 작업 종료 / 생성: {spawnedCount} / 등록 콜백: {registeredCount}"
        );

        // 한 마리도 생성되지 않았다면 포탈을 열지 않음
        if (spawnedCount == 0)
        {
            Debug.LogError(
                "[MonsterSpawner] 생성된 몬스터가 없습니다. 포탈 잠금 유지"
            );

            return;
        }

        // 모든 생성 작업이 끝난 후 딱 한 번 호출
        enemyManager.FinishEnemySpawning();

        Debug.Log(
            $"[MonsterSpawner] 모든 몬스터 생성 완료: {spawnedCount}마리"
        );

        if (registeredCount != spawnedCount)
        {
            Debug.LogWarning(
                $"[MonsterSpawner] 등록 콜백 수 확인 필요: {registeredCount}/{spawnedCount}"
            );
        }
    }

    // EnemyHealth에서 호출
    // 여기서는 생성 완료 판정을 하지 않음
    public void Registered()
    {
        registeredCount++;

        Debug.Log(
            $"[MonsterSpawner] 몬스터 등록 확인: {registeredCount}/{totalSpawnCount}"
        );
    }

    private GameObject GetRandomMonster()
    {
        int validCount = 0;

        foreach (GameObject monster in monsterPrefabs)
        {
            if (monster != null)
            {
                validCount++;
            }
        }

        if (validCount == 0)
            return null;

        int index = Random.Range(0, validCount);

        foreach (GameObject monster in monsterPrefabs)
        {
            if (monster == null)
                continue;

            if (index == 0)
                return monster;

            index--;
        }

        return null;
    }

    private bool TryGetSpawnPosition(out Vector2 result)
    {
        result = Vector2.zero;

        Bounds bounds = spawnArea.bounds;

        float radius = Mathf.Max(0f, spawnCheckRadius);

        float minX = bounds.min.x + radius;
        float maxX = bounds.max.x - radius;

        float minY = bounds.min.y + radius;
        float maxY = bounds.max.y - radius;

        if (minX > maxX || minY > maxY)
            return false;

        for (int i = 0; i < maxAttemptsPerMonster; i++)
        {
            Vector2 position = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

            if (Physics2D.OverlapCircle(
                position,
                radius,
                blockingLayers
            ) == null)
            {
                result = position;
                return true;
            }
        }

        return false;
    }
    public void ResetForRestart()
    {
        StopAllCoroutines();

        // 기존 스폰 진행 횟수 초기화
        // 예: spawnedCount = 0;

        // 기존 스폰 코루틴 다시 시작
        // 예: StartCoroutine(SpawnCoroutine());
    }
}
