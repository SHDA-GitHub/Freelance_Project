using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    public static CombatSystem Instance;
    public FlavorTextUI flavorTextUI;
    public CharacterStats playerAttacks;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private string FormatFlavorText(string template, CharacterStats attacker, CharacterStats target, string actionName, int damage = 0)
    {
        if (string.IsNullOrEmpty(template))
            return "";

        return template
            .Replace("{attacker}", attacker.characterName)
            .Replace("{target}", target.characterName)
            .Replace("{action}", actionName)
            .Replace("{damage}", damage.ToString());
    }

    public IEnumerator ExecuteAttack(CharacterStats attacker, CharacterStats target, Attack attack)
    {
        if (attacker.currentPP < attack.powerCost)
        {
            yield break;
        }
        attacker.currentPP -= attack.powerCost;
        TurnManager.Instance.battleHUD.UpdateHUD();
        string message = !string.IsNullOrEmpty(attack.flavorText)
            ? FormatFlavorText(attack.flavorText, attacker, target, attack.attackName, attack.damage)
            : $"{attacker.characterName} used {attack.attackName}!";
        yield return flavorTextUI.ShowTextCoroutine(message);
        if (attack.attackSound != null)
        AudioManager.Instance.PlaySFX(attack.attackSound);
        target.ReceiveDamage(attack.damage);
        TurnManager.Instance.battleHUD.UpdateHUD();
        yield return StartCoroutine(FlashDamageEffect(target));
        if (attack.damage > 0)
        { yield return flavorTextUI.ShowTextCoroutine($"{target.characterName} took {attack.damage} damage!"); }
        if (target.currentHealth <= 0)
        { yield return TurnManager.Instance.HandleEnemyDeath(target); }
        TurnManager.Instance.battleHUD.UpdateHUD();
        TurnManager.Instance.CheckWinLose();
        yield return new WaitForSeconds(0.3f);
        TurnManager.Instance.EndTurn();
    }

    public IEnumerator ExecuteSpecialAttack(CharacterStats attacker, CharacterStats target, SpecialAttack specAttack)
    {
        if (attacker.currentPP < specAttack.powerCost)
        {
            yield break;
        }
        attacker.currentPP -= specAttack.powerCost;
        TurnManager.Instance.battleHUD.UpdateHUD();
        string message = !string.IsNullOrEmpty(specAttack.flavorText)
            ? FormatFlavorText(specAttack.flavorText, attacker, target, specAttack.specAttackName, specAttack.damage)
            : $"{attacker.characterName} used {specAttack.specAttackName}!";
        yield return flavorTextUI.ShowTextCoroutine(message);
        if (specAttack.attackSound != null)
        AudioManager.Instance.PlaySFX(specAttack.attackSound);
        target.ReceiveDamage(specAttack.damage);
        TurnManager.Instance.battleHUD.UpdateHUD();
        yield return StartCoroutine(FlashDamageEffect(target));
        if (specAttack.damage > 0)
        { yield return flavorTextUI.ShowTextCoroutine($"{target.characterName} took {specAttack.damage} damage!"); }
        if (target.currentHealth <= 0)
        { yield return TurnManager.Instance.HandleEnemyDeath(target); }
        TurnManager.Instance.battleHUD.UpdateHUD();
        TurnManager.Instance.CheckWinLose();
        yield return new WaitForSeconds(0.3f);
        TurnManager.Instance.EndTurn();
    }

    public IEnumerator ExecuteItem(CharacterStats user, CharacterStats target, Item item)
    {
        if (!Inventory.Instance.items.Contains(item))
            yield break;
        string message = !string.IsNullOrEmpty(item.flavorText)
            ? FormatFlavorText(item.flavorText, user, target, item.itemName, item.healAmount)
            : $"{user.characterName} used {item.itemName}!";
        yield return flavorTextUI.ShowTextCoroutine(message);
        if (item.itemSound != null)
            AudioManager.Instance.PlaySFX(item.itemSound);
        target.currentHealth = Mathf.Min(target.currentHealth + item.healAmount, target.maxHealth);
        yield return new WaitForSeconds(0.75f);
        TurnManager.Instance.battleHUD.UpdateHUD();
        if (item.healAmount > 0)
            yield return flavorTextUI.ShowTextCoroutine($"{target.characterName} recovered {item.healAmount} HP!");
        if (item.consumable)
            Inventory.Instance.items.Remove(item);

        yield return new WaitForSeconds(0.3f);

        TurnManager.Instance.EndTurn();
    }

    public IEnumerator FlashDamageEffect(CharacterStats target)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        for (int i = 0; i < 3; i++)
        {
            sr.color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(0.1f);
            sr.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.1f);
            sr.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.1f);
        }
    }
}