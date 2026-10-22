
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("이동할 씬")]
    [Tooltip("이동할 씬의 이름을 입력합니다.")]
    [SerializeField] private string targetSceneName = "BossRoom";

    [Header("플레이어 설정")]
    [SerializeField] private string playerTag = "Player";

    [Header("포탈 표시 설정")]
    [Tooltip("포탈의 이미지가 있는 자식 오브젝트를 연결합니다.")]
    [SerializeField] private GameObject portalVisual;

    [Tooltip("포탈의 Trigger Collider를 연결합니다.")]
    [SerializeField] private Collider2D portalCollider;

    private bool isUnlocked;
    private bool isLoading;

    private void Awake()
    {
        // Collider 자동 연결
        if (portalCollider == null)
        {
            portalCollider = GetComponent<Collider2D>();
        }

        // 시작할 때 포탈 잠금
        SetPortalActive(false);
    }

    private void Start()
    {
        if (StageEnemyManager.Instance == null)
        {
            Debug.LogError(
                "Portal: StageEnemyManager가 씬에 없습니다.",
                this
            );

            return;
        }

        // 모든 적 처치 이벤트 구독
        StageEnemyManager.Instance.OnAllEnemiesDefeated += UnlockPortal;

        // 적이 이미 모두 처치된 상태라면 즉시 활성화
        if (StageEnemyManager.Instance.AllEnemiesDefeated)
        {
            UnlockPortal();
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (StageEnemyManager.Instance != null)
        {
            StageEnemyManager.Instance.OnAllEnemiesDefeated -= UnlockPortal;
        }
    }

    // =========================================
    // 포탈 잠금 해제
    // =========================================

    private void UnlockPortal()
    {
        if (isUnlocked)
            return;

        isUnlocked = true;

        SetPortalActive(true);

        Debug.Log("포탈 활성화 완료! 플레이어가 들어갈 수 있습니다.");
    }

    // =========================================
    // 포탈 표시 및 충돌 설정
    // =========================================

    private void SetPortalActive(bool active)
    {
        // 루트 오브젝트는 비활성화하지 않습니다.
        // Portal 스크립트가 이벤트를 계속 받을 수 있어야 합니다.

        if (portalVisual != null)
        {
            portalVisual.SetActive(active);
        }

        if (portalCollider != null)
        {
            portalCollider.enabled = active;
        }
    }

    // =========================================
    // 플레이어가 포탈에 닿았을 때
    // =========================================


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(
            $"[Portal] 충돌 감지: {other.name}, " +
            $"태그: {other.tag}, 잠금 해제: {isUnlocked}",
            this
        );

        if (!isUnlocked || isLoading)
        {
            Debug.Log("[Portal] 아직 잠겨 있거나 씬 이동 중입니다.", this);
            return;
        }

        // 충돌한 오브젝트부터 부모 방향으로 플레이어 태그 검색
        Transform current = other.transform;
        bool isPlayer = false;

        while (current != null)
        {
            if (current.CompareTag(playerTag))
            {
                isPlayer = true;
                break;
            }

            current = current.parent;
        }

        if (!isPlayer)
        {
            Debug.Log("[Portal] 플레이어가 아닌 오브젝트입니다.", this);
            return;
        }

        Debug.Log("[Portal] 플레이어 확인 완료. 씬 이동을 시도합니다.", this);

        LoadTargetScene();
    }


    // =========================================
    // 씬 이동
    // =========================================

    private void LoadTargetScene()
    {
        if (isLoading || !isUnlocked)
            return;

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError(
                "Portal: 이동할 씬 이름이 비어 있습니다.",
                this
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            Debug.LogError(
                $"Portal: '{targetSceneName}' 씬을 불러올 수 없습니다. " +
                "Build Profiles의 Scene List를 확인하세요.",
                this
            );

            return;
        }

        isLoading = true;

        Debug.Log($"포탈 이동: {targetSceneName}");

        SceneManager.LoadScene(targetSceneName);
    }
}
