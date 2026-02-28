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

    [Header("Audio")]
    public AudioClip itemSound;

    public bool consumable = true;

    [TextArea] public string flavorText;
}