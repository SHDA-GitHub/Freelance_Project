using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
                yield return new WaitForSeconds(0.3f);
                yield return flavorTextUI.ShowTextCoroutine(
                    $"{attacker.characterName} missed!"
                );
                yield return new WaitForSeconds(0.3f);
                yield break;
            }
        }
        if (attack.attackSound != null)
        AudioManager.Instance.PlaySFX(attack.attackSound);
        int offenseBonus = attacker.GetOffenseModifier();
        int defenseBonus = target.GetDefenseModifier();

        int finalDamage = attack.damage + offenseBonus - defenseBonus;

        finalDamage = Mathf.Max(0, finalDamage);

        target.ReceiveDamage(finalDamage);
        if (TurnManager.Instance.playerParty.Contains(target) && finalDamage > 0)
        {
            StartCoroutine(ShakeCamera());
        }
        yield return StartCoroutine(FlashDamageEffect(target));
        yield return new WaitForSeconds(0.3f);
        if (finalDamage > 0)
        {yield return flavorTextUI.ShowTextCoroutine($"{target.characterName} took {finalDamage} damage!");}
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
                if (target.IsImmune(attack.statusEffect))
                {
                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is immune to being {attack.statusEffect}!"
                    );
                }
                else
                {
                    target.ApplyStatus(attack.statusEffect, attack.statusDuration, attack.dotAmount, attack.dotDrainsPP);

                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is now {attack.statusEffect}!"
                    );

                    audioManager.clip = statusEffectGain;
                    audioManager.Play();
                }

                yield return new WaitForSeconds(0.3f);
            }
        }
        if (attack.stunstatusEffect != StunStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < attack.statusChance)
            {
                if (target.IsImmune(attack.stunstatusEffect))
                {
                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is immune to being {attack.stunstatusEffect}!"
                    );
                }
                else
                {
                    target.ApplyStun(attack.stunstatusEffect, attack.statusDuration);

                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is now {attack.stunstatusEffect}!"
                    );

                    audioManager.clip = statusEffectGain;
                    audioManager.Play();
                }

                yield return new WaitForSeconds(0.3f);
            }
        }
        if (attack.missStatusEffect != MissStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < attack.statusChance)
            {
                if (target.IsImmune(attack.missStatusEffect))
                {
                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is immune to being {attack.missStatusEffect}!"
                    );
                }
                else
                {
                    target.ApplyMiss(attack.missStatusEffect, attack.statusDuration);

                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is now {attack.missStatusEffect}!"
                    );

                    audioManager.clip = statusEffectGain;
                    audioManager.Play();
                }

                yield return new WaitForSeconds(0.3f);
            }
        }
        if (attack.statChangeEffect != OffenseDefenseChangeStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < attack.statusChance)
            {
                if (target.IsImmune(attack.statChangeEffect))
                {
                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is immune to being {attack.statChangeEffect}!"
                    );
                }
                else
                {
                    target.ApplyStatChange(attack.statChangeEffect, attack.statusDuration, attack.offenseChange, attack.defenseChange);

                    if (attack.offenseChange > 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went up by {attack.offenseChange}!"
                        );
                    }
                    else if (attack.defenseChange > 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s defense went up by {attack.defenseChange}!"
                        );
                    }
                    else if (attack.offenseChange < 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went down by {attack.offenseChange}!"
                        );
                    }
                    else if (attack.defenseChange < 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s defense went down by {attack.defenseChange}!"
                        );
                    }
                    else if (attack.offenseChange > 0 && attack.defenseChange > 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went up by {attack.offenseChange}, and defense went up by {attack.defenseChange}!"
                        );
                    }
                    else if (attack.offenseChange < 0 && attack.defenseChange < 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went down by {attack.offenseChange}, and defense went down by {attack.defenseChange}!"
                        );
                    }
                    else if (attack.offenseChange > 0 && attack.defenseChange < 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went up by {attack.offenseChange}, and defense went down by {attack.defenseChange}!"
                        );
                    }
                    else if (attack.offenseChange < 0 && attack.defenseChange > 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went up by {attack.offenseChange}, and defense went down by {attack.defenseChange}!"
                        );
                    }

                    audioManager.clip = statusEffectGain;
                    audioManager.Play();
                }

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
                yield return new WaitForSeconds(0.3f);
                yield return flavorTextUI.ShowTextCoroutine(
                    $"{attacker.characterName} missed!"
                );
                yield return new WaitForSeconds(0.3f);
                yield break;
            }
        }
        if (specAttack.attackSound != null)
        AudioManager.Instance.PlaySFX(specAttack.attackSound);
        int offenseBonus = attacker.GetOffenseModifier();
        int defenseBonus = target.GetDefenseModifier();

        int finalDamage = specAttack.damage + offenseBonus - defenseBonus;

        finalDamage = Mathf.Max(0, finalDamage);

        target.ReceiveDamage(finalDamage);
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
        if (finalDamage > 0)
        {yield return flavorTextUI.ShowTextCoroutine($"{target.characterName} took {finalDamage} damage!");}
        yield return new WaitForSeconds(0.3f);
        if (specAttack.statusEffect != DOTStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < specAttack.statusChance)
            {
                if (target.IsImmune(specAttack.statusEffect))
                {
                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is immune to being {specAttack.statusEffect}!"
                    );
                }
                else
                {
                    target.ApplyStatus(specAttack.statusEffect, specAttack.statusDuration, specAttack.dotAmount, specAttack.dotDrainsPP);

                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is now {specAttack.statusEffect}!"
                    );

                    audioManager.clip = statusEffectGain;
                    audioManager.Play();
                }

                yield return new WaitForSeconds(0.3f);
            }
        }
        if (specAttack.stunstatusEffect != StunStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < specAttack.statusChance)
            {
                if (target.IsImmune(specAttack.stunstatusEffect))
                {
                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is immune to being {specAttack.stunstatusEffect}!"
                    );
                }
                else
                {
                    target.ApplyStun(specAttack.stunstatusEffect, specAttack.statusDuration);

                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is now {specAttack.stunstatusEffect}!"
                    );

                    audioManager.clip = statusEffectGain;
                    audioManager.Play();
                }

                yield return new WaitForSeconds(0.3f);
            }
        }
        if (specAttack.missStatusEffect != MissStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < specAttack.statusChance)
            {
                if (target.IsImmune(specAttack.missStatusEffect))
                {
                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is immune to being {specAttack.missStatusEffect}!"
                    );
                }
                else
                {
                    target.ApplyMiss(specAttack.missStatusEffect, specAttack.statusDuration);

                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is now {specAttack.missStatusEffect}!"
                    );

                    audioManager.clip = statusEffectGain;
                    audioManager.Play();
                }

                yield return new WaitForSeconds(0.3f);
            }
        }
        if (specAttack.statChangeEffect != OffenseDefenseChangeStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < specAttack.statusChance)
            {
                if (target.IsImmune(specAttack.statChangeEffect))
                {
                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is immune to being {specAttack.statChangeEffect}!"
                    );
                }
                else
                {
                    target.ApplyStatChange(specAttack.statChangeEffect, specAttack.statusDuration, specAttack.offenseChange, specAttack.defenseChange);

                    if (specAttack.offenseChange > 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went up by {specAttack.offenseChange}!"
                        );
                    }
                    else if (specAttack.defenseChange > 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s defense went up by {specAttack.defenseChange}!"
                        );
                    }
                    else if (specAttack.offenseChange < 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went down by {specAttack.offenseChange}!"
                        );
                    }
                    else if (specAttack.defenseChange < 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s defense went down by {specAttack.defenseChange}!"
                        );
                    }
                    else if (specAttack.offenseChange > 0 && specAttack.defenseChange > 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went up by {specAttack.offenseChange}, and defense went up by {specAttack.defenseChange}!"
                        );
                    }
                    else if (specAttack.offenseChange < 0 && specAttack.defenseChange < 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went down by {specAttack.offenseChange}, and defense went down by {specAttack.defenseChange}!"
                        );
                    }
                    else if (specAttack.offenseChange > 0 && specAttack.defenseChange < 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went up by {specAttack.offenseChange}, and defense went down by {specAttack.defenseChange}!"
                        );
                    }
                    else if (specAttack.offenseChange < 0 && specAttack.defenseChange > 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went up by {specAttack.offenseChange}, and defense went down by {specAttack.defenseChange}!"
                        );
                    }

                    audioManager.clip = statusEffectGain;
                    audioManager.Play();
                }
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

        if (item.isKeyItem)
        {
            yield break;
        }

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
        if (item.statusEffect != DOTStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < item.statusChance)
            {
                if (target.IsImmune(item.statusEffect))
                {
                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is immune to being {item.statusEffect}!"
                    );
                }
                else
                {
                    target.ApplyStatus(item.statusEffect, item.statusDuration, item.dotAmount, item.dotDrainsPP);

                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is now {item.statusEffect}!"
                    );

                    audioManager.clip = statusEffectGain;
                    audioManager.Play();
                }

                yield return new WaitForSeconds(0.3f);
            }
        }
        if (item.stunstatusEffect != StunStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < item.statusChance)
            {
                if (target.IsImmune(item.stunstatusEffect))
                {
                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is immune to being {item.stunstatusEffect}!"
                    );
                }
                else
                {
                    target.ApplyStun(item.stunstatusEffect, item.statusDuration);

                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is now {item.stunstatusEffect}!"
                    );

                    audioManager.clip = statusEffectGain;
                    audioManager.Play();
                }

                yield return new WaitForSeconds(0.3f);
            }
        }
        if (item.missStatusEffect != MissStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < item.statusChance)
            {
                if (target.IsImmune(item.missStatusEffect))
                {
                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is immune to being {item.missStatusEffect}!"
                    );
                }
                else
                {
                    target.ApplyMiss(item.missStatusEffect, item.statusDuration);

                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is now {item.missStatusEffect}!"
                    );

                    audioManager.clip = statusEffectGain;
                    audioManager.Play();
                }

                yield return new WaitForSeconds(0.3f);
            }
        }
        if (item.statChangeEffect != OffenseDefenseChangeStatusEffectType.None)
        {
            int roll = Random.Range(0, 100);

            if (roll < item.statusChance)
            {
                if (target.IsImmune(item.statChangeEffect))
                {
                    yield return flavorTextUI.ShowTextCoroutine(
                        $"{target.characterName} is immune to being {item.statChangeEffect}!"
                    );
                }
                else
                {
                    target.ApplyStatChange(item.statChangeEffect, item.statusDuration, item.offenseChange, item.defenseChange);

                    if (item.offenseChange > 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went up by {item.offenseChange}!"
                        );
                    }
                    else if (item.defenseChange > 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s defense went up by {item.defenseChange}!"
                        );
                    }
                    else if (item.offenseChange < 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went down by {item.offenseChange}!"
                        );
                    }
                    else if (item.defenseChange < 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s defense went down by {item.defenseChange}!"
                        );
                    }
                    else if (item.offenseChange > 0 && item.defenseChange > 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went up by {item.offenseChange}, and defense went up by {item.defenseChange}!"
                        );
                    }
                    else if (item.offenseChange < 0 && item.defenseChange < 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went down by {item.offenseChange}, and defense went down by {item.defenseChange}!"
                        );
                    }
                    else if (item.offenseChange > 0 && item.defenseChange < 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went up by {item.offenseChange}, and defense went down by {item.defenseChange}!"
                        );
                    }
                    else if (item.offenseChange < 0 && item.defenseChange > 0)
                    {
                        yield return flavorTextUI.ShowTextCoroutine(
                            $"{target.characterName}'s offense went up by {item.offenseChange}, and defense went down by {item.defenseChange}!"
                        );
                    }
                    audioManager.clip = statusEffectGain;
                    audioManager.Play();
                }
                yield return new WaitForSeconds(0.3f);
            }
        }
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
        IEnumerator Cleanse(CharacterStats character)
        {

            if (item.removeAllStatusEffects)
            {
                if (character.IsDOT() || character.IsStunned() || character.IsMissAttack())
                {
                    character.RemoveAllStatusEffects();
                    flavorTextUI.ShowImmediateText($"{character.characterName} was cleansed!");
                    yield return new WaitForSeconds(0.3f);
                    yield break;
                }
            }

            if (item.removeDOT && character.IsDOT())
            {
                character.RemoveDOTEffects();
                flavorTextUI.ShowImmediateText($"{character.characterName} was cured!");
                yield return new WaitForSeconds(0.3f);
            }
            if (item.removeStun && character.IsStunned())
            {
                character.RemoveStunEffects();
                flavorTextUI.ShowImmediateText($"{character.characterName} is no longer stunned!");
                yield return new WaitForSeconds(0.3f);
            }
            if (item.removeMiss && character.IsMissAttack())
            {
                character.RemoveMissEffects();
                flavorTextUI.ShowImmediateText($"{character.characterName}'s accuracy was restored!");
                yield return new WaitForSeconds(0.3f);
            }
        }

        if (item.healAllParty)
        {
            foreach (var member in TurnManager.Instance.playerParty)
            {
                if (member != null)
                    yield return StartCoroutine(Cleanse(member));
            }
        }
        else
        {
            yield return StartCoroutine(Cleanse(target));
            yield return new WaitForSeconds(0.3f);
        }
        yield return new WaitForSeconds(0.75f);
        TurnManager.Instance.battleHUD.UpdateHUD();
        if (item.consumable)
        Inventory.Instance.items.Remove(invItem);
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
        {
            sr.color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(0.1f);

            sr.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.1f);
        }

        sr.color = Color.white;
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