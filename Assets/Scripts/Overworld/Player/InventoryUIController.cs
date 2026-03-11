using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    [Header("Inventory Roots")]
    [SerializeField] private GameObject inventoryRoot;
    [SerializeField] private GameObject[] inventoryScreens;
    [SerializeField] private UITabController uiTabController;

    [Header("Item UI")]
    [SerializeField] private Transform itemGrid1;
    [SerializeField] private Transform itemGrid2;
    [SerializeField] private GameObject itemButtonPrefab;

    [SerializeField] private GameObject leftButton;
    [SerializeField] private GameObject rightButton;

    private const int maxItemsPerGrid = 8;
    private int currentGrid = 0;

    [Header("First Selected Buttons")]
    [SerializeField] private GameObject rootFirstButton;
    [SerializeField] private NavMeshSurface enemyPatrolSurface;

    private Controls controls;

    private bool inventoryOpen = false;
    private bool screenLocked = false;
    private int currentScreen = -1;

    private PlayerControl player;

    private void Awake()
    {
        player = FindFirstObjectByType<PlayerControl>();
        if (uiTabController == null)
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
        {
            OpenInventory();
            player.DisableControls();
            enemyPatrolSurface.enabled = false;
        }
        else
        {
            CloseInventory();
            player.EnableControls();
            enemyPatrolSurface.enabled = true;
        }
    }

    void OpenInventory()
    {
        uiTabController.ResetTabs();
        inventoryOpen = true;

        inventoryRoot.SetActive(true);

        RefreshItemUI();
        ShowGrid(0);

        EventSystem.current.SetSelectedGameObject(rootFirstButton);

        controls.Player.Disable();
        controls.UI.Enable();
    }

    public void NextGrid()
    {
        if (currentGrid == 0)
        {
            currentGrid = 1;
            ShowGrid(currentGrid);
        }
    }

    public void PreviousGrid()
    {
        if (currentGrid == 1)
        {
            currentGrid = 0;
            ShowGrid(currentGrid);
        }
    }

    void ShowGrid(int index)
    {
        itemGrid1.gameObject.SetActive(index == 0);
        itemGrid2.gameObject.SetActive(index == 1);

        leftButton.SetActive(index == 1);
        rightButton.SetActive(index == 0 && Inventory.Instance.items.Count > maxItemsPerGrid);
    }

    void RefreshItemUI()
    {
        ClearGrid(itemGrid1);
        ClearGrid(itemGrid2);

        var items = Inventory.Instance.items;

        Dictionary<Item, InventoryItem> uniqueItems = new Dictionary<Item, InventoryItem>();

        foreach (var invItem in items)
        {
            if (!uniqueItems.ContainsKey(invItem.itemData))
            {
                uniqueItems.Add(invItem.itemData, invItem);
            }
        }

        int index = 0;

        foreach (var invItem in uniqueItems.Values)
        {
            Transform targetGrid = (index < maxItemsPerGrid) ? itemGrid1 : itemGrid2;

            GameObject buttonObj = Instantiate(itemButtonPrefab, targetGrid);

            ActionButton actionButton = buttonObj.GetComponent<ActionButton>();

            if (actionButton != null)
            {
                actionButton.Setup(invItem, OnItemClicked);
            }

            index++;
        }

        rightButton.SetActive(uniqueItems.Count > maxItemsPerGrid);
        leftButton.SetActive(false);
    }

    void OnItemClicked(object action)
    {
        if (action is InventoryItem item)
        {
            Debug.Log("Clicked item: " + item.itemData.itemName);

            Inventory.Instance.flavorTextUI.ShowImmediateText(item.itemData.flavorText);
        }
    }

    void ClearGrid(Transform grid)
    {
        foreach (Transform child in grid)
        {
            Destroy(child.gameObject);
        }
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
        enemyPatrolSurface.enabled = true;
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