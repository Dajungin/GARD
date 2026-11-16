
using UnityEngine;

public class StageEnemySetup : MonoBehaviour
{
    private void Start()
    {
        if (StageEnemyManager.Instance == null)
        {
            Debug.LogError(
                "[StageEnemySetup] StageEnemyManager가 없습니다."
            );

            return;
        }

        Debug.Log(
            "[StageEnemySetup] 스테이지 초기화 완료. 몬스터 생성은 MonsterSpawner가 담당합니다."
        );
    }
}
