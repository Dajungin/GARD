
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("오디오 소스")]
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    [Header("기본 전체 음량")]
    [SerializeField, Range(0f, 1f)]
    private float defaultMasterVolume = 1f;

    // 저장 키
    private const string MASTER_KEY = "Audio_MasterVolume";

    // 현재 전체 음량
    public float MasterVolume { get; private set; }

    private void Awake()
    {
        // 중복 AudioManager 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬이 바뀌어도 유지
        DontDestroyOnLoad(gameObject);

        // 오디오 소스 생성
        CreateAudioSources();

        // 저장된 전체 음량 불러오기
        LoadVolumeSettings();

        // 전체 음량 적용
        ApplyAllVolumes();
    }

    // 오디오 소스 생성
    private void CreateAudioSources()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        // 배경음 설정
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;

        // 효과음 설정
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
    }

    // 저장된 전체 음량 불러오기
    private void LoadVolumeSettings()
    {
        MasterVolume = Mathf.Clamp01(
            PlayerPrefs.GetFloat(
                MASTER_KEY,
                defaultMasterVolume
            )
        );
    }

    // 전체 음량 변경
    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);

        // 음량 저장
        PlayerPrefs.SetFloat(MASTER_KEY, MasterVolume);
        PlayerPrefs.Save();

        // 음량 적용
        ApplyAllVolumes();
    }

    // 전체 음량 적용
    private void ApplyAllVolumes()
    {
        AudioListener.volume = MasterVolume;
    }

    // 전체 음량 초기화
    public void ResetVolume()
    {
        SetMasterVolume(defaultMasterVolume);
    }

    // 기존 코드 호환용 초기화 함수
    public void ResetVolumeSettings()
    {
        ResetVolume();
    }

    // 배경음 재생
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("재생할 배경음이 없습니다.");
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // 배경음 정지
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    // 효과음 재생
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("재생할 효과음이 없습니다.");
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    // 효과음 재생 (개별 음량 지정)
    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volume)
        );
    }
}
