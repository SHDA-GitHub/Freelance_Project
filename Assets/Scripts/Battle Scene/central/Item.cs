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