
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform target;

    [Header("카메라 시야")]
    [SerializeField, Min(1f)]
    private float cameraSize = 7f;

    [Header("Y축 설정")]
    [Tooltip("체크하면 플레이어의 Y축도 따라갑니다.")]
    [SerializeField] private bool followTargetY = false;

    [Tooltip("Y축 추적을 끄면 이 위치에 카메라가 고정됩니다.")]
    [SerializeField] private float fixedYPosition = 0f;

    [Header("카메라 움직임")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [SerializeField, Min(0f)]
    private float smoothSpeed = 8f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        ApplyCameraSettings();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        FindPlayer();
        SnapToPlayer();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 바뀌면 플레이어를 다시 찾습니다.
        target = null;

        FindPlayer();

        ApplyCameraSettings();
        SnapToPlayer();
    }

    private void ApplyCameraSettings()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = cameraSize;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            FindPlayer();

            if (target == null)
                return;
        }

        Vector3 targetPosition = GetCameraTargetPosition();

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }

    private Vector3 GetCameraTargetPosition()
    {
        float targetY = followTargetY
            ? target.position.y + offset.y
            : fixedYPosition;

        return new Vector3(
            target.position.x + offset.x,
            targetY,
            offset.z
        );
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            target = null;
            Debug.LogWarning("[CameraFollow] Player를 찾을 수 없습니다.");
        }
    }

    private void SnapToPlayer()
    {
        if (target == null)
            return;

        transform.position = GetCameraTargetPosition();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        SnapToPlayer();
    }
}
