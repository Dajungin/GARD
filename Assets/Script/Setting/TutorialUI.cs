
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [Header("튜토리얼 패널")]
    [SerializeField] private GameObject tutorialPanel;

    [Header("시작 화면 패널")]
    [SerializeField] private GameObject startPanel;

    private void Start()
    {
        // 처음에는 튜토리얼을 닫아 둠
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    // 튜토리얼 열기
    public void OpenTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        if (startPanel != null)
            startPanel.SetActive(false);
    }

    // 튜토리얼 닫기
    public void CloseTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (startPanel != null)
            startPanel.SetActive(true);
    }
}
