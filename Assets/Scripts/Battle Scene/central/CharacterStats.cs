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

    public void StartTurn()
    {
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
