using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonContainer;

    public void ShowMenu<T>(
        List<T> actions,
        CharacterStats currentCharacter,
        System.Action<CharacterStats, T> onClickCallback,
        bool closeOnClick = true)
    where T : ScriptableObject
    {
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);
        foreach (var action in actions)
        {
            var btnObj = Instantiate(buttonPrefab, buttonContainer);
            var actionButton = btnObj.GetComponent<ActionButton>();
            if (actionButton != null)
            {
                actionButton.Setup(action, (a) =>
                {
                    onClickCallback?.Invoke(currentCharacter, a as T);
                    if (closeOnClick)
                        gameObject.SetActive(false);

                });
            }
            gameObject.SetActive(true);
        }
    }

    public void HideMenu()
    {
        gameObject.SetActive(false);
    }

    public MenuController attackMenuController;

    public void ShowAttackMenu(CharacterStats player)
    {
        attackMenuController.ShowMenu<Attack>(
            player.attacks,
            player,
            (character, attack) =>
            {
                UIManager.Instance.HideAllMenus();

                TurnManager.Instance.StartTargetSelection(
                    TurnManager.Instance.enemyParty,
                    (target) =>
                    {
                        if (attack.targetAllEnemies)
                        {
                            StartCoroutine(
                                CombatSystem.Instance.ExecuteAttackOnAll(
                                    player,
                                    TurnManager.Instance.enemyParty,
                                    attack
                                )
                            );
                        }
                        else
                        {
                            StartCoroutine(
                                CombatSystem.Instance.ExecuteAttack(player, target, attack)
                            );
                        }
                    },
                    attack.targetAllEnemies
                );
            },
            closeOnClick: false
        );
    }

    public MenuController itemMenuController;

    public void ShowItemMenu(CharacterStats player)
    {
        itemMenuController.ShowMenu<Item>(
            Inventory.Instance.items,
            player,
            (character, item) =>
            {
                UIManager.Instance.HideAllMenus();

            TurnManager.Instance.StartTargetSelection(
                TurnManager.Instance.playerParty,
                (target) =>
                {
                    UIManager.Instance.HideAllMenus();

                    StartCoroutine(
                        CombatSystem.Instance.ExecuteItem(character, target, item as Item)
                    );
                });
            },
            closeOnClick: false
        );
    }

    public MenuController specAttackMenuController;

    public void ShowSpecialAttackMenu(CharacterStats player)
    {
        specAttackMenuController.ShowMenu<SpecialAttack>(
            Inventory.Instance.specAttacks,
            player,
            (character, specAttack) =>
            {
                UIManager.Instance.HideAllMenus();

                TurnManager.Instance.StartTargetSelection(
                    TurnManager.Instance.enemyParty,
                    (target) =>
                    {
                        if (specAttack.targetAllEnemies)
                        {
                            StartCoroutine(
                                CombatSystem.Instance.ExecuteSpecialAttackOnAll(
                                    player,
                                    TurnManager.Instance.enemyParty,
                                    specAttack
                                )
                            );
                        }
                        else
                        {
                            StartCoroutine(
                                CombatSystem.Instance.ExecuteSpecialAttack(player, target, specAttack)
                            );
                        }
                    },
                    specAttack.targetAllEnemies
                );
            },
            closeOnClick: false
        );
    }
}