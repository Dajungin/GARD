
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

    [Header("플레이어 화면 위치")]
    [Tooltip("양수일수록 플레이어가 화면 아래쪽에 보입니다.")]
    [SerializeField] private float verticalOffset = 2f;

    [Header("카메라 움직임")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField, Min(0f)] private float smoothSpeed = 8f;

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
        FindPlayer();
        SnapToPlayer();
        ApplyCameraSettings();
    }

    private void ApplyCameraSettings()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (cam != null && cam.orthographic)
            cam.orthographicSize = cameraSize;
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
        Vector3 position = target.position + offset;

        // 카메라가 플레이어보다 위를 바라보게 하여
        // 플레이어가 화면 아래쪽에 보이도록 함
        position.y += verticalOffset;

        return position;
    }

    private void FindPlayer()
    {
        if (target != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            target = player.transform;
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
