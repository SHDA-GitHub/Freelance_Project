using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Battle/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public int healAmount;
    public AudioClip itemSound;
    public bool consumable = true;
    [TextArea] public string flavorText;
}

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public List<Item> items = new List<Item>();
    public List<SpecialAttack> specAttacks = new List<SpecialAttack>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void UseItem(Item item, CharacterStats target)
    {
        if (!items.Contains(item)) return;

        target.currentHealth = Mathf.Min(target.currentHealth + item.healAmount, target.maxHealth);

        if (item.consumable)
            items.Remove(item);
    }

    public void UseSpecialAttack(SpecialAttack specAttack, CharacterStats target)
    {
        if (!specAttacks.Contains(specAttack)) return;

        if (specAttack.oneUse)
            specAttacks.Remove(specAttack);
    }
}