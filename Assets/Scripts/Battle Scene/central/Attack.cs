using UnityEngine;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Battle/Attack")]
public class Attack : ScriptableObject
{
    [Header("Status Effect")]
    public StatusEffectType statusEffect = StatusEffectType.None;

    [Range(0, 100)]
    public int statusChance = 0;

    public int statusDuration = 2;
    public string attackName;
    public int powerCost;
    public int damage;
    public AudioClip attackSound;
    [TextArea] public string flavorText;
}