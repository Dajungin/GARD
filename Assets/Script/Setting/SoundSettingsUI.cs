
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoundSettingsUI : MonoBehaviour
{
    [Header("오디오 매니저")]
    [SerializeField]
    private AudioManager_PlayerController audioManager;

    [Header("전체 사운드")]
    [SerializeField]
    private Slider masterSlider;

    [SerializeField]
    private TMP_Text masterPercentText;

    [Header("배경음")]
    [SerializeField]
    private Slider bgmSlider;

    [SerializeField]
    private TMP_Text bgmPercentText;

    [Header("효과음")]
    [SerializeField]
    private Slider sfxSlider;

    [SerializeField]
    private TMP_Text sfxPercentText;

    // 기존 AudioManager에서 사용하는 저장 키
    private const string MASTER_KEY = "Audio_PlayerController_Master";
    private const string BGM_KEY = "Audio_PlayerController_BGM";
    private const string SFX_KEY = "Audio_PlayerController_SFX";

    private const float DEFAULT_VOLUME = 0.5f;

    private bool initialized = false;

    private void Start()
    {
        InitializeSettings();
    }

    private void InitializeSettings()
    {
        if (audioManager == null)
        {
            Debug.LogError("AudioManager_PlayerController가 연결되지 않았습니다.");
            return;
        }

        // 저장된 음량 불러오기
        float master = PlayerPrefs.GetFloat(
            MASTER_KEY, DEFAULT_VOLUME);

        float bgm = PlayerPrefs.GetFloat(
            BGM_KEY, DEFAULT_VOLUME);

        float sfx = PlayerPrefs.GetFloat(
            SFX_KEY, DEFAULT_VOLUME);

        // 슬라이더 범위 설정
        SetupSlider(masterSlider);
        SetupSlider(bgmSlider);
        SetupSlider(sfxSlider);

        // 기존 이벤트 제거
        masterSlider.onValueChanged.RemoveListener(SetMasterVolume);
        bgmSlider.onValueChanged.RemoveListener(SetBGMVolume);
        sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);

        // 저장된 값 적용
        masterSlider.SetValueWithoutNotify(master);
        bgmSlider.SetValueWithoutNotify(bgm);
        sfxSlider.SetValueWithoutNotify(sfx);

        // 매니저에 음량 적용
        audioManager.SetMasterVolume(master);
        audioManager.SetBgmVolume(bgm);
        audioManager.SetSfxVolume(sfx);

        // 슬라이더 이벤트 연결
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // 퍼센트 표시
        UpdateAllLabels();

        initialized = true;
    }

    private void SetupSlider(Slider slider)
    {
        if (slider == null)
        {
            Debug.LogError("사운드 슬라이더가 연결되지 않았습니다.");
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    // 전체 사운드
    public void SetMasterVolume(float value)
    {
        if (!initialized) return;

        audioManager.SetMasterVolume(value);

        PlayerPrefs.SetFloat(MASTER_KEY, value);

        UpdateLabel(masterPercentText, value);

        PlayerPrefs.Save();
    }

    // 배경음
    public void SetBGMVolume(float value)
    {
        if (!initialized) return;

        audioManager.SetBgmVolume(value);

        PlayerPrefs.SetFloat(BGM_KEY, value);

        UpdateLabel(bgmPercentText, value);

        PlayerPrefs.Save();
    }

    // 효과음
    public void SetSFXVolume(float value)
    {
        if (!initialized) return;

        audioManager.SetSfxVolume(value);

        PlayerPrefs.SetFloat(SFX_KEY, value);

        UpdateLabel(sfxPercentText, value);

        PlayerPrefs.Save();
    }

    // 전체 퍼센트 표시
    private void UpdateAllLabels()
    {
        UpdateLabel(masterPercentText, masterSlider.value);
        UpdateLabel(bgmPercentText, bgmSlider.value);
        UpdateLabel(sfxPercentText, sfxSlider.value);
    }

    private void UpdateLabel(TMP_Text text, float value)
    {
        if (text == null) return;

        int percent = Mathf.RoundToInt(value * 100f);

        text.text = $"{percent}%";
    }

    // 설정 초기화
    public void ResetSoundSettings()
    {
        if (audioManager == null) return;

        masterSlider.SetValueWithoutNotify(DEFAULT_VOLUME);
        bgmSlider.SetValueWithoutNotify(DEFAULT_VOLUME);
        sfxSlider.SetValueWithoutNotify(DEFAULT_VOLUME);

        audioManager.SetMasterVolume(DEFAULT_VOLUME);
        audioManager.SetBgmVolume(DEFAULT_VOLUME);
        audioManager.SetSfxVolume(DEFAULT_VOLUME);

        PlayerPrefs.SetFloat(MASTER_KEY, DEFAULT_VOLUME);
        PlayerPrefs.SetFloat(BGM_KEY, DEFAULT_VOLUME);
        PlayerPrefs.SetFloat(SFX_KEY, DEFAULT_VOLUME);

        UpdateAllLabels();

        PlayerPrefs.Save();
    }
}
