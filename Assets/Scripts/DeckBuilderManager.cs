using UnityEngine;

public class DeckBuilderManager : MonoBehaviour
{
    public static DeckBuilderManager Instance;

    public PlayerDeck playerDeck;
    public PlayerCardCollection playerCollection;
    public AttackLibrary cardLibrary;

    [Header("Deck Builder UI")]
    public Transform cardGridParent;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ClearPlayerDeck();
        PopulateDeckBuilder();
    }

    private void ClearPlayerDeck()
    {
        playerDeck.selectedCards.Clear();
    }

    public void PopulateDeckBuilder()
    {
        foreach (Transform child in cardGridParent)
        {
            Destroy(child.gameObject);
        }
        foreach (AttackData ownedCard in playerCollection.ownedCards)
        {
            GameObject deckPrefab = GetDeckBuilderPrefab(ownedCard);

            if (deckPrefab == null)
            {
                Debug.LogWarning($"No deck builder prefab for {ownedCard.CardName}");
                continue;
            }

            GameObject cardGO = Instantiate(deckPrefab, cardGridParent);

            DeckBuilderCard deckBuilderCard = cardGO.GetComponent<DeckBuilderCard>();
            if (deckBuilderCard == null)
            {
                Debug.LogError($"Deck prefab {deckPrefab.name} has no DeckBuilderCard!");
                continue;
            }

            deckBuilderCard.cardData = ownedCard;
        }
    }

    private GameObject GetDeckBuilderPrefab(AttackData cardData)
    {
        foreach (var entry in cardLibrary.allPlayerattacks)
        {
            if (entry.attackData == cardData)
                return entry.deckBuilderCardPrefab;
        }

        return null;
    }

    public bool CanAddCard(AttackData card)
    {
        return playerDeck.selectedCards.Count < playerDeck.maxDeckSize;
    }

    public void AddCardToPlayerDeck(AttackData card)
    {
        if (!playerCollection.HasCard(card)) return;
        if (!CanAddCard(card)) return;

        if (!playerDeck.selectedCards.Contains(card))
        {
            playerDeck.selectedCards.Add(card);
        }
    }
}