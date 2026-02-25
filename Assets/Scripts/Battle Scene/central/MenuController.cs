using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonContainer;
    private Controls controls;

    private void Awake()
    {
        controls = new Controls();
        controls.UI.Enable();
    }

    public void ShowMenu<T>(
        List<T> actions,
        CharacterStats currentCharacter,
        System.Action<CharacterStats, T> onClickCallback,
        bool closeOnClick = true)
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
                    onClickCallback?.Invoke(currentCharacter, (T)a);
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
                        TurnManager.Instance.PlayerUseAttack(player, target, attack);
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
        itemMenuController.ShowMenu<InventoryItem>(
            Inventory.Instance.items,
            player,
            (character, invItem) =>
            {
                UIManager.Instance.HideAllMenus();

                TurnManager.Instance.StartTargetSelection(
                    TurnManager.Instance.playerParty,
                    (target) =>
                    {
                        UIManager.Instance.HideAllMenus();

                        StartCoroutine(
                            CombatSystem.Instance.ExecuteItem(character, target, invItem)
                        );
                    },
                    false,
                    true
                );
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
                        TurnManager.Instance.PlayerUseSpecialAttack(player, target, specAttack);
                    },
                    specAttack.targetAllEnemies
                );
            },
            closeOnClick: false
        );
    }
}