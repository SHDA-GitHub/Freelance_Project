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