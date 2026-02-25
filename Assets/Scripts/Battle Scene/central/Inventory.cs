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

    public void UseSpecialAttack(InventorySpecialAttack invSpecAttack)
    {
        if (!specAttacks.Contains(invSpecAttack))
            return;

        if (invSpecAttack.attackData.oneUse)
            specAttacks.Remove(invSpecAttack);
    }
}