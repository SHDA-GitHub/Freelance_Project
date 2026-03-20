using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterStats : MonoBehaviour
{
    public string characterName;
    public int maxHealth;
    public int currentHealth;
    public int maxPP;
    public int currentPP;
    public int level;
    public int currentEXP;

    [Header("Status Immunities")]
    public List<DOTStatusEffectType> immuneDOTEffects = new List<DOTStatusEffectType>();
    public List<StunStatusEffectType> immuneStunEffects = new List<StunStatusEffectType>();
    public List<MissStatusEffectType> immuneMissEffects = new List<MissStatusEffectType>();
    public List<OffenseDefenseChangeStatusEffectType> immuneOffDefEffects = new List<OffenseDefenseChangeStatusEffectType>();

    [Header("Enemy Only")]
    public EnemyLoadout enemyLoadout;
    public Item dropReward;
    public int expReward = 5;
    public int currencyReward = 5;

    public List<Attack> attacks;
    public PlayerStatsSO playerStats;
    public List<StatusEffect> activeStatusEffects = new List<StatusEffect>();
    public List<StunStatusEffect> activeStunEffects = new List<StunStatusEffect>();
    public List<MissStatusEffect> activeMissEffects = new List<MissStatusEffect>();
    public List<OffenseDefenseChangeStatusEffect> activeOffDefEffects = new List<OffenseDefenseChangeStatusEffect>();

    public int overtimeDamage = 1;
    public bool isPlayer = false;

    void Start()
    {
        if (isPlayer && playerStats != null)
        {
            characterName = playerStats.characterName;
            currentHealth = playerStats.currentHealth;
            maxHealth = playerStats.maxHealth;
            currentPP = playerStats.currentPP;
            maxPP = playerStats.maxPP;
            level = playerStats.level;
            currentEXP = playerStats.currentEXP;
        }
    }

    public void ApplyStatus(DOTStatusEffectType type, int duration, int amountPerTurn = 1, bool drainPP = false)
    {
        if (type == DOTStatusEffectType.None)
            return;

        StatusEffect existing = activeStatusEffects.Find(s => s.type == type);

        if (existing != null)
        {
            existing.duration = Mathf.Max(existing.duration, duration);
        }
        else
        {
            activeStatusEffects.Add(new StatusEffect(type, duration, amountPerTurn, drainPP));
        }
    }

    public void ApplyStun(StunStatusEffectType type, int duration)
    {
        if (type == StunStatusEffectType.None)
            return;

        activeStunEffects.Add(new StunStatusEffect(type, duration));
    }

    public void ApplyMiss(MissStatusEffectType type, int duration)
    {
        if (type == MissStatusEffectType.None)
            return;

        activeMissEffects.Add(new MissStatusEffect(type, duration));
    }

    public void ApplyStatChange(OffenseDefenseChangeStatusEffectType type, int duration, int offense, int defense)
    {
        if (type == OffenseDefenseChangeStatusEffectType.None)
            return;

        activeOffDefEffects.Add(new OffenseDefenseChangeStatusEffect(type, duration, offense, defense));
    }

    public void ApplyStatusEffects()
    {
        for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = activeStatusEffects[i];

            int amount = effect.amountPerTurn;

            if (effect.drainPP)
            {
                ApplyOvertimePPReduction(amount);

                CombatSystem.Instance.StartCoroutine(
                    CombatSystem.Instance.flavorTextUI.ShowTextCoroutine(
                        $"{characterName} is {effect.type} and lost {amount} PP!"
                    )
                );
            }
            else
            {
                ApplyOvertimeDamage(amount);

                CombatSystem.Instance.StartCoroutine(
                    CombatSystem.Instance.flavorTextUI.ShowTextCoroutine(
                        $"{characterName} is {effect.type} and took {amount} damage!"
                    )
                );
            }

            if (effect.duration <= 0)
            {
                activeStatusEffects.RemoveAt(i);
                CombatSystem.Instance.StartCoroutine(
                    CombatSystem.Instance.flavorTextUI.ShowTextCoroutine(
                        $"{characterName} is no longer {effect.type}."
                    )
                );
            }
        }
    }

    public void ReceiveDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (isPlayer)
            playerStats.currentHealth = currentHealth;
    }

    public void ApplyOvertimeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (isPlayer)
            playerStats.currentHealth = currentHealth;
    }

    public void ApplyOvertimePPReduction(int damage)
    {
        currentPP -= damage;
        currentPP = Mathf.Max(currentPP, 0);

        if (isPlayer)
            playerStats.currentPP = currentPP;
    }

    public bool IsStunned()
    {
        return activeStunEffects.Count > 0;
    }

    public bool IsDOT()
    {
        return activeStatusEffects.Count > 0;
    }

    public bool IsMissAttack()
    {
        return activeMissEffects.Count > 0;
    }

    public bool IsStatChange()
    {
        return activeOffDefEffects.Count > 0;
    }

    public int GetOffenseModifier()
    {
        int total = 0;

        foreach (var effect in activeOffDefEffects)
            total += effect.offenseModifier;

        return total;
    }

    public int GetDefenseModifier()
    {
        int total = 0;

        foreach (var effect in activeOffDefEffects)
            total += effect.defenseModifier;

        return total;
    }

    public void ReduceAllEffectsAfterTurn()
    {
        ReduceStunEffects();
        ReduceMissEffects();
        ReduceDOTDurations();
        ReduceOffDefEffects();
    }

    public void ReduceDOTDurations()
    {
        bool removedAny = false;
        for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
        {
            activeStatusEffects[i].duration--;

            if (activeStatusEffects[i].duration <= 0)
            {
                activeStatusEffects.RemoveAt(i);
                removedAny = true;
            }
        }

        if (removedAny)
            StatusEffectManager.Instance.ShowStatusEffect(this);
    }

    public void ReduceStunEffects()
    {
        bool removedAny = false;
        for (int i = activeStunEffects.Count - 1; i >= 0; i--)
        {
            activeStunEffects[i].duration--;
            if (activeStunEffects[i].duration <= 0)
            {
                activeStunEffects.RemoveAt(i);
                removedAny = true;
            }
        }
        if (removedAny)
            StatusEffectManager.Instance.ShowStatusEffect(this);
    }

    public void ReduceMissEffects()
    {
        bool removedAny = false;
        for (int i = activeMissEffects.Count - 1; i >= 0; i--)
        {
            activeMissEffects[i].duration--;
            if (activeMissEffects[i].duration <= 0)
            {
                activeMissEffects.RemoveAt(i);
                removedAny = true;
            }
        }
        if (removedAny)
            StatusEffectManager.Instance.ShowStatusEffect(this);
    }

    public void ReduceOffDefEffects()
    {
        bool removedAny = false;
        for (int i = activeOffDefEffects.Count - 1; i >= 0; i--)
        {
            activeOffDefEffects[i].duration--;

            if (activeOffDefEffects[i].duration <= 0)
            {
                activeOffDefEffects.RemoveAt(i);
                removedAny = true;
            }
        }
        if (removedAny)
            StatusEffectManager.Instance.ShowStatusEffect(this);
    }

    public void RemoveAllStatusEffects()
    {
        activeStatusEffects.Clear();
        activeStunEffects.Clear();
        activeMissEffects.Clear();
        activeOffDefEffects.Clear();
        RefreshStatusEffectUI();
    }

    private void RefreshStatusEffectUI()
    {
        if (StatusEffectManager.Instance != null)
            StatusEffectManager.Instance.ShowStatusEffect(this);
    }

    public void RemoveDOTEffect(DOTStatusEffectType type)
    {
        activeStatusEffects.RemoveAll(e => e.type == type);
        StatusEffectManager.Instance.ShowStatusEffect(this);
    }

    public void RemoveStunEffect(StunStatusEffectType type)
    {
        activeStunEffects.RemoveAll(e => e.type == type);
        StatusEffectManager.Instance.ShowStatusEffect(this);
    }

    public void RemoveMissEffect(MissStatusEffectType type)
    {
        activeMissEffects.RemoveAll(e => e.type == type);
        StatusEffectManager.Instance.ShowStatusEffect(this);
    }

    public void RemoveStatEffect(OffenseDefenseChangeStatusEffectType type)
    {
        activeOffDefEffects.RemoveAll(e => e.type == type);
        StatusEffectManager.Instance.ShowStatusEffect(this);
    }

    public bool IsImmune(DOTStatusEffectType type)
    {
        return immuneDOTEffects.Contains(type);
    }

    public bool IsImmune(StunStatusEffectType type)
    {
        return immuneStunEffects.Contains(type);
    }

    public bool IsImmune(MissStatusEffectType type)
    {
        return immuneMissEffects.Contains(type);
    }

    public bool IsImmune(OffenseDefenseChangeStatusEffectType type)
    {
        return immuneOffDefEffects.Contains(type);
    }

    public void SetInvisible()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, 0f);
    }

    public void GainEXP(int amount)
    {
        if (!isPlayer) return;

        playerStats.GainEXP(amount);
    }
}
