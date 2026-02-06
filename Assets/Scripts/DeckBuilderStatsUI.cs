using TMPro;
using UnityEngine;

public class DeckBuilderStatsUI : MonoBehaviour
{
    public PlayerStats playerStats;

    public TMP_Text levelText;
    public TMP_Text hpText;
    public TMP_Text manaText;
    public TMP_Text expText;
    public TMP_Text enemiesText;

    void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        levelText.text = $"Level: {playerStats.currentLevel}";
        hpText.text = $"HP: {playerStats.currentHealth} / {playerStats.maxHealth}";
        manaText.text = $"Mana: {playerStats.currentMana} / {playerStats.maxMana}";
        expText.text = $"EXP: {playerStats.currentEXP} / {playerStats.EXPThreshold}";
        enemiesText.text = $"Enemies Slain: {playerStats.enemiesSlain}";
    }
}