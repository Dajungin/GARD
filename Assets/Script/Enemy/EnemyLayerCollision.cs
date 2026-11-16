
using UnityEngine;

public class EnemyLayerCollision : MonoBehaviour
{
    [Header("충돌 무시 설정")]
    [SerializeField] private string enemyLayerName = "Enemy";

    private void Awake()
    {
        int enemyLayer = LayerMask.NameToLayer(enemyLayerName);

        if (enemyLayer == -1)
        {
            Debug.LogError(
                $"'{enemyLayerName}' 레이어를 찾을 수 없습니다."
            );
            return;
        }

        // Enemy 레이어끼리 물리 충돌 무시
        Physics2D.IgnoreLayerCollision(
            enemyLayer,
            enemyLayer,
            true
        );

        Debug.Log("Enemy끼리 충돌하지 않도록 설정했습니다.");
    }
}
