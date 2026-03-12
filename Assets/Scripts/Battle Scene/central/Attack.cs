using UnityEngine;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Battle/Attack")]
public class Attack : ScriptableObject
{
    [Header("Targeting")]
    public bool targetAllEnemies = false;

    [Header("DOT Behavior")]
    public bool drainPPInstead = false;

    [Header("DOT Settings")]
    public int dotAmount = 1;
    public bool dotDrainsPP = false;

    [Header("Status Effect")]
    public DOTStatusEffectType statusEffect = DOTStatusEffectType.None;
    public StunStatusEffectType stunstatusEffect = StunStatusEffectType.None;
    public MissStatusEffectType missStatusEffect = MissStatusEffectType.None;

    [Header("Life Steal / Heal On Hit")]
    public bool healOnHit = false;
    public int healAmount = 0;

    [TextArea(3, 6)]
    public string descriptionText;

    [Range(0, 100)]
    public int statusChance = 0;

    public int statusDuration = 2;
    public string attackName;
    public int powerCost;
    public int damage;
    public AudioClip attackSound;
    [TextArea] public string flavorText;
}