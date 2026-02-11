using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonContainer;

    public void ShowMenu<T>(List<T> actions, CharacterStats currentCharacter, System.Action<CharacterStats, T> onClickCallback)
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
                var target = TurnManager.Instance.enemyParty[0];

                CombatSystem.Instance.ExecuteAttack(character, target, attack as Attack);
                TurnManager.Instance.EndTurn();
            }
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
                Inventory.Instance.UseItem(item as Item, character);
                TurnManager.Instance.EndTurn();
            }
        );
    }

    public MenuController specItemMenuController;

    public void ShowSpecialItemMenu(CharacterStats player)
    {
        specItemMenuController.ShowMenu<Attack>(
            player.attacks,
            player,
            (character, specAttack) =>
            {
                var target = TurnManager.Instance.enemyParty[0];

                CombatSystem.Instance.ExecuteAttack(character, target, specAttack as Attack);
                //CharacterStats.RemoveAttack(specAttack as Attack, character);
                TurnManager.Instance.EndTurn();
            }
        );
    }
}