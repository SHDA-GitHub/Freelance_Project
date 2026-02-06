using UnityEngine;

public class DeckBuilderApplier : MonoBehaviour
{
    public PlayerDeck playerDeck;
    public GameManager gameManager;

    void Start()
    {
        ApplyDeck();
    }

    public void ApplyDeck()
    {
        foreach (Attack card in gameManager.playerCards)
        {
            bool shouldBeActive = playerDeck.selectedCards.Contains(card.attackData);
            card.gameObject.SetActive(shouldBeActive);
        }
    }
}
