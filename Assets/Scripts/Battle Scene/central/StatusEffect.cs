[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public int duration;

    public StatusEffect(StatusEffectType type, int duration)
    {
        this.type = type;
        this.duration = duration;
    }
}