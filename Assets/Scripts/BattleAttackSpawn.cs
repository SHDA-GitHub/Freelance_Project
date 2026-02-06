using System.Collections.Generic;
using UnityEngine;

public class BattleAttackSpawner : MonoBehaviour
{
    public PlayerDeck playerDeck;
    public GameManager gameManager;

    public Transform[] threeCardSlots;
    public Transform[] fourCardSlots;

    public AttackLibrary cardLibrary;

    private List<GameObject> spawnedCards = new List<GameObject>();

    void Start()
    {
        SpawnPlayerDeckCards();
    }

    void SpawnPlayerDeckCards()
    {
        ClearExistingCards();

        Transform[] slotsToUse = GetSlotsForCardCount(playerDeck.selectedCards.Count);

        for (int i = 0; i < playerDeck.selectedCards.Count && i < slotsToUse.Length; i++)
        {
            AttackData cardData = playerDeck.selectedCards[i];
            Transform slot = slotsToUse[i];

            GameObject cardPrefab = GetCardPrefabFromLibrary(cardData);
            if (cardPrefab == null) continue;

            GameObject newCardGO = Instantiate(cardPrefab, slot);
            newCardGO.transform.localPosition = Vector3.zero;
            newCardGO.transform.localRotation = Quaternion.identity;

            Attack cardComp = newCardGO.GetComponent<Attack>();
            cardComp.attackData = cardData;

            spawnedCards.Add(newCardGO);
        }

        gameManager.RegisterCards();
    }

    Transform[] GetSlotsForCardCount(int count)
    {
        if (count == 4)
            return fourCardSlots;

        return threeCardSlots;
    }
    GameObject GetCardPrefabFromLibrary(AttackData cardData)
    {
        foreach (var entry in cardLibrary.allPlayerattacks)
        {
            if (entry.attackData == cardData)
            {
                return entry.cardPrefab;
            }
        }

        return null;
    }

    void ClearExistingCards()
    {
        foreach (var card in spawnedCards)
        {
            Destroy(card);
        }
        spawnedCards.Clear();
    }

    public void SpawnAttack(AttackData cardData, Transform slot)
    {
        GameObject cardPrefab = GetCardPrefabFromLibrary(cardData);
        if (cardPrefab == null) return;

        GameObject newCardGO = Instantiate(cardPrefab, slot);
        newCardGO.transform.localPosition = Vector3.zero;
        newCardGO.transform.localRotation = Quaternion.identity;

        Attack cardComp = newCardGO.GetComponent<Attack>();
        cardComp.attackData = cardData;

        spawnedCards.Add(newCardGO);
    }

}