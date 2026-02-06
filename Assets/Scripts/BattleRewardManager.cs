using System.Collections.Generic;
using UnityEngine;

public class BattleRewardManager : MonoBehaviour
{
    public static BattleRewardManager Instance;

    [Header("Player")]
    [SerializeField] private PlayerCardCollection playerCollection;

    [Header("Enemy Reward Tables")]
    [SerializeField] private List<EnemyRewardMapping> enemyRewardMappings = new();

    private Dictionary<EnemyLoadout, EnemyRewardMapping> rewardLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BuildLookup();
    }

    private void BuildLookup()
    {
        rewardLookup = new Dictionary<EnemyLoadout, EnemyRewardMapping>();

        foreach (var entry in enemyRewardMappings)
        {
            if (entry.enemyLoadout == null) continue;

            if (!rewardLookup.ContainsKey(entry.enemyLoadout))
                rewardLookup.Add(entry.enemyLoadout, entry);
        }
    }
    private List<AttackData> BuildRewardPool(EnemyRewardMapping mapping)
    {
        List<AttackData> pool = new();

        foreach (var card in mapping.rewardCards)
        {
            if (card == null) continue;

            if (!playerCollection.HasCard(card))
                pool.Add(card);
        }

        return pool;
    }

    public void TryGiveEnemyReward(EnemyAI enemy)
    {
        if (enemy == null || enemy.loadout == null)
            return;

        if (!rewardLookup.TryGetValue(enemy.loadout, out var mapping))
        {
            Debug.LogWarning($"No reward mapping found for loadout: {enemy.loadout.name}");
            return;
        }

        if (Random.value > mapping.dropChance)
        {
            Debug.Log("No reward dropped.");
            return;
        }

        List<AttackData> rewardPool = BuildRewardPool(mapping);

        if (rewardPool.Count == 0)
        {
            Debug.Log("No valid reward cards available.");
            return;
        }

        int options = mapping.rewardOptions;
        List<AttackData> rolledOptions = PickRandomOptions(rewardPool, options);
        AttackData finalReward = rolledOptions[Random.Range(0, rolledOptions.Count)];

        GiveCard(finalReward);
    }
    private List<AttackData> PickRandomOptions(List<AttackData> pool, int count)
    {
        List<AttackData> tempPool = new List<AttackData>(pool);
        List<AttackData> result = new List<AttackData>();

        count = Mathf.Min(count, tempPool.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, tempPool.Count);
            result.Add(tempPool[index]);
            tempPool.RemoveAt(index);
        }

        return result;
    }

    private void GiveCard(AttackData card)
    {
        if (card == null) return;

        if (!playerCollection.HasCard(card))
        {
            playerCollection.AddCard(card);
            Debug.Log($"New card obtained: {card.CardName}");
        }
        else
        {
            Debug.Log($"Duplicate card skipped: {card.CardName}");
        }
    }
}