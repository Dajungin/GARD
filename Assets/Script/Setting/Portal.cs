
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("이동할 씬")]
    [SerializeField] private string targetSceneName = "BossRoom";

    [Header("플레이어 설정")]
    [SerializeField] private string playerTag = "Player";

    [Header("포탈 표시 설정")]
    [SerializeField] private GameObject portalVisual;

    [SerializeField] private Collider2D portalCollider;

    private SpriteRenderer[] portalRenderers;

    private StageEnemyManager enemyManager;

    private bool isUnlocked;
    private bool isLoading;

    private void Awake()
    {
        if (portalCollider == null)
        {
            portalCollider = GetComponent<Collider2D>();
        }

        portalRenderers =
            GetComponentsInChildren<SpriteRenderer>(true);

        isUnlocked = false;
        isLoading = false;

        // 게임 시작 시 포탈 잠금
        SetPortalActive(false);
    }

    private void Start()
    {
        enemyManager = StageEnemyManager.Instance;

        if (enemyManager == null)
        {
            Debug.LogError(
                "[Portal] StageEnemyManager가 없습니다."
            );

            return;
        }

        // 완료 이벤트 등록
        enemyManager.OnAllEnemiesDefeated += UnlockPortal;

        // 이미 모든 몬스터가 처치된 경우 확인
        if (enemyManager.AllEnemiesDefeated)
        {
            UnlockPortal();
        }
        else
        {
            SetPortalActive(false);
        }
    }

    private void OnDestroy()
    {
        if (enemyManager != null)
        {
            enemyManager.OnAllEnemiesDefeated -= UnlockPortal;
        }
    }

    private void UnlockPortal()
    {
        if (isUnlocked)
            return;

        if (enemyManager == null)
            return;

        // 매니저가 완료를 확인했을 때만 열기
        if (!enemyManager.AllEnemiesDefeated)
            return;

        isUnlocked = true;

        SetPortalActive(true);

        Debug.Log("[Portal] 모든 몬스터 처치 완료! 포탈 개방");
    }

    private void SetPortalActive(bool active)
    {
        if (portalVisual != null && portalVisual != gameObject)
        {
            portalVisual.SetActive(active);
        }

        if (portalRenderers != null)
        {
            foreach (SpriteRenderer renderer in portalRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = active;
                }
            }
        }

        if (portalCollider != null)
        {
            portalCollider.enabled = active;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isUnlocked || isLoading)
            return;

        Transform current = other.transform;

        while (current != null)
        {
            if (current.CompareTag(playerTag))
            {
                LoadTargetScene();
                return;
            }

            current = current.parent;
        }
    }

    private void LoadTargetScene()
    {
        if (isLoading)
            return;

        if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            Debug.LogError(
                $"[Portal] 씬을 찾을 수 없습니다: {targetSceneName}"
            );

            return;
        }

        isLoading = true;

        SceneManager.LoadScene(targetSceneName);
    }
}
