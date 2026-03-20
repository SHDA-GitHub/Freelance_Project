using System.Collections.Generic;
using UnityEngine;

public class StatusEffectPanel : MonoBehaviour
{
    public StatusEffectUI panelUI;

    public void UpdatePanel(CharacterStats character)
    {
        var effects = new List<(string name, Sprite icon)>();
        foreach (var s in character.activeStatusEffects)
            effects.Add((s.type.ToString(), GetSpriteForEffect(s.type)));
        foreach (var s in character.activeStunEffects)
            effects.Add((s.type.ToString(), GetSpriteForEffect(s.type)));
        foreach (var s in character.activeMissEffects)
            effects.Add((s.type.ToString(), GetSpriteForEffect(s.type)));
        foreach (var s in character.activeOffDefEffects)
            effects.Add((s.type.ToString(), GetSpriteForEffect(s.type)));

        if (effects.Count > 0)
        {
            panelUI.SetEffect(effects[0].icon, effects[0].name, character.characterName);
            panelUI.gameObject.SetActive(true);
            gameObject.SetActive(true);
        }
        else
        {
            panelUI.ClearEffect();
            gameObject.SetActive(false);
        }
    }

    private Sprite GetSpriteForEffect(object type)
    {
        return StatusEffectDatabase.Instance.GetSpriteForType(type);
    }
}