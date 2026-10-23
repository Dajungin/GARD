
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform target;

    [Header("카메라 설정")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [SerializeField, Min(0f)]
    private float smoothSpeed = 8f;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        FindPlayer();

        if (target != null)
            SnapToPlayer();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환 후 기존 플레이어를 다시 확인
        FindPlayer();

        if (target != null)
            SnapToPlayer();
    }

    private void LateUpdate()
    {
        // 플레이어를 찾지 못했다면 계속 탐색
        if (target == null)
        {
            FindPlayer();

            if (target == null)
                return;

            SnapToPlayer();
        }

        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }

    private void FindPlayer()
    {
        // 기존 대상이 유효하면 유지
        if (target != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
            Debug.Log("CameraFollow: 플레이어 추적 연결 완료", this);
        }
        else
        {
            Debug.LogWarning(
                "CameraFollow: Player 태그가 붙은 활성 오브젝트를 찾을 수 없습니다.",
                this
            );
        }
    }

    private void SnapToPlayer()
    {
        if (target == null)
            return;

        transform.position = target.position + offset;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        SnapToPlayer();
    }
}
