using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSceneTrigger : MonoBehaviour
{
    [SerializeField] private string battleSceneName = "Battle Scene";
    [SerializeField] private OverworldEnemyPatrolScript OEPS;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            BattleDataBridge.UpcomingEnemyPreset = OEPS.enemyType;
            SceneManager.LoadScene(battleSceneName);
        }
    }
}