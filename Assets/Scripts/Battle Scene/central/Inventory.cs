using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public FlavorTextUI flavorTextUI;
    public List<InventoryItem> items = new List<InventoryItem>();
    public List<SpecialAttack> specAttacks = new List<SpecialAttack>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void UseSpecialAttack(SpecialAttack specAttack, CharacterStats target)
    {
        if (!specAttacks.Contains(specAttack)) return;

        if (specAttack.oneUse)
            specAttacks.Remove(specAttack);
    }
}