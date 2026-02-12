using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpecAttack", menuName = "Battle/Special Attack")]
public class SpecialAttack : ScriptableObject
{
    public string specAttackName;
    public int powerCost;
    public int damage;
    public AudioClip attackSound;
    public bool oneUse = true;
    [TextArea] public string flavorText;
}