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

    [SerializeField] private int overtimeDamage = 1;

    public void StartTurn()
    {
        ApplyStatusEffects();

        if (currentHealth <= 0)
            return;

        if (TurnManager.Instance.currentTurn == TurnType.Player)
            UIManager.Instance.ShowPlayerOptions(this);
        else
            PerformEnemyAction();
    }

    void PerformEnemyAction()
    {
        if (enemyLoadout == null)
        {
            Debug.LogWarning($"{characterName} has no EnemyLoadout assigned!");
            return;
        }

        Attack attack = enemyLoadout.GetRandomAttack();

        if (attack == null)
            return;

        CombatSystem.Instance.StartCoroutine(
            CombatSystem.Instance.ExecuteAttack(
                this,
                TurnManager.Instance.playerParty[0],
                attack
            )
        );
    }

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

    private void ApplyStatusEffects()
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

            effect.duration--;

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

}
