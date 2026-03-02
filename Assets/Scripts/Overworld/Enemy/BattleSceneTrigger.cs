using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSceneTrigger : MonoBehaviour
{
    [SerializeField] private string battleSceneName = "Battle Scene";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(battleSceneName);
        }
    }
}