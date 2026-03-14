using UnityEngine;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Battle/Attack")]
public class Attack : ScriptableObject
{
    [Header("Targeting")]
    public bool targetAllEnemies = false;
    public bool targetSelf = false;

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

    [Header("Life Steal / Heal On Hit")]
    public bool healOnHit = false;
    public int healAmount = 0;

    [TextArea(3, 6)]
    public string descriptionText;

    [Header("Status Chances")]
    [Range(0, 100)] public int dotStatusChance = 0;
    [Range(0, 100)] public int stunStatusChance = 0;
    [Range(0, 100)] public int missStatusChance = 0;
    [Range(0, 100)] public int statStatusChance = 0;

    public int statusDuration = 2;
    public string attackName;
    public int powerCost;
    public int damage;
    public AudioClip attackSound;
    [TextArea] public string flavorText;
}