
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

    // 한 번만 생성하기 위한 변수
    private bool hasSpawned = false;

    private void Start()
    {
        SpawnMonsters();
    }

    public void SpawnMonsters()
    {
        // 이미 생성했다면 다시 실행하지 않음
        if (hasSpawned)
            return;

        hasSpawned = true;

        // 스폰 영역 확인
        if (spawnArea == null)
        {
            Debug.LogError("MonsterSpawner: SpawnArea가 등록되지 않았습니다.");
            return;
        }

        // 몬스터 프리팹 확인
        if (monsterPrefabs == null || monsterPrefabs.Length == 0)
        {
            Debug.LogError("MonsterSpawner: 몬스터 프리팹이 없습니다.");
            return;
        }

        if (totalSpawnCount <= 0)
        {
            Debug.LogWarning("MonsterSpawner: 생성할 몬스터 수가 0 이하입니다.");
            return;
        }

        int spawnedCount = 0;

        for (int i = 0; i < totalSpawnCount; i++)
        {
            GameObject selectedMonster = GetRandomMonster();

            if (selectedMonster == null)
                continue;

            if (TryGetSpawnPosition(out Vector2 spawnPosition))
            {
                Instantiate(
                    selectedMonster,
                    spawnPosition,
                    Quaternion.identity
                );

                spawnedCount++;
            }
            else
            {
                Debug.LogWarning(
                    $"MonsterSpawner: {i + 1}번째 몬스터의 생성 위치를 찾지 못했습니다."
                );

                break;
            }
        }

        Debug.Log(
            $"몬스터 생성 완료! {spawnedCount} / {totalSpawnCount}마리"
        );
    }

    // 등록한 몬스터 중 랜덤 선택
    private GameObject GetRandomMonster()
    {
        int validCount = 0;

        foreach (GameObject monster in monsterPrefabs)
        {
            if (monster != null)
                validCount++;
        }

        if (validCount == 0)
            return null;

        int randomIndex = Random.Range(0, validCount);

        foreach (GameObject monster in monsterPrefabs)
        {
            if (monster == null)
                continue;

            if (randomIndex == 0)
                return monster;

            randomIndex--;
        }

        return null;
    }

    // 맵 내부에서 겹치지 않는 위치 찾기
    private bool TryGetSpawnPosition(out Vector2 result)
    {
        result = Vector2.zero;

        Bounds bounds = spawnArea.bounds;

        float radius = Mathf.Max(0f, spawnCheckRadius);

        // 몬스터가 맵 경계를 넘어가지 않도록 여유 공간 확보
        float minX = bounds.min.x + radius;
        float maxX = bounds.max.x - radius;

        float minY = bounds.min.y + radius;
        float maxY = bounds.max.y - radius;

        if (minX > maxX || minY > maxY)
            return false;

        for (int i = 0; i < maxAttemptsPerMonster; i++)
        {
            Vector2 randomPosition = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

            // 다른 오브젝트와 겹치는지 검사
            Collider2D hit = Physics2D.OverlapCircle(
                randomPosition,
                radius,
                blockingLayers
            );

            // 아무것도 없다면 생성 가능
            if (hit == null)
            {
                result = randomPosition;
                return true;
            }
        }

        return false;
    }

    // 스폰 영역 확인용 Gizmos
    private void OnDrawGizmosSelected()
    {
        if (spawnArea == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            spawnArea.bounds.center,
            spawnArea.bounds.size
        );
    }
}
