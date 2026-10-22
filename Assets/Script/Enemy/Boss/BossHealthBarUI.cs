
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("연결할 UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text bossNameText;

    [Header("표시 설정")]
    [SerializeField] private string bossName = "RUBY WITCH";
    [SerializeField] private bool showHealthNumber = true;

    private BossHealth bossHealth;

    private void Start()
    {
        // 씬에 있는 보스 찾기
        bossHealth = FindFirstObjectByType<BossHealth>();

        if (bossHealth == null)
        {
            Debug.LogError("BossHealthBarUI: 보스를 찾을 수 없습니다.", this);
            return;
        }

        // 체력 변경 이벤트 등록
        bossHealth.OnHealthChanged += UpdateHealth;

        // 처음 체력 표시
        UpdateHealth(bossHealth.CurrentHP, bossHealth.MaxHP);
    }

    private void UpdateHealth(int currentHP, int maxHP)
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHP;
            healthSlider.value = currentHP;
        }

        if (bossNameText != null)
        {
            if (showHealthNumber)
            {
                bossNameText.text =
                    $"{bossName}  {currentHP} / {maxHP}";
            }
            else
            {
                bossNameText.text = bossName;
            }
        }
    }

    private void OnDestroy()
    {
        // UI가 제거될 때 이벤트 구독 해제
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= UpdateHealth;
        }
    }
}
