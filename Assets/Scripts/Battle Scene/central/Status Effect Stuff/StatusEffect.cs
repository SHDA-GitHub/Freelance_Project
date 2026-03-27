[System.Serializable]
public class StatusEffect
{
    public DOTStatusEffectType type;
    public int duration;

    public int amountPerTurn = 1;
    public bool drainPP = false;

    public StatusEffect(DOTStatusEffectType type, int duration, int amountPerTurn = 1, bool drainPP = false)
    {
        this.type = type;
        this.duration = duration;
        this.amountPerTurn = amountPerTurn;
        this.drainPP = drainPP;
    }
}

[System.Serializable]
public class StunStatusEffect
{
    public StunStatusEffectType type;
    public int duration;

    public StunStatusEffect(StunStatusEffectType type, int duration)
    {
        this.type = type;
        this.duration = duration;
    }
}

[System.Serializable]
public class MissStatusEffect
{
    public MissStatusEffectType type;
    public int duration;

    public MissStatusEffect(MissStatusEffectType type, int duration)
    {
        this.type = type;
        this.duration = duration;
    }
}

[System.Serializable]
public class OffenseDefenseChangeStatusEffect
{
    public OffenseDefenseChangeStatusEffectType type;
    public int duration;

    public int offenseModifier;
    public int defenseModifier;

    public OffenseDefenseChangeStatusEffect(
        OffenseDefenseChangeStatusEffectType type,
        int duration,
        int offenseModifier,
        int defenseModifier)
    {
        this.type = type;
        this.duration = duration;
        this.offenseModifier = offenseModifier;
        this.defenseModifier = defenseModifier;
    }
}