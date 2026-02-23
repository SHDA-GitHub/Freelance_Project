using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpecAttack", menuName = "Battle/Special Attack")]
public class SpecialAttack : ScriptableObject
{
    [Header("Status Effect")]
    public StatusEffectType statusEffect = StatusEffectType.None;

    [Range(0, 100)]
    public int statusChance = 0;

    public int statusDuration = 2;
    public string specAttackName;
    public int powerCost;
    public int damage;
    public AudioClip attackSound;
    public bool specialAttackCamShake = false;
    public bool oneUse = true;
    [TextArea] public string flavorText;
}