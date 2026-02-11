using UnityEngine;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Battle/Attack")]
public class Attack : ScriptableObject
{
    public string attackName;
    public int powerCost;
    public int damage;
    public AudioClip attackSound;
    public bool consumable = true;
    [TextArea] public string flavorText;
}