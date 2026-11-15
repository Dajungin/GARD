
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScreenSettings : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private TMP_Dropdown screenModeDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Button resetSettingsButton;

    [Header("창 설정")]
    [SerializeField] private BorderlessWindow windowController;

    [Header("초기 설정")]
    [SerializeField] private int defaultWindowWidth = 1280;
    [SerializeField] private int defaultWindowHeight = 720;

    [Tooltip("0: 전체 화면, 1: 테두리 없는 창, 2: 일반 창")]
    [SerializeField] private int defaultScreenMode = 0;

    private List<Vector2Int> availableResolutions =
        new List<Vector2Int>();

    private Coroutine windowCoroutine;

    private const string MODE_KEY = "ScreenMode";
    private const string WIDTH_KEY = "ScreenWidth";
    private const string HEIGHT_KEY = "ScreenHeight";

    private void Start()
    {
        if (screenModeDropdown == null ||
            resolutionDropdown == null ||
            resetSettingsButton == null ||
            windowController == null)
        {
            Debug.LogError("ScreenSettings Inspector 연결을 확인하세요.");
            return;
        }

        // 화면 모드 목록
        screenModeDropdown.ClearOptions();

        screenModeDropdown.AddOptions(new List<string>
        {
            "전체 화면",
            "테두리 없는 창 모드",
            "일반 창 모드"
        });

        CreateResolutionList();

        // 이벤트 연결
        screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        resetSettingsButton.onClick.AddListener(ResetSettings);

        LoadSettings();
    }

    // 해상도 목록 생성
    private void CreateResolutionList()
    {
        availableResolutions.Clear();

        Resolution[] resolutions = Screen.resolutions;

        foreach (Resolution resolution in resolutions)
        {
            AddResolution(resolution.width, resolution.height);
        }

        AddResolution(Display.main.systemWidth, Display.main.systemHeight);
        AddResolution(defaultWindowWidth, defaultWindowHeight);
        AddResolution(Screen.width, Screen.height);

        availableResolutions.Sort((a, b) =>
        {
            int result = a.x.CompareTo(b.x);

            if (result != 0)
                return result;

            return a.y.CompareTo(b.y);
        });

        List<string> options = new List<string>();

        foreach (Vector2Int resolution in availableResolutions)
        {
            options.Add($"{resolution.x} x {resolution.y}");
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
    }

    private void AddResolution(int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;

        Vector2Int resolution = new Vector2Int(width, height);

        if (!availableResolutions.Contains(resolution))
        {
            availableResolutions.Add(resolution);
        }
    }

    // 저장된 설정 불러오기
    private void LoadSettings()
    {
        int mode = PlayerPrefs.GetInt(MODE_KEY, defaultScreenMode);

        int width = PlayerPrefs.GetInt(
            WIDTH_KEY, defaultWindowWidth);

        int height = PlayerPrefs.GetInt(
            HEIGHT_KEY, defaultWindowHeight);

        mode = Mathf.Clamp(mode, 0, 2);

        screenModeDropdown.SetValueWithoutNotify(mode);

        int resolutionIndex = FindResolutionIndex(width, height);

        resolutionDropdown.SetValueWithoutNotify(resolutionIndex);

        ApplySettings();
    }

    private int FindResolutionIndex(int width, int height)
    {
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].x == width &&
                availableResolutions[i].y == height)
            {
                return i;
            }
        }

        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].x == defaultWindowWidth &&
                availableResolutions[i].y == defaultWindowHeight)
            {
                return i;
            }
        }

        return 0;
    }

    private void OnScreenModeChanged(int index)
    {
        ApplySettings();
    }

    private void OnResolutionChanged(int index)
    {
        if (screenModeDropdown.value != 0)
        {
            ApplySettings();
        }
    }

    // 화면 설정 적용
    private void ApplySettings()
    {
        if (availableResolutions.Count == 0)
            return;

        int mode = screenModeDropdown.value;
        int resolutionIndex = Mathf.Clamp(
            resolutionDropdown.value,
            0,
            availableResolutions.Count - 1
        );

        int selectedWidth = availableResolutions[resolutionIndex].x;
        int selectedHeight = availableResolutions[resolutionIndex].y;

        // 전체 화면이 아니면 해상도 변경 가능
        resolutionDropdown.interactable = mode != 0;

        // 이전 화면 전환 작업만 중지
        if (windowCoroutine != null)
        {
            StopCoroutine(windowCoroutine);
            windowCoroutine = null;
        }

        if (mode == 0)
        {
            // 전체 화면: 모니터 기본 해상도 고정
            int width = Display.main.systemWidth;
            int height = Display.main.systemHeight;

            Screen.SetResolution(
                width,
                height,
                FullScreenMode.FullScreenWindow
            );
        }
        else if (mode == 1)
        {
            // 작은 테두리 없는 창
            windowCoroutine = StartCoroutine(
                windowController.ApplyBorderless(
                    selectedWidth,
                    selectedHeight
                )
            );
        }
        else if (mode == 2)
        {
            // 일반 창
            windowCoroutine = StartCoroutine(
                windowController.ApplyNormal(
                    selectedWidth,
                    selectedHeight
                )
            );
        }

        // 선택한 창 해상도는 별도로 보존
        PlayerPrefs.SetInt(MODE_KEY, mode);
        PlayerPrefs.SetInt(WIDTH_KEY, selectedWidth);
        PlayerPrefs.SetInt(HEIGHT_KEY, selectedHeight);

        PlayerPrefs.Save();

        Debug.Log(
            $"화면 모드: {mode}, 선택 해상도: " +
            $"{selectedWidth} x {selectedHeight}"
        );
    }

    // 설정 초기화
    public void ResetSettings()
    {
        int resolutionIndex = FindResolutionIndex(
            defaultWindowWidth,
            defaultWindowHeight
        );

        screenModeDropdown.SetValueWithoutNotify(
            Mathf.Clamp(defaultScreenMode, 0, 2)
        );

        resolutionDropdown.SetValueWithoutNotify(resolutionIndex);

        ApplySettings();

        Debug.Log("화면 설정 초기화 완료");
    }

    private void OnDestroy()
    {
        if (screenModeDropdown != null)
        {
            screenModeDropdown.onValueChanged.RemoveListener(
                OnScreenModeChanged
            );
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveListener(
                OnResolutionChanged
            );
        }

        if (resetSettingsButton != null)
        {
            resetSettingsButton.onClick.RemoveListener(ResetSettings);
        }
    }
}
