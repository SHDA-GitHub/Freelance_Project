[System.Serializable]
public class StatusEffect
{
    public DOTStatusEffectType type;
    public int duration;

    public StatusEffect(DOTStatusEffectType type, int duration)
    {
        this.type = type;
        this.duration = duration;
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