using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public List<PlayerStatsSO> partyMembers = new List<PlayerStatsSO>();
    private int currentIndex = 0;

    public PlayerDataOverworld uiDisplay;
    [SerializeField] private TextMeshProUGUI coinText;

    void Start()
    {
        if (partyMembers.Count > 0)
        {
            UpdateUI();
        }
        UpdateCoinUI();
    }

    public void NextMember()
    {
        if (partyMembers.Count == 0) return;

        currentIndex++;
        if (currentIndex >= partyMembers.Count)
            currentIndex = 0;

        UpdateUI();
        UpdateCoinUI();
    }

    public void PreviousMember()
    {
        if (partyMembers.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = partyMembers.Count - 1;

        UpdateUI();
        UpdateCoinUI();
    }

    private void UpdateUI()
    {
        if (uiDisplay != null)
        {
            uiDisplay.playerStats = partyMembers[currentIndex];
        }
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = $"Balance: {CurrencyManager.Instance.GetCoinCount()} Dollars";
    }

    public PlayerStatsSO GetCurrentMember()
    {
        if (partyMembers.Count == 0) return null;
        return partyMembers[currentIndex];
    }
}