using System.Collections.Generic;

[System.Serializable]
public class CooldownTracker
{
    private Dictionary<AttackData, int> cooldowns = new Dictionary<AttackData, int>();

    public void Register(AttackData card)
    {
        if (!cooldowns.ContainsKey(card))
            cooldowns.Add(card, 0);
    }

    public bool IsReady(AttackData card)
    {
        if (!cooldowns.TryGetValue(card, out int value))
            return true;
        return value <= 0;
    }


    public void SetCooldown(AttackData card, int turns)
    {
        if (turns <= 0)
            return;

        cooldowns[card] = turns;
    }


    public void Tick()
    {
        var keys = new List<AttackData>(cooldowns.Keys);

        foreach (var key in keys)
        {
            if (cooldowns[key] > 0)
                cooldowns[key]--;
        }
    }

    public int GetCooldown(AttackData card)
    {
        if (!cooldowns.TryGetValue(card, out int value))
            return 0;

        return value;
    }

}
