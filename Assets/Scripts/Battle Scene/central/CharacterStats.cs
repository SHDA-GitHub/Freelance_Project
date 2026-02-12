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

    public List<Attack> attacks;

    public void StartTurn()
    {
        if (TurnManager.Instance.currentTurn == TurnType.Player)
            UIManager.Instance.ShowPlayerOptions(this);
        else
            PerformEnemyAction();
    }

    void PerformEnemyAction()
    {
        if (attacks.Count == 0) return;

        var attack = attacks[Random.Range(0, attacks.Count)];
        CombatSystem.Instance.StartCoroutine(CombatSystem.Instance.ExecuteAttack(this, TurnManager.Instance.playerParty[0], attack));
        TurnManager.Instance.EndTurn();
    }

    public void ReceiveDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        StartCoroutine(CombatSystem.Instance.FlashDamageEffect(this));
    }

    public void ApplyOvertimeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        StartCoroutine(CombatSystem.Instance.FlashDamageEffect(this));
    }
}
