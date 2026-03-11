using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnemyLoadout", menuName = "Battle/Enemy Loadout")]
public class EnemyLoadout : ScriptableObject
{
    public string enemyName;

    [Header("Attack Slots")]
    public List<Attack> attackSlots = new List<Attack>();

    public Attack GetRandomAttack()
    {
        if (attackSlots == null || attackSlots.Count == 0)
            return null;

        return attackSlots[Random.Range(0, attackSlots.Count)];
    }
}