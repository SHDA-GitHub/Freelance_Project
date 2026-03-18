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

    [Header("Item Action Menu")]
    [SerializeField] private GameObject itemActionMenu;
    [SerializeField] private GameObject useButton;
    [SerializeField] private GameObject dropButton;

    private object lockedActionData = null;
    private bool itemMenuOpen = false;

    [Header("SpecialAttack UI")]
    [SerializeField] private Transform specialGrid1;
    [SerializeField] private Transform specialGrid2;
    [SerializeField] private GameObject specAttackButtonPrefab;

    [SerializeField] private GameObject leftSpecButton;
    [SerializeField] private GameObject rightSpecButton;

    [Header("Key Item UI")]
    [SerializeField] private Transform keyItemGrid1;
    [SerializeField] private Transform keyItemGrid2;
    [SerializeField] private GameObject keyItemButtonPrefab;

    [SerializeField] private GameObject leftKeyItemButton;
    [SerializeField] private GameObject rightKeyItemButton;


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
    private DialogueManager dialogueManager;

    private void Awake()
    {
        dialogueManager = GetComponent<DialogueManager>();
        player = FindFirstObjectByType<PlayerControl>();
        if (uiTabController == null)
        uiTabController = GetComponent<UITabController>();
        controls = new Controls();

        controls.Player.InventoryOpen.started += OnInventoryToggle;
        controls.UI.Cancel.performed += OnCancel;

        controls.Player.Enable();
        controls.UI.Enable();
    }

    public void OnInventoryToggle(InputAction.CallbackContext ctx)
    {
        if (dialogueManager != null && dialogueManager.IsDialogueActive())
            return;

        if (!inventoryOpen)
        {
            OpenInventory();
        }
        else
        {
            CloseInventory();
        }
    }

    public void OpenInventory()
    {
        DialogueManager.Instance.isExternalUILocked = true;
        uiTabController.ResetTabs();
        inventoryOpen = true;

        inventoryRoot.SetActive(true);

        RefreshItemUI();
        ShowItemGrid(0);

        specialGrid1.parent.gameObject.SetActive(false);
        keyItemGrid1.parent.gameObject.SetActive(false);

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

    void ShowKeyItemGrid(int index)
    {
        keyItemGrid1.gameObject.SetActive(index == 0);
        keyItemGrid2.gameObject.SetActive(index == 1);

        leftKeyItemButton.SetActive(index == 1);
        rightKeyItemButton.SetActive(index == 0 && Inventory.Instance.items.FindAll(i => i.itemData.isKeyItem).Count > maxItemsPerGrid);
    }

    public void RefreshItemUI()
    {
        ClearGrid(itemGrid1);
        ClearGrid(itemGrid2);
        ClearGrid(specialGrid1);
        ClearGrid(specialGrid2);
        ClearGrid(keyItemGrid1);
        ClearGrid(keyItemGrid2);

        int index = 0;
        foreach (var invItem in Inventory.Instance.items)
        {
            if (invItem.itemData.isKeyItem) continue;

            Transform targetGrid = (index < maxItemsPerGrid) ? itemGrid1 : itemGrid2;
            var button = Instantiate(itemButtonPrefab, targetGrid);
            button.GetComponent<ActionButton>().Setup(invItem, OnItemClicked);
            index++;
        }

        int keyIndex = 0;
        foreach (var keyItem in Inventory.Instance.keyItems)
        {
            Transform targetGrid = (keyIndex < maxItemsPerGrid) ? keyItemGrid1 : keyItemGrid2;
            var button = Instantiate(keyItemButtonPrefab, targetGrid);
            button.GetComponent<ActionButton>().Setup(keyItem, OnItemClicked);
            keyIndex++;
        }

        index = 0;
        foreach (var invSpecials in Inventory.Instance.specAttacks)
        {
            Transform targetGrid = (index < maxItemsPerGrid) ? specialGrid1 : specialGrid2;
            var button = Instantiate(specAttackButtonPrefab, targetGrid);
            button.GetComponent<ActionButton>().Setup(invSpecials, OnItemClicked);
            index++;
        }

        leftItemButton.SetActive(false);
        rightItemButton.SetActive(Inventory.Instance.items.FindAll(i => !i.itemData.isKeyItem).Count > maxItemsPerGrid);

        leftKeyItemButton.SetActive(false);
        rightKeyItemButton.SetActive(Inventory.Instance.items.FindAll(i => i.itemData.isKeyItem).Count > maxItemsPerGrid);

        leftSpecButton.SetActive(false);
        rightSpecButton.SetActive(Inventory.Instance.specAttacks.Count > maxItemsPerGrid);
    }

    void OnItemClicked(object action)
    {
        if (itemMenuOpen)
            return;

        lockedActionData = action;
        itemMenuOpen = true;

        itemActionMenu.SetActive(true);

        EventSystem.current.SetSelectedGameObject(useButton);
    }

    public bool IsItemMenuOpen()
    {
        return itemMenuOpen;
    }

    public void CloseItemMenu()
    {
        itemMenuOpen = false;
        lockedActionData = null;

        itemActionMenu.SetActive(false);

        EventSystem.current.SetSelectedGameObject(rootFirstButton);
    }

    public void DropItem()
    {
        if (!(lockedActionData is InventoryItem invItem))
            return;

        if (DialogueManager.Instance == null)
            return;

        NPCDialogue tempDialogue = new NPCDialogue();

        string itemName = invItem.itemData.itemName;

        NPCDialogue.DialogueLine confirmLine = new NPCDialogue.DialogueLine
        {
            dialogueText = $"Are you sure you want to throw away the {itemName}?",
            isChoiceActive = true,
            yesButtonText = "Yes",
            noButtonText = "No"
        };

        confirmLine.yesDialogueLines = new NPCDialogue.DialogueLine[]
        {
        new NPCDialogue.DialogueLine
        {
            dialogueText = $"You threw away the {itemName}."
        }
        };

        confirmLine.noDialogueLines = new NPCDialogue.DialogueLine[]
        {
        new NPCDialogue.DialogueLine
        {
            dialogueText = $"You decided to keep the {itemName}."
        }
        };

        tempDialogue.onChoiceMade += (bool yes) =>
        {
            if (yes)
            {
                RemoveItem(invItem);
            }
        };

        CloseItemMenu();

        DialogueManager.Instance.StartDialogue(
            tempDialogue,
            new NPCDialogue.DialogueLine[] { confirmLine },
            null
        );
    }

    public void DropSpecialAttack()
    {
        if (!(lockedActionData is InventorySpecialAttack invSpecial))
            return;

        if (DialogueManager.Instance == null)
            return;

        NPCDialogue tempDialogue = new NPCDialogue();

        string specialName = invSpecial.attackData.specAttackName;

        NPCDialogue.DialogueLine confirmLine = new NPCDialogue.DialogueLine
        {
            dialogueText = $"Are you sure you want to throw away the {specialName}?",
            isChoiceActive = true,
            yesButtonText = "Yes",
            noButtonText = "No"
        };

        confirmLine.yesDialogueLines = new NPCDialogue.DialogueLine[]
        {
        new NPCDialogue.DialogueLine
        {
            dialogueText = $"You threw away the {specialName}."
        }
        };

        confirmLine.noDialogueLines = new NPCDialogue.DialogueLine[]
        {
        new NPCDialogue.DialogueLine
        {
            dialogueText = $"You decided to keep the {specialName}."
        }
        };

        tempDialogue.onChoiceMade += (bool yes) =>
        {
            if (yes)
            {
                RemoveSpecial(invSpecial);
            }
        };

        CloseItemMenu();

        DialogueManager.Instance.StartDialogue(
            tempDialogue,
            new NPCDialogue.DialogueLine[] { confirmLine },
            null
        );
    }

    void RemoveItem(InventoryItem invItem)
    {
        var foundItem = Inventory.Instance.items
            .Find(i => i.itemData == invItem.itemData);

        if (foundItem == null)
            return;

        foundItem.quantity--;

        if (foundItem.quantity <= 0)
            Inventory.Instance.items.Remove(foundItem);

        currentGrid = 0;
        RefreshItemUI();
        ShowItemGrid(0);
    }

    void RemoveSpecial(InventorySpecialAttack invSpecial)
    {
        var foundSpecial = Inventory.Instance.specAttacks
            .Find(i => i.attackData == invSpecial.attackData);

        if (foundSpecial == null)
            return;

        foundSpecial.quantity--;

        if (foundSpecial.quantity <= 0)
            Inventory.Instance.specAttacks.Remove(foundSpecial);

        currentGrid = 0;
        RefreshItemUI();
        ShowItemGrid(0);
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
        DialogueManager.Instance.isExternalUILocked = false;
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

        if (itemMenuOpen)
        {
            CloseItemMenu();
            return;
        }

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
        keyItemGrid1.parent.gameObject.SetActive(false);

        EventSystem.current.SetSelectedGameObject(leftItemButton);
    }

    public void ShowSpecialsInventory()
    {
        currentInventoryType = InventoryType.Specials;
        ShowSpecialAttackGrid(0);

        specialGrid1.parent.gameObject.SetActive(true);
        itemGrid1.parent.gameObject.SetActive(false);
        keyItemGrid1.parent.gameObject.SetActive (false);

        EventSystem.current.SetSelectedGameObject(leftSpecButton);
    }

    public void ShowKeyItemsInventory()
    {
        currentInventoryType = InventoryType.Items;
        ShowKeyItemGrid(0);

        keyItemGrid1.parent.gameObject.SetActive(true);
        itemGrid1.parent.gameObject.SetActive(false);
        specialGrid1.parent.gameObject.SetActive(false);

        EventSystem.current.SetSelectedGameObject(leftKeyItemButton);
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