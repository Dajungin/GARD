
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerScenePersistence : MonoBehaviour
{
    public static PlayerScenePersistence Instance { get; private set; }

    private void Awake()
    {
        // 중복 플레이어 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬이 바뀌어도 플레이어 유지
        DontDestroyOnLoad(gameObject);

        // 씬 로드 완료 이벤트 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새 씬에 배치된 시작 위치 찾기
        PlayerSpawnPoint spawnPoint = FindFirstObjectByType<PlayerSpawnPoint>();

        if (spawnPoint == null)
        {
            Debug.LogWarning(
                $"씬 '{scene.name}'에 PlayerSpawnPoint가 없습니다. " +
                "플레이어 위치를 변경하지 않습니다."
            );
            return;
        }

        // 기존 플레이어를 새 씬의 시작 위치로 이동
        transform.position = spawnPoint.transform.position;

        // Rigidbody2D가 있다면 이동 속도 초기화
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Debug.Log(
            $"플레이어를 '{scene.name}'의 시작 위치로 이동했습니다: " +
            $"{spawnPoint.transform.position}"
        );
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
