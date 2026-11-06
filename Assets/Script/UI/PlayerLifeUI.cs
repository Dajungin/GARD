
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerLifeUI : MonoBehaviour
{
    [Header("플레이어 체력")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("UI 텍스트")]
    [SerializeField] private TMP_Text lifeText;

    private void OnEnable()
    {
        // 씬이 변경될 때마다 플레이어를 다시 찾음
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // 처음 실행할 때 플레이어 연결
        FindPlayer();

        UpdateLifeUI();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로딩이 완료되면 실행
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();

        UpdateLifeUI();
    }

    // 현재 씬에 있는 기존 플레이어 찾기
    private void FindPlayer()
    {
        if (playerHealth != null)
            return;

        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning(
                "PlayerLifeUI: 플레이어의 PlayerHealth를 찾을 수 없습니다."
            );
        }
        else
        {
            Debug.Log("PlayerLifeUI: 기존 플레이어 연결 성공!");
        }
    }

    // 목숨 UI 갱신
    private void Update()
    {
        if (playerHealth == null)
        {
            FindPlayer();
            return;
        }

        UpdateLifeUI();
    }

    private void UpdateLifeUI()
    {
        if (playerHealth == null || lifeText == null)
            return;

        lifeText.text =
            $"HP : {playerHealth.CurrentHP} / {playerHealth.MaxHP}";
    }
}
