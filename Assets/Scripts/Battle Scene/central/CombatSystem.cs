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

    public IEnumerator ExecuteAttack(CharacterStats attacker, CharacterStats target, Attack attack)
    {
        if (attacker.currentPP < attack.powerCost)
        {
            yield break;
        }
        attacker.currentPP -= attack.powerCost;
        string message = !string.IsNullOrEmpty(attack.flavorText)
            ? attack.flavorText
            : $"{attacker.characterName} used {attack.attackName}!";
        yield return flavorTextUI.ShowTextCoroutine(message);
        if (attack.attackSound != null)
            AudioSource.PlayClipAtPoint(attack.attackSound, target.transform.position);
        target.ReceiveDamage(attack.damage);
        yield return StartCoroutine(FlashDamageEffect(target));
        yield return flavorTextUI.ShowTextCoroutine($"{target.characterName} took {attack.damage} damage!");
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
        string message = !string.IsNullOrEmpty(specAttack.flavorText)
            ? specAttack.flavorText
            : $"{attacker.characterName} used {specAttack.specAttackName}!";
        yield return flavorTextUI.ShowTextCoroutine(message);
        if (specAttack.attackSound != null)
            AudioSource.PlayClipAtPoint(specAttack.attackSound, target.transform.position);
        target.ReceiveDamage(specAttack.damage);
        yield return StartCoroutine(FlashDamageEffect(target));
        yield return flavorTextUI.ShowTextCoroutine($"{target.characterName} took {specAttack.damage} damage!");
        TurnManager.Instance.CheckWinLose();
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