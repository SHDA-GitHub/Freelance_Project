using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyLocator : MonoBehaviour
{
    [SerializeField] private OverworldEnemyPatrolScript enemy;
    [SerializeField] private GameObject enemyObject;

    private void Awake()
    {
        if (enemy == null)
            enemy = GetComponentInParent<OverworldEnemyPatrolScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemy?.SetPlayerInRange(other.transform, true);
            if (enemyObject != null)
                enemyObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            enemy?.SetPlayerInRange(null, false);
    }
}