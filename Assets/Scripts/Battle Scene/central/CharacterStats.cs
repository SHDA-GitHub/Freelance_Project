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

    [Header("Enemy Only")]
    public EnemyLoadout enemyLoadout;

    public List<Attack> attacks;
    public List<StatusEffect> activeStatusEffects = new List<StatusEffect>();
    public List<StunStatusEffect> activeStunEffects = new List<StunStatusEffect>();
    public List<MissStatusEffect> activeMissEffects = new List<MissStatusEffect>();

    public int overtimeDamage = 1;

    public void ApplyStatus(DOTStatusEffectType type, int duration)
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
            activeStatusEffects.Add(new StatusEffect(type, duration));
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

    public void ApplyStatusEffects()
    {
        for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = activeStatusEffects[i];

            ApplyOvertimeDamage(overtimeDamage);

            CombatSystem.Instance.StartCoroutine(
                CombatSystem.Instance.flavorTextUI.ShowTextCoroutine(
                    $"{characterName} is {effect.type} and took {overtimeDamage} damage!"
                )
            );

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
    }

    public void ApplyOvertimeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
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

    public void ReduceAllEffectsAfterTurn()
    {
        ReduceStunEffects();
        ReduceMissEffects();
        ReduceDOTDurations();
    }

    public void ReduceDOTDurations()
    {
        for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
        {
            activeStatusEffects[i].duration--;

            if (activeStatusEffects[i].duration <= 0)
            {
                activeStatusEffects.RemoveAt(i);
            }
        }
    }

    public void ReduceStunEffects()
    {
        for (int i = activeStunEffects.Count - 1; i >= 0; i--)
        {
            activeStunEffects[i].duration--;
            if (activeStunEffects[i].duration <= 0)
                activeStunEffects.RemoveAt(i);
        }
    }

    public void ReduceMissEffects()
    {
        for (int i = activeMissEffects.Count - 1; i >= 0; i--)
        {
            activeMissEffects[i].duration--;
            if (activeMissEffects[i].duration <= 0)
                activeMissEffects.RemoveAt(i);
        }
    }

    public void RemoveAllStatusEffects()
    {
        activeStatusEffects.Clear();
        activeStunEffects.Clear();
        activeMissEffects.Clear();
    }

    public void RemoveDOTEffects()
    {
        activeStatusEffects.Clear();
    }

    public void RemoveStunEffects()
    {
        activeStunEffects.Clear();
    }

    public void RemoveMissEffects()
    {
        activeMissEffects.Clear();
    }

    public void SetInvisible()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, 0f);
    }
}
