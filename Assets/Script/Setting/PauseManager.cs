
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("일시정지 UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("씬 설정")]
    [SerializeField] private string titleSceneName = "Start";

    public bool IsPaused { get; private set; }

    private float previousTimeScale = 1f;

    private void Awake()
    {
        // 이미 PauseManager가 있다면 새로 생성된 중복 오브젝트 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬이 바뀌어도 PauseManager와 자식 UI 유지
        DontDestroyOnLoad(gameObject);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        IsPaused = false;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // 새 Input System의 ESC 키 입력 확인
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (IsPaused)
            return;

        if (pausePanel == null)
        {
            Debug.LogWarning("PausePanel을 Inspector에 연결해 주세요.");
            return;
        }

        previousTimeScale = Time.timeScale;
        IsPaused = true;

        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!IsPaused)
            return;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = previousTimeScale > 0f
            ? previousTimeScale
            : 1f;

        IsPaused = false;
    }

    // 현재 플레이 중인 씬 다시 시작
    public void RestartGame()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        ResumeGame();
        SceneManager.LoadScene(currentSceneName);
    }

    // 타이틀 씬으로 이동
    public void ReturnToTitle()
    {
        if (string.IsNullOrWhiteSpace(titleSceneName))
        {
            Debug.LogWarning("Title Scene Name을 입력해 주세요.");
            return;
        }

        ResumeGame();
        SceneManager.LoadScene(titleSceneName);
    }

    // 게임 종료
    public void QuitGame()
    {
        ResumeGame();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        Time.timeScale = 1f;
        Instance = null;
    }
}
