using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;

    private ShopItem itemData;
    private ShopUIController shopUI;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(ShopItem item, ShopUIController controller)
    {
        itemData = item;
        shopUI = controller;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => shopUI.TryBuyItem(item));

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
}