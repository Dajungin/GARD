
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("체력바 UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text bossNameText;

    [Header("보스 이름")]
    [SerializeField] private string bossName = "RUBY WITCH";
    [SerializeField] private bool showHealthNumber = true;

    [Header("체력바 색상")]
    [SerializeField]
    private Color phaseOneColor =
        new Color(0.85f, 0.12f, 0.25f);

    [SerializeField]
    private Color phaseTwoColor =
        new Color(1f, 0.55f, 0.05f);

    [Header("2페이즈 전환 연출")]
    [SerializeField] private GameObject phaseTransitionPanel;
    [SerializeField] private CanvasGroup transitionCanvasGroup;
    [SerializeField] private TMP_Text phaseTransitionText;

    [SerializeField, Min(0.1f)]
    private float transitionDisplayDuration = 2f;

    [SerializeField, Min(0f)]
    private float fadeDuration = 0.25f;

    private BossHealth bossHealth;
    private Coroutine transitionEffectCoroutine;

    private void Start()
    {
        if (phaseTransitionPanel != null)
            phaseTransitionPanel.SetActive(false);

        if (transitionCanvasGroup != null)
            transitionCanvasGroup.alpha = 0f;

        bossHealth = FindFirstObjectByType<BossHealth>();

        if (bossHealth == null)
        {
            Debug.LogError(
                "BossHealthBarUI: 씬에서 BossHealth를 찾지 못했습니다.",
                this
            );
            return;
        }

        bossHealth.OnHealthChanged += UpdateHealth;
        bossHealth.OnPhaseTwoStarted += PlayPhaseTwoEffect;
        bossHealth.OnBossDied += HandleBossDied;

        // UI가 처음 표시될 때 현재 상태 반영
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

        UpdateHealthBarColor();
    }

    private void UpdateHealthBarColor()
    {
        if (fillImage == null)
            return;

        bool phaseTwo =
            bossHealth != null && bossHealth.IsPhaseTwo;

        fillImage.color =
            phaseTwo ? phaseTwoColor : phaseOneColor;
    }

    private void PlayPhaseTwoEffect()
    {
        UpdateHealthBarColor();

        if (phaseTransitionPanel == null)
        {
            Debug.LogWarning(
                "2페이즈 연출 패널이 연결되지 않았습니다.",
                this
            );
            return;
        }

        if (transitionEffectCoroutine != null)
            StopCoroutine(transitionEffectCoroutine);

        transitionEffectCoroutine =
            StartCoroutine(PhaseTwoEffectRoutine());
    }

    private IEnumerator PhaseTwoEffectRoutine()
    {
        phaseTransitionPanel.SetActive(true);

        if (phaseTransitionText != null)
            phaseTransitionText.text = "PHASE 2";

        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;

            float fadeInTime = Mathf.Min(
                fadeDuration,
                transitionDisplayDuration * 0.5f
            );

            // 서서히 나타나기
            if (fadeInTime > 0f)
            {
                float elapsed = 0f;

                while (elapsed < fadeInTime)
                {
                    elapsed += Time.unscaledDeltaTime;

                    transitionCanvasGroup.alpha =
                        Mathf.Clamp01(elapsed / fadeInTime);

                    yield return null;
                }
            }

            transitionCanvasGroup.alpha = 1f;

            // 화면에 표시되는 시간
            float holdTime = Mathf.Max(
                0f,
                transitionDisplayDuration - fadeInTime
            );

            float holdElapsed = 0f;

            while (holdElapsed < holdTime)
            {
                holdElapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // 서서히 사라지기
            if (fadeInTime > 0f)
            {
                float fadeOutElapsed = 0f;

                while (fadeOutElapsed < fadeInTime)
                {
                    fadeOutElapsed += Time.unscaledDeltaTime;

                    transitionCanvasGroup.alpha =
                        1f - Mathf.Clamp01(
                            fadeOutElapsed / fadeInTime
                        );

                    yield return null;
                }
            }

            transitionCanvasGroup.alpha = 0f;
        }
        else
        {
            yield return new WaitForSecondsRealtime(
                transitionDisplayDuration
            );
        }

        phaseTransitionPanel.SetActive(false);
        transitionEffectCoroutine = null;
    }

    private void HandleBossDied()
    {
        if (transitionEffectCoroutine != null)
        {
            StopCoroutine(transitionEffectCoroutine);
            transitionEffectCoroutine = null;
        }

        if (phaseTransitionPanel != null)
            phaseTransitionPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= UpdateHealth;
            bossHealth.OnPhaseTwoStarted -= PlayPhaseTwoEffect;
            bossHealth.OnBossDied -= HandleBossDied;
        }
    }
}
