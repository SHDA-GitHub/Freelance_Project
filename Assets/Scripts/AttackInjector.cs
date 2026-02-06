using UnityEngine;

public class AttackInjector : MonoBehaviour
{
    public AttackLibrary attackLibrary;

    public Transform[] threeAttackSlots;
    public Transform[] fourAttackSlots;

    public BattleAttackSpawner battleAttackSpawner;
    public bool Spawn = true;

    public int numberOfAttacksToInject = 3;

    void Start()
    {
        if (Spawn)
        {
            InjectAttacks();
        }
    }

    public void InjectAttacks()
    {
        Transform[] slotsToUse = GetSlotsForAttackCount(numberOfAttacksToInject);

        for (int i = 0; i < numberOfAttacksToInject && i < slotsToUse.Length; i++)
        {
            AttackData randomAttackData =
                attackLibrary.allPlayerattacks[Random.Range(0, attackLibrary.allPlayerattacks.Count)].attackData;

            battleAttackSpawner.SpawnAttack(randomAttackData, slotsToUse[i]);
        }
    }

    Transform[] GetSlotsForAttackCount(int count)
    {
        if (count == 4)
            return fourAttackSlots;

        return threeAttackSlots;
    }
}
