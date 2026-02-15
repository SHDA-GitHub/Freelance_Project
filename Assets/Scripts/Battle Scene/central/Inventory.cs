using System.Collections;
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
    public FlavorTextUI flavorTextUI;
    public List<Item> items = new List<Item>();
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