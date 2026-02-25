using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ActionButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI buttonText;

    private object actionData;
    private Action<object> onClickCallback;

    public void Setup(object action, Action<object> callback)
    {
        actionData = action;
        onClickCallback = callback;

        if (buttonText != null)
        {
            buttonText.text = GetDisplayName(action);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback?.Invoke(actionData));
    }

    private string GetDisplayName(object action)
    {
        if (action is Attack attack)
            return attack.attackName;

        if (action is SpecialAttack specAttack)
            return specAttack.specAttackName;

        if (action is InventoryItem invItem)
            return invItem.itemData.itemName;

        if (action is ScriptableObject so)
            return so.name;

        return "Unknown";
    }
}