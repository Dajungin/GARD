
using UnityEngine;

public class StageEnemySetup : MonoBehaviour
{
    private void Start()
    {
        if (StageEnemyManager.Instance != null)
        {
            StageEnemyManager.Instance.FinishEnemySpawning();
        }
        else
        {
            Debug.LogError("StageEnemyManager가 없습니다.");
        }
    }
}
