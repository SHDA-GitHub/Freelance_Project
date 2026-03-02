using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

        if (action is InventorySpecialAttack invSpec)
            return invSpec.attackData.specAttackName;

        if (action is InventoryItem invItem)
            return invItem.itemData.itemName;

        if (action is ScriptableObject so)
            return so.name;

        return "Unknown";
    }

    private string GetDescription(object action)
    {
        if (action is Attack attack)
            return attack.descriptionText;

        if (action is InventorySpecialAttack invSpec)
            return invSpec.attackData.descriptionText;

        if (action is InventoryItem invItem)
            return invItem.itemData.descriptionText;

        return "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        string description = GetDescription(actionData);

        if (!string.IsNullOrEmpty(description))
        {
            TurnManager.Instance.ShowDescription(description);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TurnManager.Instance.HideDescription();
    }
}