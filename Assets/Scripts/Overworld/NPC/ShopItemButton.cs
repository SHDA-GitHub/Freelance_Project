using UnityEngine;
using TMPro;

public class ShopItemButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;

    private ShopItem itemData;
    private ShopUIController shopUI;

    public void Setup(ShopItem item, ShopUIController controller)
    {
        itemData = item;
        shopUI = controller;

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
        if (shopUI != null)
        {
            shopUI.TryBuyItem(itemData);
        }
    }
}