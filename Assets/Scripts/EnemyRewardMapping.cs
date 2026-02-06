using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyRewardMapping
{
    [Tooltip("Which enemy loadout this reward pool applies to")]
    public EnemyLoadout enemyLoadout;

    [Header("Reward Cards")]
    public List<AttackData> rewardCards = new List<AttackData>();

    [Header("Drop Settings")]
    [Range(0f, 1f)]
    public float dropChance = 1f;

    [Tooltip("Base number of options to roll")]
    public int rewardOptions = 3;
}