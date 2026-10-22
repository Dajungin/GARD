
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("이동할 씬")]
    [SerializeField] private string targetSceneName = "BossRoom";

    [Header("플레이어 설정")]
    [SerializeField] private string playerTag = "Player";

    [Header("포탈 활성화 설정")]
    [SerializeField] private bool hidePortalUntilUnlocked = true;

    [Tooltip("포탈 이미지와 Collider를 포함한 오브젝트를 연결합니다.")]
    [SerializeField] private GameObject portalVisual;

    [SerializeField] private Collider2D portalCollider;

    private bool isUnlocked;
    private bool isLoading;

    private void Awake()
    {
        if (portalVisual == null)
        {
            portalVisual = gameObject;
        }

        if (portalCollider == null)
        {
            portalCollider = GetComponent<Collider2D>();
        }

        // 씬이 시작될 때 포탈을 잠급니다.
        SetPortalActive(false);
    }

    private void Start()
    {
        if (StageEnemyManager.Instance == null)
        {
            Debug.LogError(
                "Portal: 씬에 StageEnemyManager가 없습니다.",
                this
            );
            return;
        }

        StageEnemyManager.Instance.OnAllEnemiesDefeated += UnlockPortal;

        // 적이 없는 스테이지라면 바로 활성화합니다.
        if (StageEnemyManager.Instance.AllEnemiesDefeated)
        {
            UnlockPortal();
        }
    }

    private void OnDestroy()
    {
        if (StageEnemyManager.Instance != null)
        {
            StageEnemyManager.Instance.OnAllEnemiesDefeated -= UnlockPortal;
        }
    }

    private void UnlockPortal()
    {
        if (isUnlocked)
        {
            return;
        }

        isUnlocked = true;
        SetPortalActive(true);

        Debug.Log("포탈이 활성화되었습니다!");
    }

    private void SetPortalActive(bool active)
    {
        if (hidePortalUntilUnlocked && portalVisual != null)
        {
            portalVisual.SetActive(active);
        }

        // 포탈 오브젝트 자체를 비활성화하지 않고 Collider만 제어합니다.
        if (portalCollider != null)
        {
            portalCollider.enabled = active;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isUnlocked || isLoading)
        {
            return;
        }

        if (!other.CompareTag(playerTag) &&
            !other.transform.root.CompareTag(playerTag))
        {
            return;
        }

        LoadTargetScene();
    }

    private void LoadTargetScene()
    {
        if (isLoading || !isUnlocked)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError("Portal: 이동할 씬 이름이 비어 있습니다.", this);
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
        SceneManager.LoadScene(targetSceneName);
    }
}
