using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyPreset", menuName = "Battle/Enemy Preset")]
public class EnemyPreset : ScriptableObject
{
    [Tooltip("1 to 3 enemy prefabs")]
    [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();

    public List<GameObject> EnemyPrefabs => enemyPrefabs;

    private void OnValidate()
    {
        if (enemyPrefabs.Count > 3)
            enemyPrefabs.RemoveRange(3, enemyPrefabs.Count - 3);
    }
}