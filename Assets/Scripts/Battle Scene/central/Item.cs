using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Battle/Item")]
public class Item : ScriptableObject
{
    public string itemName;

    [Header("Healing (HP)")]
    public int healAmount;
    public bool healAllParty = false;
    public bool splitHealAcrossParty = false;

    [Header("Power Recovery (PP)")]
    public int ppAmount;
    public bool restorePPToAllParty = false;
    public bool splitPPAcrossParty = false;

    [Header("Status Removal")]
    public bool removeAllStatusEffects = false;
    public bool removeDOT = false;
    public bool removeStun = false;
    public bool removeMiss = false;

    [Header("DOT Behavior")]
    public bool drainPPInstead = false;

    [Header("DOT Settings")]
    public int dotAmount = 1;
    public bool dotDrainsPP = false;

    [Header("Status Effect")]
    public DOTStatusEffectType statusEffect = DOTStatusEffectType.None;
    public StunStatusEffectType stunstatusEffect = StunStatusEffectType.None;
    public MissStatusEffectType missStatusEffect = MissStatusEffectType.None;
    public OffenseDefenseChangeStatusEffectType statChangeEffect = OffenseDefenseChangeStatusEffectType.None;

    public int offenseChange = 0;
    public int defenseChange = 0;

    [Header("Audio")]
    public AudioClip itemSound;

    [Header("Item Type")]
    public bool isKeyItem = false;
    public bool consumableKey = false;

    [TextArea(3, 6)]
    public string descriptionText;

    [Range(0, 100)]
    public int statusChance = 0;

    public int statusDuration = 2;

    public bool consumable = true;

    [TextArea] public string flavorText;
}