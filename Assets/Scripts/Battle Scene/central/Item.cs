using UnityEngine;

public class Item : ScriptableObject
{
    public string itemName;
    public int healAmount;
    public AudioClip itemSound;
    public bool consumable = true;
    [TextArea] public string flavorText;
}