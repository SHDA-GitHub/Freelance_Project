using System.Collections;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance;

    public StatusEffectPanel mainPlayerPanel;
    public StatusEffectPanel buddyPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowStatusEffect(CharacterStats targetCharacter)
    {
        StartCoroutine(ShowStatusEffectRoutine(targetCharacter));
    }

    private IEnumerator ShowStatusEffectRoutine(CharacterStats targetCharacter)
    {
        StatusEffectPanel panelToUse = null;

        if (targetCharacter.CompareTag("Player"))
            panelToUse = mainPlayerPanel;
        else if (targetCharacter.CompareTag("PlayerBuddy"))
            panelToUse = buddyPanel;
        else
            yield break;

        panelToUse.gameObject.SetActive(true);
        panelToUse.UpdatePanel(targetCharacter);
    }
}