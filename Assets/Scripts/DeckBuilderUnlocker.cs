using UnityEngine;

public class DeckBuilderUnlocker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameObject lockObject;
    [SerializeField] private GameObject cardSlot;

    [Header("Settings")]
    [SerializeField] private int requiredLevel = 4;

    private void Start()
    {
        CheckUnlock();
    }

    private void CheckUnlock()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats reference is missing!");
            return;
        }

        if (playerStats.currentLevel >= requiredLevel)
        {
            UnlockSlot();
        }
    }

    private void UnlockSlot()
    {
        if (lockObject != null)
            lockObject.SetActive(false);

        if (cardSlot != null)
            cardSlot.tag = "CardSlotOpen";
    }
}