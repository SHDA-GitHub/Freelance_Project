using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject mainMenu;
    public GameObject attackMenu;
    public GameObject itemMenu;
    public GameObject specialMenu;
    [SerializeField] private AudioSource audioManager;
    [SerializeField] private AudioClip cancelSound;
    public MenuController attackMenuController;
    public MenuController itemMenuController;
    public MenuController specialAttackMenuController;

    private CharacterStats currentCharacter;
    private Controls controls;

    private void Start()
    {
        controls = new Controls();
        controls.UI.Enable();
    }

    private void Update()
    {
        if (!mainMenu.activeSelf && controls.UI.Cancel.triggered)
        {
            AudioManager.Instance.PlaySFX(cancelSound);
            ShowPlayerOptions(currentCharacter);
        }
    }

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
        specialMenu.SetActive(false);
    }

    public void OnAttackSelected()
    {
        mainMenu.SetActive(false);
        attackMenu.SetActive(true);

        attackMenuController.ShowAttackMenu(currentCharacter);
    }

    public void OnAttackCanceled()
    {
        mainMenu.SetActive(true);
        attackMenu.SetActive(false);
    }

    public void OnItemSelected()
    {
        mainMenu.SetActive(false);
        itemMenu.SetActive(true);

        itemMenuController.ShowItemMenu(currentCharacter);
    }

    public void OnItemCanceled()
    {
        mainMenu.SetActive(true);
        itemMenu.SetActive(false);
    }

    public void OnSpecSelected()
    {
        mainMenu.SetActive(false);
        specialMenu.SetActive(true);

        specialAttackMenuController.ShowSpecialAttackMenu(currentCharacter);
    }

    public void OnSpecCanceled()
    {
        mainMenu.SetActive(true);
        specialMenu.SetActive(false);
    }

    public void HideAllMenus()
    {
        mainMenu.SetActive(false);
        attackMenu.SetActive(false);
        itemMenu.SetActive(false);
        specialMenu.SetActive(false);
    }
}
