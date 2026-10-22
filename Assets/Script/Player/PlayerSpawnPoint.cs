
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        // Scene 창에서 시작 위치를 쉽게 확인할 수 있도록 표시
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.25f);

        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.up * 1f
        );
    }
}
