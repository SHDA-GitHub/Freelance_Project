using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "CardData")]
public class AttackData : ScriptableObject
{
    public string CardName;

    [TextArea] public string PlayerPlayText;
    [TextArea] public string EnemyPlayText;
    [TextArea] public string HoverInfoText;

    [Header("Card Types")]
    public PlayerCardType playerCardType;
    public EnemyCardType enemyCardType;

    [Header("Mana")]
    [Min(0)] public float manaCost = 0;
    [Min(0)] public float healthCost = 0;
    public bool isSpell = false;
    public bool isHPDrainSpell = false;

    [Header("Card Effect Values")]
    public float effectAmountAttack = 10f;
    public float effectAmountAttackOvertime = 10f;
    public float effectAmountDefend = 7.5f;
    public float effectAmountHeal = 15f;
    public float effectAmountMana = 7.5f;
    public int effectAmountTurnSkip = 1;

    [Header("Cooldowns")]
    [Min(0)] public int playerCooldownTurns = 0;
    [Min(0)] public int enemyCooldownTurns = 0;

    [Header("Settings")]
    public bool hasFlavortext = false;
}
