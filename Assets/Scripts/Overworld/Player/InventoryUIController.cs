using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    [Header("Inventory Roots")]
    [SerializeField] private GameObject inventoryRoot;
    [SerializeField] private GameObject[] inventoryScreens;
    [SerializeField] private UITabController uiTabController;

    [Header("First Selected Buttons")]
    [SerializeField] private GameObject rootFirstButton;

    private Controls controls;

    private bool inventoryOpen = false;
    private bool screenLocked = false;
    private int currentScreen = -1;

    private void Awake()
    {
        uiTabController = GetComponent<UITabController>();
        controls = new Controls();

        controls.Player.InventoryOpen.started += OnInventoryToggle;
        controls.UI.Cancel.performed += OnCancel;

        controls.Player.Enable();
        controls.UI.Enable();
    }

    void OnInventoryToggle(InputAction.CallbackContext ctx)
    {
        if (!inventoryOpen)
            OpenInventory();
        else
            CloseInventory();
    }

    void OpenInventory()
    {
        uiTabController.ResetTabs();
        inventoryOpen = true;

        inventoryRoot.SetActive(true);
        EventSystem.current.SetSelectedGameObject(rootFirstButton);

        controls.Player.Disable();
        controls.UI.Enable();
    }

    void CloseInventory()
    {
        inventoryOpen = false;
        screenLocked = false;

        inventoryRoot.SetActive(false);

        foreach (var screen in inventoryScreens)
            screen.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);

        controls.UI.Disable();
        controls.Player.Enable();
    }

    public void OpenScreen(int index)
    {
        if (index < 0 || index >= inventoryScreens.Length)
            return;

        inventoryRoot.SetActive(false);

        foreach (var screen in inventoryScreens)
            screen.SetActive(false);

        inventoryScreens[index].SetActive(true);

        currentScreen = index;
        screenLocked = true;
    }

    void OnCancel(InputAction.CallbackContext ctx)
    {
        if (!inventoryOpen)
            return;

        if (screenLocked)
        {
            CloseScreen();
        }
        else
        {
            CloseInventory();
        }
    }

    void CloseScreen()
    {
        inventoryScreens[currentScreen].SetActive(false);
        inventoryRoot.SetActive(true);

        EventSystem.current.SetSelectedGameObject(rootFirstButton);

        screenLocked = false;
        currentScreen = -1;
    }
}