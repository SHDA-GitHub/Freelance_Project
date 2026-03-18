using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public FlavorTextUI flavorTextUI;

    public List<InventoryItem> items = new List<InventoryItem>();
    public List<InventoryItem> keyItems = new List<InventoryItem>();
    public List<InventorySpecialAttack> specAttacks = new List<InventorySpecialAttack>();

    private const int MAX_ITEMS = 16;
    private const int MAX_SPECIAL_ATTACKS = 16;

    private void Awake()
    {
        flavorTextUI = FindFirstObjectByType<FlavorTextUI>();
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public bool AddItem(Item item)
    {
        if (item == null) return false;

        if (item.isKeyItem)
        {
            return AddKeyItem(item);
        }

        var existing = items.Find(i => i.itemData.itemName == item.itemName);

        if (existing != null)
        {
            existing.quantity++;
            return true;
        }

        if (items.Count >= MAX_ITEMS)
            return false;

        items.Add(new InventoryItem(item, 1));
        return true;
    }

    public bool AddSpecialAttack(SpecialAttack attack)
    {
        var existing = specAttacks.Find(a => a.attackData == attack);

        if (existing != null)
        {
            existing.quantity++;
            return true;
        }

        if (specAttacks.Count >= MAX_SPECIAL_ATTACKS)
            return false;

        specAttacks.Add(new InventorySpecialAttack(attack, 1));
        return true;
    }

    public bool AddKeyItem(Item item)
    {
        if (item == null) return false;

        var existing = keyItems.Find(i => i.itemData == item);

        if (existing != null)
        {
            existing.quantity++;
        }
        else
        {
            keyItems.Add(new InventoryItem(item, 1));
        }

        return true;
    }

    public void UseSpecialAttack(InventorySpecialAttack invSpecAttack)
    {
        if (!specAttacks.Contains(invSpecAttack))
            return;

        invSpecAttack.quantity--;

        if (invSpecAttack.quantity <= 0 && invSpecAttack.attackData.oneUse)
            specAttacks.Remove(invSpecAttack);
        else
        {
            return;
        }
    }

    public void UseItem(InventoryItem invItem)
    {
        if (!items.Contains(invItem))
            return;

        invItem.quantity--;

        if (invItem.quantity <= 0 && invItem.itemData.consumable)
            items.Remove(invItem);
        else
        {
            return;
        }
    }
}
