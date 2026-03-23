using UnityEngine;
using TMPro;

public class PlayerDataOverworld : MonoBehaviour
{
    public PlayerStatsSO playerStats;
    public string playerName;

    public TMP_Text characterName;
    public TMP_Text hpText;
    public TMP_Text ppText;
    public TMP_Text levelText;
    public TMP_Text expText;

    void Update()
    {
        if (characterName  != null)
        {
            characterName.text = $"{playerStats.characterName}";
        }
        if (hpText != null)
        {
            hpText.text = $"HP: {playerStats.currentHealth}/{playerStats.maxHealth}";
        }
        if (ppText != null)
        {
            ppText.text = $"PP: {playerStats.currentPP}/{playerStats.maxPP}";
        }
        if (levelText != null)
        {
            levelText.text = $"Level: {playerStats.level}";
        }
        if (expText != null)
        {
            expText.text = $"EXP: {playerStats.currentEXP}";
        }
    }
}