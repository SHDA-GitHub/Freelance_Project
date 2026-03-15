using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public FlavorTextUI flavorTextUI;

    public List<InventoryItem> items = new List<InventoryItem>();
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


    public void UseSpecialAttack(InventorySpecialAttack invSpecAttack)
    {
        if (!specAttacks.Contains(invSpecAttack))
            return;

        if (invSpecAttack.attackData.oneUse)
            specAttacks.Remove(invSpecAttack);
    }
}
