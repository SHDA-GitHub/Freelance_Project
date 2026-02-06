using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyLoadout", menuName = "Enemies/Enemy Loadout")]
public class EnemyLoadout : ScriptableObject
{
    public string enemyName;

    [Header("AI Behavior Restrictions")]
    public bool canUseSteal = false;

    [Tooltip("Cards this enemy is allowed to use")]
    public List<AttackData> allowedCards = new List<AttackData>();

    public enum EnemyType
    {
        Normal,
        Elite,
        Boss
    }

    public EnemyType enemyType;
}
