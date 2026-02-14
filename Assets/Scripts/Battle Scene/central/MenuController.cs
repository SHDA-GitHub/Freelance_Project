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

                TurnManager.Instance.StartTargetSelection((target) =>
                {
                    UIManager.Instance.HideAllMenus();

                    StartCoroutine(CombatSystem.Instance.ExecuteAttack(character, target, attack as Attack));
                });
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
                Inventory.Instance.UseItem(item as Item, character);
                TurnManager.Instance.EndTurn();
            }
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

                TurnManager.Instance.StartTargetSelection((target) =>
                {
                    StartCoroutine(
                        CombatSystem.Instance.ExecuteSpecialAttack(character, target, specAttack as SpecialAttack)
                    );
                });
                Inventory.Instance.UseSpecialAttack(specAttack as SpecialAttack, character);
            },
            closeOnClick: false
        );
    }
}