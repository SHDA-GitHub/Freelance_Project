using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public List<PlayerStatsSO> partyMembers = new List<PlayerStatsSO>();
    private int currentIndex = 0;

    public PlayerDataOverworld uiDisplay;

    void Start()
    {
        if (partyMembers.Count > 0)
        {
            UpdateUI();
        }
    }

    public void NextMember()
    {
        if (partyMembers.Count == 0) return;

        currentIndex++;
        if (currentIndex >= partyMembers.Count)
            currentIndex = 0;

        UpdateUI();
    }

    public void PreviousMember()
    {
        if (partyMembers.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = partyMembers.Count - 1;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (uiDisplay != null)
        {
            uiDisplay.playerStats = partyMembers[currentIndex];
        }
    }

    public PlayerStatsSO GetCurrentMember()
    {
        if (partyMembers.Count == 0) return null;
        return partyMembers[currentIndex];
    }
}