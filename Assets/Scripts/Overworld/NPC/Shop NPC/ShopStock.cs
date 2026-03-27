using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Shop Stock")]
public class ShopStock : ScriptableObject
{
    public List<ShopItem> items = new List<ShopItem>();
}

public enum ShopItemType
{
    Item,
    SpecialAttack
}

[System.Serializable]
public class ShopItem
{
    public ShopItemType type;

    public Item item;
    public SpecialAttack specialAttack;

    public float price;
}