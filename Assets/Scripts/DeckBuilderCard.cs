using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckBuilderCard : MonoBehaviour, IPointerClickHandler
{
    public AttackData cardData;

    private bool isPlaced = false;
    private Transform originalParent;
    private Vector3 originalScale;

    private void Awake()
    {
        ResetToGrid();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(HandleCardAction());
    }

    private void OnEnable()
    {
        ResetToGrid();
    }

    private IEnumerator HandleCardAction()
    {
        if (isPlaced)
        {
            RemoveCardFromDeck();
            yield return MoveCardBackToOriginalParent();
        }
        else
        {
            PlaceCardInSlot();
        }
    }

    private void ResetToGrid()
    {
        isPlaced = false;
        if (transform.parent != null && transform.parent.CompareTag("CardSlotClosed"))
        {
            transform.parent.tag = "CardSlotOpen";
        }

        if (originalParent == null)
        {
            originalParent = transform.parent;
        }

        transform.SetParent(originalParent, worldPositionStays: true);
        transform.localScale = originalScale == Vector3.zero ? Vector3.one : originalScale;
    }

    private IEnumerator MoveCardBackToOriginalParent()
    {
        GameObject slot = transform.parent.gameObject;
        slot.tag = "CardSlotOpen";
        yield return null;
        transform.SetParent(originalParent, worldPositionStays: true);
        transform.localScale = originalScale;
        isPlaced = false;
    }

    private void PlaceCardInSlot()
    {
        foreach (var slot in GetOpenCardSlots())
        {
            if (IsSlotAvailable(slot))
            {
                originalParent = transform.parent;
                originalScale = transform.localScale;

                transform.SetParent(slot.transform, worldPositionStays: false);
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;

                isPlaced = true;
                slot.tag = "CardSlotClosed";
                DeckBuilderManager.Instance.AddCardToPlayerDeck(cardData);
                break;
            }
        }
    }

    private GameObject[] GetOpenCardSlots()
    {
        return GameObject.FindGameObjectsWithTag("CardSlotOpen");
    }

    private bool IsSlotAvailable(GameObject slot)
    {
        return slot.tag == "CardSlotOpen" && slot.transform.childCount == 0;
    }

    private void RemoveCardFromDeck()
    {
        DeckBuilderManager.Instance.playerDeck.selectedCards.Remove(cardData);
        Debug.Log($"Card {cardData.CardName} removed from deck.");
    }
}