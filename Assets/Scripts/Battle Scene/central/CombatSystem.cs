using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CombatSystem : MonoBehaviour
{
    public static CombatSystem Instance;
    public FlavorTextUI flavorTextUI;
    [SerializeField] private AudioClip statusEffectGain;
    [SerializeField] private AudioSource audioManager;
    public CharacterStats playerAttacks;

    [Header("Camera Shake")]
    [SerializeField] private Transform battleCamera;
    [SerializeField] private float shakeAmount = 1f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeSpeed = 0.02f;

    private Vector3 originalCameraPosition;

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
        TurnManager.Instance.battleHUD.UpdateHUD();
        string message = !string.IsNullOrEmpty(attack.flavorText)
            ? FormatFlavorText(attack.flavorText, attacker, target, attack.attackName, attack.damage)
            : $"{attacker.characterName} used {attack.attackName}!";
        yield return flavorTextUI.ShowTextCoroutine(message);
        if (attacker.activeMissEffects.Count > 0)
        {
            int roll = Random.Range(0, 100);

            if (roll < 50)
            {
                yield return flavorTextUI.ShowTextCoroutine(
                    $"{attacker.characterName} missed their attack!"
                );
                yield return new WaitForSeconds(0.3f);
                yield break;
            }
        }
        if (attack.attackSound != null)
        AudioManager.Instance.PlaySFX(attack.attackSound);
        target.ReceiveDamage(attack.damage);
        if (TurnManager.Instance.playerParty.Contains(target) && attack.damage > 0)
        {
            StartCoroutine(ShakeCamera());
        }
        yield return StartCoroutine(FlashDamageEffect(target));
        yield return new WaitForSeconds(0.3f);
        if (attack.damage > 0)
        { yield return flavorTextUI.ShowTextCoroutine($"{target.characterName} took {attack.damage} damage!"); }
        yield return new WaitForSeconds(0.3f);
        if (attack.healOnHit && TurnManager.Instance.playerParty.Contains(target))
        {
            int healValue = Mathf.Max(0, attack.healAmount);

            attacker.currentHealth = Mathf.Min(
                attacker.currentHealth + healValue,
                attacker.maxHealth
            );

            yield return flavorTextUI.ShowTextCoroutine(
                $"{attacker.characterName} recovered {healValue} HP!"
            );
            TurnManager.Instance.battleHUD.UpdateHUD();
            yield return new WaitForSeconds(0.3f);
        }
        TurnManager.Instance.battleHUD.UpdateHUD();
        if (attack.statusEffect != DOTStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < attack.statusChance)
            {
                target.ApplyStatus(attack.statusEffect, attack.statusDuration);

                yield return flavorTextUI.ShowTextCoroutine(
                    $"{target.characterName} is now {attack.statusEffect}!"
                );
                audioManager.clip = statusEffectGain;
                audioManager.Play();
                yield return new WaitForSeconds(0.3f);
            }
        }
        if (attack.stunstatusEffect != StunStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < attack.statusChance)
            {
                target.ApplyStun(attack.stunstatusEffect, attack.statusDuration);

                yield return flavorTextUI.ShowTextCoroutine(
                    $"{target.characterName} is {attack.stunstatusEffect}!"
                );
                audioManager.clip = statusEffectGain;
                audioManager.Play();
                yield return new WaitForSeconds(0.3f);
            }
        }
        if (attack.missStatusEffect != MissStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < attack.statusChance)
            {
                target.ApplyMiss(attack.missStatusEffect, attack.statusDuration);

                yield return flavorTextUI.ShowTextCoroutine(
                    $"{target.characterName} is {attack.missStatusEffect}!"
                );
                audioManager.clip = statusEffectGain;
                audioManager.Play();
                yield return new WaitForSeconds(0.3f);
            }
        }

        Debug.Log("Attacking: " + target.characterName);
        TurnManager.Instance.battleHUD.UpdateHUD();
    }

    public IEnumerator ExecuteSpecialAttack(CharacterStats attacker, CharacterStats target, InventorySpecialAttack invSpecAttack)
    {
        SpecialAttack specAttack = invSpecAttack.attackData;
        TurnManager.Instance.battleHUD.UpdateHUD();
        string message = !string.IsNullOrEmpty(specAttack.flavorText)
            ? FormatFlavorText(specAttack.flavorText, attacker, target, specAttack.specAttackName, specAttack.damage)
            : $"{attacker.characterName} used {specAttack.specAttackName}!";
        yield return flavorTextUI.ShowTextCoroutine(message);
        if (attacker.activeMissEffects.Count > 0)
        {
            int roll = Random.Range(0, 100);

            if (roll < 50)
            {
                yield return flavorTextUI.ShowTextCoroutine(
                    $"{attacker.characterName} missed their attack!"
                );
                yield return new WaitForSeconds(0.3f);
                yield break;
            }
        }
        if (specAttack.attackSound != null)
        AudioManager.Instance.PlaySFX(specAttack.attackSound);
        target.ReceiveDamage(specAttack.damage);
        if (specAttack.specialAttackCamShake)
        {
            StartCoroutine(ShakeCamera());
        }
        if (TurnManager.Instance.playerParty.Contains(target) && specAttack.damage > 0)
        {
            StartCoroutine(ShakeCamera());
        }
        TurnManager.Instance.battleHUD.UpdateHUD();
        yield return StartCoroutine(FlashDamageEffect(target));
        yield return new WaitForSeconds(0.3f);
        if (specAttack.damage > 0)
        { yield return flavorTextUI.ShowTextCoroutine($"{target.characterName} took {specAttack.damage} damage!"); }
        yield return new WaitForSeconds(0.3f);
        if (specAttack.statusEffect != DOTStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < specAttack.statusChance)
            {
                target.ApplyStatus(specAttack.statusEffect, specAttack.statusDuration);

                yield return flavorTextUI.ShowTextCoroutine(
                    $"{target.characterName} is now {specAttack.statusEffect}!"
                );
                audioManager.clip = statusEffectGain;
                audioManager.Play();
                yield return new WaitForSeconds(0.3f);
            }
        }
        if (specAttack.stunstatusEffect != StunStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < specAttack.statusChance)
            {
                target.ApplyStun(specAttack.stunstatusEffect, specAttack.statusDuration);

                yield return flavorTextUI.ShowTextCoroutine(
                    $"{target.characterName} is {specAttack.stunstatusEffect}!"
                );
                audioManager.clip = statusEffectGain;
                audioManager.Play();
                yield return new WaitForSeconds(0.3f);
            }
        }
        if (specAttack.missStatusEffect != MissStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < specAttack.statusChance)
            {
                target.ApplyMiss(specAttack.missStatusEffect, specAttack.statusDuration);

                yield return flavorTextUI.ShowTextCoroutine(
                    $"{target.characterName} is {specAttack.missStatusEffect}!"
                );
                audioManager.clip = statusEffectGain;
                audioManager.Play();
                yield return new WaitForSeconds(0.3f);
            }
        }
        Debug.Log("Attacking: " + target.characterName);
        TurnManager.Instance.battleHUD.UpdateHUD();
    }

    public IEnumerator ExecuteItem(CharacterStats user, CharacterStats target, InventoryItem invItem)
    {
        if (!Inventory.Instance.items.Contains(invItem))
            yield break;

        Item item = invItem.itemData;
        string message;
        if (user != target && TurnManager.Instance.playerParty.Contains(target))
        {
            message = $"{user.characterName} used {item.itemName} on {target.characterName}!";
        }
        else
        {
            message = !string.IsNullOrEmpty(item.flavorText)
                ? FormatFlavorText(item.flavorText, user, target, item.itemName, item.healAmount)
                : $"{user.characterName} used {item.itemName}!";
        }
        yield return flavorTextUI.ShowTextCoroutine(message);
        if (item.itemSound != null)
            AudioManager.Instance.PlaySFX(item.itemSound);
            yield return new WaitForSeconds(0.3f);
        if (item.healAllParty)
        {
            List<CharacterStats> party = TurnManager.Instance.playerParty
                .FindAll(p => p != null && p.currentHealth > 0);

            int healAmount = item.healAmount;

            if (item.splitHealAcrossParty && party.Count > 0)
            {
                healAmount = item.healAmount / party.Count;
            }

            foreach (var member in party)
            {
                member.currentHealth = Mathf.Min(member.currentHealth + healAmount, member.maxHealth);
                yield return flavorTextUI.ShowTextCoroutine(
                    $"{member.characterName} recovered {healAmount} HP!"
                );
                yield return new WaitForSeconds(0.3f);
            }
        }
        else
        {
            target.currentHealth = Mathf.Min(target.currentHealth + item.healAmount, target.maxHealth);

            if (item.healAmount > 0)
            {
                yield return flavorTextUI.ShowTextCoroutine(
                    $"{target.characterName} recovered {item.healAmount} HP!"
                );
                yield return new WaitForSeconds(0.3f);
            }
        }
        if (item.ppAmount > 0)
        {
            if (item.restorePPToAllParty)
            {
                List<CharacterStats> party = TurnManager.Instance.playerParty
                    .FindAll(p => p != null && p.currentHealth > 0);

                int ppRestore = item.ppAmount;

                if (item.splitPPAcrossParty && party.Count > 0)
                {
                    ppRestore = item.ppAmount / party.Count;
                }

                foreach (var member in party)
                {
                    member.currentPP = Mathf.Min(member.currentPP + ppRestore, member.maxPP);

                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{member.characterName} recovered {ppRestore} PP!"
                    );
                    yield return new WaitForSeconds(0.3f);
                }
            }
            else
            {
                target.currentPP = Mathf.Min(target.currentPP + item.ppAmount, target.maxPP);

                yield return flavorTextUI.ShowTextCoroutine(
                    $"{target.characterName} recovered {item.ppAmount} PP!"
                );
                yield return new WaitForSeconds(0.3f);
            }
        }
        void Cleanse(CharacterStats character)
        {
            if (item.removeAllStatusEffects)
            {
                character.RemoveAllStatusEffects();
                flavorTextUI.ShowImmediateText($"{character.characterName} was cleansed!");
                return;
            }

            if (item.removeDOT)
            {
                character.RemoveDOTEffects();
                flavorTextUI.ShowImmediateText($"{character.characterName} was cured!");
            }

            if (item.removeStun)
            {
                character.RemoveStunEffects();
                flavorTextUI.ShowImmediateText($"{character.characterName} is no longer stunned!");
            }

            if (item.removeMiss)
            {
                character.RemoveMissEffects();
                flavorTextUI.ShowImmediateText($"{character.characterName}'s accuracy was restored!");
            }
        }

        if (item.healAllParty)
        {
            foreach (var member in TurnManager.Instance.playerParty)
            {
                if (member != null)
                    Cleanse(member);
            }
        }
        else
        {
            Cleanse(target);
        }
        yield return new WaitForSeconds(0.75f);
        TurnManager.Instance.battleHUD.UpdateHUD();
        if (item.consumable)
            Inventory.Instance.items.Remove(invItem);

        yield return new WaitForSeconds(0.3f);

        TurnManager.Instance.EndTurn();
    }

    public IEnumerator ExecuteAttackOnAll(CharacterStats attacker, List<CharacterStats> targets, Attack attack)
    {
        List<CharacterStats> hitTargets = new List<CharacterStats>();

        foreach (var target in targets)
        {
            if (target != null && target.currentHealth > 0)
            {
                yield return StartCoroutine(ExecuteAttack(attacker, target, attack));
                hitTargets.Add(target);
            }
        }

        foreach (var target in hitTargets)
        {
            if (target.currentHealth <= 0)
            {
                yield return TurnManager.Instance.HandleEnemyDeath(target);
                yield return TurnManager.Instance.HandlePlayerDeath(target);
            }
        }

        TurnManager.Instance.CheckWinLose();
    }

    public IEnumerator ExecuteSpecialAttackOnAll(CharacterStats attacker, List<CharacterStats> targets, InventorySpecialAttack invSpecAttack)
    {
        List<CharacterStats> hitTargets = new List<CharacterStats>();

        foreach (var target in targets)
        {
            if (target != null && target.currentHealth > 0)
            {
                yield return StartCoroutine(ExecuteSpecialAttack(attacker, target, invSpecAttack));
                hitTargets.Add(target);
            }
        }

        foreach (var target in hitTargets)
        {
            if (target.currentHealth <= 0)
            {
                yield return TurnManager.Instance.HandleEnemyDeath(target);
                yield return TurnManager.Instance.HandlePlayerDeath(target);
            }
        }

        TurnManager.Instance.CheckWinLose();
    }

    public IEnumerator FlashDamageEffect(CharacterStats target)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        for (int i = 0; i < 3; i++)

            sr.color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(0.1f);
            sr.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.1f);
            sr.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.1f);
        }

    public IEnumerator ShakeCamera()
    {
        if (battleCamera == null)
            yield break;

        originalCameraPosition = battleCamera.localPosition;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetY = Random.Range(-shakeAmount, shakeAmount);
            battleCamera.localPosition = new Vector3(
                originalCameraPosition.x,
                originalCameraPosition.y + offsetY,
                originalCameraPosition.z
            );

            elapsed += shakeSpeed;
            yield return new WaitForSeconds(shakeSpeed);
        }

        battleCamera.localPosition = originalCameraPosition;
    }
}