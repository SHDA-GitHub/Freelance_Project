using System.Collections.Generic;
using TMPro;
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

    [SerializeField] private GameObject descriptionMenu;
    [SerializeField] private TextMeshProUGUI descriptionTextUI;

    private enum InventoryType { Items, Specials }
    private InventoryType currentInventoryType = InventoryType.Items;

    [Header("Item UI")]
    [SerializeField] private Transform itemGrid1;
    [SerializeField] private Transform itemGrid2;
    [SerializeField] private GameObject itemButtonPrefab;

    [SerializeField] private GameObject leftItemButton;
    [SerializeField] private GameObject rightItemButton;

    [Header("SpecialAttack UI")]
    [SerializeField] private Transform specialGrid1;
    [SerializeField] private Transform specialGrid2;
    [SerializeField] private GameObject specAttackButtonPrefab;

    [SerializeField] private GameObject leftSpecButton;
    [SerializeField] private GameObject rightSpecButton;

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
        }
        else
        {
            CloseInventory();
        }
    }

    void OpenInventory()
    {
        uiTabController.ResetTabs();
        inventoryOpen = true;

        inventoryRoot.SetActive(true);

        RefreshItemUI();
        ShowItemGrid(0);

        player.DisableControls();
        controls.Player.Disable();
        enemyPatrolSurface.enabled = false;
        EventSystem.current.SetSelectedGameObject(rootFirstButton);
    }

    public void NextGrid()
    {
        if (currentInventoryType == InventoryType.Items)
        {
            currentGrid = Mathf.Min(currentGrid + 1, 1);
            ShowItemGrid(currentGrid);
        }
        else
        {
            currentGrid = Mathf.Min(currentGrid + 1, 1);
            ShowSpecialAttackGrid(currentGrid);
        }
    }

    public void PreviousGrid()
    {
        if (currentInventoryType == InventoryType.Items)
        {
            currentGrid = Mathf.Max(currentGrid - 1, 0);
            ShowItemGrid(currentGrid);
        }
        else
        {
            currentGrid = Mathf.Max(currentGrid - 1, 0);
            ShowSpecialAttackGrid(currentGrid);
        }
    }

    void ShowItemGrid(int index)
    {
        itemGrid1.gameObject.SetActive(index == 0);
        itemGrid2.gameObject.SetActive(index == 1);

        leftItemButton.SetActive(index == 1);
        rightItemButton.SetActive(index == 0 && Inventory.Instance.items.Count > maxItemsPerGrid);
    }

    void ShowSpecialAttackGrid(int index)
    {
        specialGrid1.gameObject.SetActive(index == 0);
        specialGrid2.gameObject.SetActive(index == 1);

        leftSpecButton.SetActive(index == 1);
        rightSpecButton.SetActive(index == 0 && Inventory.Instance.specAttacks.Count > maxItemsPerGrid);
    }

    void RefreshItemUI()
    {
        ClearGrid(itemGrid1);
        ClearGrid(itemGrid2);
        ClearGrid(specialGrid1);
        ClearGrid(specialGrid2);

        var itemGroups = new Dictionary<Item, int>();
        foreach (var invItem in Inventory.Instance.items)
        {
            if (itemGroups.ContainsKey(invItem.itemData))
                itemGroups[invItem.itemData]++;
            else
                itemGroups[invItem.itemData] = 1;
        }

        int index = 0;
        foreach (var kvp in itemGroups)
        {
            var newItem = new InventoryItem(kvp.Key);
            Transform targetGrid = (index < maxItemsPerGrid) ? itemGrid1 : itemGrid2;
            var button = Instantiate(itemButtonPrefab, targetGrid);
            button.GetComponent<ActionButton>().Setup(newItem, OnItemClicked);
            index++;
        }

        var specGroups = new Dictionary<SpecialAttack, int>();
        foreach (var invSpec in Inventory.Instance.specAttacks)
        {
            if (specGroups.ContainsKey(invSpec.attackData))
                specGroups[invSpec.attackData]++;
            else
                specGroups[invSpec.attackData] = 1;
        }

        index = 0;
        foreach (var kvp in specGroups)
        {
            var newSpec = new InventorySpecialAttack(kvp.Key);
            Transform targetGrid = (index < maxItemsPerGrid) ? specialGrid1 : specialGrid2;
            var button = Instantiate(specAttackButtonPrefab, targetGrid);
            button.GetComponent<ActionButton>().Setup(newSpec, OnItemClicked);
            index++;
        }

        leftItemButton.SetActive(false);
        rightItemButton.SetActive(itemGroups.Count > maxItemsPerGrid);
        leftSpecButton.SetActive(false);
        rightSpecButton.SetActive(specGroups.Count > maxItemsPerGrid);
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
        player.EnableControls();
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

    public void ShowItemsInventory()
    {
        currentInventoryType = InventoryType.Items;
        ShowItemGrid(0);
        itemGrid1.parent.gameObject.SetActive(true);
        specialGrid1.parent.gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(leftItemButton);
    }

    public void ShowSpecialsInventory()
    {
        currentInventoryType = InventoryType.Specials;
        ShowSpecialAttackGrid(0);
        specialGrid1.parent.gameObject.SetActive(true);
        itemGrid1.parent.gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(leftSpecButton);
    }

    public void ShowDescription(string text)
    {
        if (descriptionMenu == null || descriptionTextUI == null)
            return;

        descriptionMenu.SetActive(true);
        descriptionTextUI.text = text;
    }

    public void HideDescription()
    {
        if (descriptionMenu == null)
            return;

        descriptionMenu.SetActive(false);
    }

}