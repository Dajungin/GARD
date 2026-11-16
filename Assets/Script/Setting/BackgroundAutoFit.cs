
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundAutoFit : MonoBehaviour
{
    [Header("카메라 설정")]
    [SerializeField] private Camera targetCamera;

    [Header("카메라 따라가기")]
    [SerializeField] private bool followCamera = true;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private float originalZ;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        originalScale = transform.localScale;
        originalZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;

            if (targetCamera == null)
                return;
        }

        AdjustBackground();

        if (followCamera)
        {
            Vector3 cameraPosition = targetCamera.transform.position;

            transform.position = new Vector3(
                cameraPosition.x,
                cameraPosition.y,
                originalZ
            );
        }
    }

    private void AdjustBackground()
    {
        if (spriteRenderer == null ||
            spriteRenderer.sprite == null ||
            !targetCamera.orthographic)
            return;

        // 현재 카메라가 보여주는 화면 크기
        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;

        // 원본 스프라이트 크기
        float spriteWidth =
            spriteRenderer.sprite.bounds.size.x;

        float spriteHeight =
            spriteRenderer.sprite.bounds.size.y;

        if (spriteWidth <= 0f || spriteHeight <= 0f)
            return;

        // 가로와 세로를 각각 화면 크기에 맞춤
        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;

        transform.localScale = new Vector3(
            originalScale.x * scaleX,
            originalScale.y * scaleY,
            originalScale.z
        );
    }
}
