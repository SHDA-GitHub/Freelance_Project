using UnityEngine;
using TMPro;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI ppText;
    [SerializeField] private TextMeshProUGUI nameText;

    private CharacterStats currentCharacter;

    public void SetCharacter(CharacterStats character)
    {
        currentCharacter = character;
        UpdateHUD();
    }

    public void UpdateHUD()
    {
        if (currentCharacter == null) return;

        nameText.text = currentCharacter.characterName;
        hpText.text = $"HP: {currentCharacter.currentHealth}/{currentCharacter.maxHealth}";
        ppText.text = $"PP: {currentCharacter.currentPP}/{currentCharacter.maxPP}";
    }
}