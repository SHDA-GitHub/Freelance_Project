using UnityEngine;
using TMPro;

public class ShopItemButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;

    private ShopItem itemData;

    public void Setup(ShopItem item)
    {
        itemData = item;

        switch (item.type)
        {
            case ShopItemType.Item:
                nameText.text = item.item.itemName;
                break;

            case ShopItemType.SpecialAttack:
                nameText.text = item.specialAttack.specAttackName;
                break;
        }

        priceText.text = item.price.ToString();
    }

    public void OnClick()
    {
        switch (itemData.type)
        {
            case ShopItemType.Item:
                Inventory.Instance.AddItem(itemData.item);
                Debug.Log("Bought item: " + itemData.item.itemName);
                break;

            case ShopItemType.SpecialAttack:
                Inventory.Instance.AddSpecialAttack(itemData.specialAttack);
                Debug.Log("Bought special: " + itemData.specialAttack.specAttackName);
                break;
        }
    }
}