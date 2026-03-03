using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public FlavorTextUI flavorTextUI;
    public List<InventoryItem> items = new List<InventoryItem>();
    public List<InventorySpecialAttack> specAttacks = new List<InventorySpecialAttack>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    public void AddItem(Item item)
    {
        InventoryItem existingItem =
            items.Find(i => i.itemData == item);

        if (existingItem != null)
        {
            InventoryItem newItem =
                new InventoryItem(item);
            items.Add(newItem);
        }
    }

    public void AddSpecialAttack(SpecialAttack attack)
    {
        InventorySpecialAttack existingAttack =
            specAttacks.Find(a => a.attackData == attack);

        if (existingAttack == null)
        {
            InventorySpecialAttack newAttack =
                new InventorySpecialAttack(attack);
            specAttacks.Add(newAttack);
        }
    }

    public void UseSpecialAttack(InventorySpecialAttack invSpecAttack)
    {
        if (!specAttacks.Contains(invSpecAttack))
            return;

        if (invSpecAttack.attackData.oneUse)
            specAttacks.Remove(invSpecAttack);
    }
}