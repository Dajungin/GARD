
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundSettingsUI : MonoBehaviour
{
    [Header("설정 패널")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject soundPanel;

    [Header("게임 오버 패널")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("씬 설정")]
    [SerializeField] private string startSceneName = "Start";

    private bool isRestarting = false;

    // ==============================
    // 게임 오버 표시
    // ==============================
    public void ShowGameOver()
    {
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("[SoundSettingsUI] GameOverPanel이 연결되지 않았습니다.");
        }
    }

    // ==============================
    // 게임 오버 닫기
    // ==============================
    public void CloseGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // ==============================
    // 게임 재시작 버튼
    // ==============================
    public void RestartGame()
    {
        if (isRestarting)
            return;

        StartCoroutine(RestartGameCoroutine());
    }

    private IEnumerator RestartGameCoroutine()
    {
        isRestarting = true;

        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();

        if (!currentScene.IsValid() || !currentScene.isLoaded)
        {
            Debug.LogError("[SoundSettingsUI] 현재 씬을 찾을 수 없습니다.");
            isRestarting = false;
            yield break;
        }

        string gameSceneName = currentScene.name;

        Debug.Log("[SoundSettingsUI] 게임 재시작: " + gameSceneName);

        CloseGameOver();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (soundPanel != null)
            soundPanel.SetActive(false);

        // 플레이어 초기화 예약
        PlayerHealth.RequestResetAfterSceneLoad();

        AsyncOperation operation = SceneManager.LoadSceneAsync(
            gameSceneName,
            LoadSceneMode.Single
        );

        if (operation == null)
        {
            Debug.LogError("[SoundSettingsUI] 게임 씬 재로드 실패");
            isRestarting = false;
            yield break;
        }

        yield return operation;

        Time.timeScale = 1f;
        isRestarting = false;

        Debug.Log("[SoundSettingsUI] 게임 재시작 완료");
    }

    // ==============================
    // 시작 화면으로 돌아가기
    // ==============================
    public void GoToStartScene()
    {
        if (isRestarting)
            return;

        StartCoroutine(GoToStartSceneCoroutine());
    }

    private IEnumerator GoToStartSceneCoroutine()
    {
        isRestarting = true;

        Time.timeScale = 1f;

        CloseGameOver();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (soundPanel != null)
            soundPanel.SetActive(false);

        Debug.Log("[SoundSettingsUI] 시작 화면으로 이동: " + startSceneName);

        AsyncOperation operation = SceneManager.LoadSceneAsync(
            startSceneName,
            LoadSceneMode.Single
        );

        if (operation == null)
        {
            Debug.LogError("[SoundSettingsUI] Start 씬 로드 실패");
            isRestarting = false;
            yield break;
        }

        yield return operation;

        Time.timeScale = 1f;
        isRestarting = false;
    }

    // ==============================
    // 게임 종료 버튼
    // ==============================
    public void QuitGame()
    {
        Debug.Log("[SoundSettingsUI] 게임 종료");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
