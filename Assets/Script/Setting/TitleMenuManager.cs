
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenuManager : MonoBehaviour
{
    [Header("게임 시작 시 이동할 씬 이름")]
    [SerializeField] private string nextSceneName = "Stage1";

    public void StartGame()
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("이동할 씬 이름을 입력해 주세요.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }
}
