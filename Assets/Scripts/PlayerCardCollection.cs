using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Player Card Collection")]
public class PlayerCardCollection : ScriptableObject
{
    [Header("Starter Cards (Never Removed)")]
    public List<AttackData> starterCards = new List<AttackData>();

    [Header("Current Owned Cards")]
    public List<AttackData> ownedCards = new List<AttackData>();

    public bool HasCard(AttackData card)
    {
        return ownedCards.Contains(card);
    }

    public void AddCard(AttackData card)
    {
        if (!ownedCards.Contains(card))
            ownedCards.Add(card);
    }

    public void ResetCards()
    {
        ownedCards.Clear();

        foreach (var card in starterCards)
        {
            if (card != null && !ownedCards.Contains(card))
                ownedCards.Add(card);
        }
    }
}
