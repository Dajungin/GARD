
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class PanelAutoResize : MonoBehaviour
{
    [Header("기준 해상도")]
    [SerializeField]
    private Vector2 referenceResolution =
        new Vector2(1920f, 1080f);

    [Header("기준 패널 크기")]
    [SerializeField]
    private Vector2 referencePanelSize =
        new Vector2(900f, 650f);

    [Header("최소 / 최대 크기")]
    [SerializeField]
    private Vector2 minimumSize =
        new Vector2(300f, 250f);

    [SerializeField]
    private Vector2 maximumSize =
        new Vector2(1800f, 1200f);

    [Header("내부 UI 그룹")]
    [SerializeField]
    private RectTransform uiContent;

    private RectTransform panelRect;

    private Vector2 lastScreenSize;

    private void Awake()
    {
        panelRect = GetComponent<RectTransform>();
        UpdateLayout();
    }

    private void Start()
    {
        UpdateLayout();
    }

    private void OnEnable()
    {
        panelRect = GetComponent<RectTransform>();
        UpdateLayout();
    }

    private void Update()
    {
        Vector2 currentScreenSize =
            new Vector2(Screen.width, Screen.height);

        if (currentScreenSize != lastScreenSize)
        {
            UpdateLayout();
        }
    }

    private void UpdateLayout()
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        if (referenceResolution.x <= 0 ||
            referenceResolution.y <= 0)
            return;

        // 화면 가로, 세로 비율 계산
        float scaleX =
            Screen.width / referenceResolution.x;

        float scaleY =
            Screen.height / referenceResolution.y;

        // 가로세로 비율을 유지하는 공통 배율
        float scale = Mathf.Min(scaleX, scaleY);

        // 패널 크기 계산
        float width = Mathf.Clamp(
            referencePanelSize.x * scale,
            minimumSize.x,
            maximumSize.x
        );

        float height = Mathf.Clamp(
            referencePanelSize.y * scale,
            minimumSize.y,
            maximumSize.y
        );

        // 패널 크기 적용
        panelRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            width
        );

        panelRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            height
        );

        // 내부 UI 전체를 동일한 비율로 조절
        if (uiContent != null)
        {
            uiContent.localScale =
                new Vector3(scale, scale, 1f);
        }

        lastScreenSize = new Vector2(
            Screen.width,
            Screen.height
        );
    }
}
