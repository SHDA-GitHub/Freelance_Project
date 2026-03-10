using TMPro;
using UnityEngine;

public class ItemButtonUI : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    private InventoryItem item;

    public void SetItem(InventoryItem newItem)
    {
        item = newItem;
        itemNameText.text = item.itemData.itemName;
    }

    public void OnClick()
    {
        Inventory.Instance.flavorTextUI.ShowImmediateText(item.itemData.flavorText);
    }
}