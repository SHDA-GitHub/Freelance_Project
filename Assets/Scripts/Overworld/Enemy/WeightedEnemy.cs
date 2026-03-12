using UnityEngine;

[System.Serializable]
public class WeightedEnemy
{
    public EnemyPreset enemy;
    [Range(0f, 1f)]
    public float spawnChance = 1f;
}