using UnityEngine;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Battle/Attack")]
public class Attack : ScriptableObject
{
    public string attackName;
    public int powerCost;
    public int damage;
    public AudioClip attackSound;
    [TextArea] public string flavorText;
}