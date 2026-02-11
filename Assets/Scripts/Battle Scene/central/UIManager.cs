using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject mainMenu;
    public GameObject attackMenu;
    public GameObject itemMenu;

    public MenuController attackMenuController;
    public MenuController itemMenuController;

    private CharacterStats currentCharacter;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowPlayerOptions(CharacterStats character)
    {
        currentCharacter = character;
        mainMenu.SetActive(true);
        attackMenu.SetActive(false);
        itemMenu.SetActive(false);
    }

    public void OnAttackSelected()
    {
        mainMenu.SetActive(false);
        attackMenu.SetActive(true);

        attackMenuController.ShowAttackMenu(currentCharacter);
    }

    public void OnItemSelected()
    {
        mainMenu.SetActive(false);
        itemMenu.SetActive(true);

        itemMenuController.ShowItemMenu(currentCharacter);
    }
}
