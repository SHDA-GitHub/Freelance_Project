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

    [Header("Party Selection")]
    [SerializeField] private GameObject partySelectMenu;
    [SerializeField] private GameObject partyFirstButton;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject inputBlocker;
    [SerializeField] private AudioClip useItemSFX;

    private InventoryItem pendingItemUse;

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

        controls.Player.InventoryOpen.performed += OnInventoryToggle;
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
        PartyManager.Instance.UpdateCoinUI();
        uiTabController.ResetTabs();
        inventoryOpen = true;

        inventoryRoot.SetActive(true);
        SetInputBlocked(false);

        RefreshItemUI();
        ShowItemGrid(0);

        specialGrid1.parent.gameObject.SetActive(false);
        keyItemGrid1.parent.gameObject.SetActive(false);

        player.DisableControls();
        controls.Player.Disable();
        enemyPatrolSurface.enabled = false;
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(rootFirstButton);
        }
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

        SetInputBlocked(true);

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
        SetInputBlocked(true);

        if (!(lockedActionData is InventoryItem invItem))
            return;

        if (DialogueManager.Instance == null)
            return;

        if (invItem.itemData.isKeyItem)
        {
            NPCDialogue cannotDropDialogue = new NPCDialogue();

            NPCDialogue.DialogueLine line = new NPCDialogue.DialogueLine
            {
                dialogueText = $"You cannot drop a key item."
            };

            CloseItemMenu();

            DialogueManager.Instance.onDialogueEnded += HandleItemResultDialogueEnd;

            DialogueManager.Instance.StartDialogue(
                cannotDropDialogue,
                new NPCDialogue.DialogueLine[] { line },
                null
            );

            return;
        }

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

        tempDialogue.onChoiceMade += (string yes) =>
        {
            if (yes == "yes")
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

        if (!DialogueManager.Instance.IsDialogueActive())
        {
            SetInputBlocked(false);
        }
    }

    public void UseItem()
    {
        SetInputBlocked(true);

        if (!(lockedActionData is InventoryItem invItem))
            return;

        if (DialogueManager.Instance == null)
            return;

        if (invItem.itemData.healAmount <= 0 && invItem.itemData.ppAmount <= 0)
        {
            ShowCannotUseDialogue(invItem);
            return;
        }

        if (invItem.itemData.isKeyItem)
        {
            NPCDialogue cannotDropDialogue = new NPCDialogue();

            NPCDialogue.DialogueLine line = new NPCDialogue.DialogueLine
            {
                dialogueText = $"You cannot use a key item."
            };

            CloseItemMenu();

            DialogueManager.Instance.onDialogueEnded += HandleItemResultDialogueEnd;

            DialogueManager.Instance.StartDialogue(
                cannotDropDialogue,
                new NPCDialogue.DialogueLine[] { line },
                null
            );

            return;
        }

        string itemName = invItem.itemData.itemName;

        NPCDialogue tempDialogue = new NPCDialogue();

        NPCDialogue.DialogueLine confirmLine = new NPCDialogue.DialogueLine
        {
            dialogueText = $"Do you want to use the {itemName}?",
            isChoiceActive = true,
            yesButtonText = "Yes",
            noButtonText = "No"
        };

        confirmLine.yesDialogueLines = new NPCDialogue.DialogueLine[]
        {
        new NPCDialogue.DialogueLine
        {
            dialogueText = $"Select a party member."
        }
        };

        confirmLine.noDialogueLines = new NPCDialogue.DialogueLine[]
        {
        new NPCDialogue.DialogueLine
        {
            dialogueText = $"You decided not to use the {itemName}."
        }
        };

        tempDialogue.onChoiceMade += (string choice) =>
        {
            if (choice == "yes")
            {
                pendingItemUse = invItem;

                partySelectMenu.SetActive(true);

                if (partyFirstButton != null)
                    EventSystem.current.SetSelectedGameObject(partyFirstButton);
            }
        };

        CloseItemMenu();

        DialogueManager.Instance.onDialogueEnded += HandleItemResultDialogueEnd;

        DialogueManager.Instance.StartDialogue(
            tempDialogue,
            new NPCDialogue.DialogueLine[] { confirmLine },
            null
        );
    }

    private void HandleItemResultDialogueEnd()
    {
        SetInputBlocked(false);

        DialogueManager.Instance.onDialogueEnded -= HandleItemResultDialogueEnd;
    }

    void ShowCannotUseDialogue(InventoryItem invItem)
    {
        string itemName = invItem.itemData.itemName;

        NPCDialogue failDialogue = new NPCDialogue();

        NPCDialogue.DialogueLine line = new NPCDialogue.DialogueLine
        {
            dialogueText = $"You cannot use the {itemName} outside of battle."
        };

        CloseItemMenu();

        DialogueManager.Instance.onDialogueEnded += HandleItemResultDialogueEnd;

        DialogueManager.Instance.StartDialogue(
            failDialogue,
            new NPCDialogue.DialogueLine[] { line },
            null
        );
    }

    private void SetInputBlocked(bool blocked)
    {
        if (inputBlocker != null)
            inputBlocker.SetActive(blocked);
    }

    public void OnPartyMemberSelected(PlayerStatsSO target)
    {
        if (pendingItemUse == null)
            return;

        InventoryItem item = pendingItemUse;

        int beforeHP = target.currentHealth;
        int beforePP = target.currentPP;

        ApplyItemEffectToTarget(item, target);

        if (audioSource != null && useItemSFX != null)
            audioSource.PlayOneShot(useItemSFX);

        ShowItemResultDialogue(item, target, beforeHP, beforePP);

        RemoveItem(item);

        pendingItemUse = null;

        partySelectMenu.SetActive(false);
    }

    void ShowItemResultDialogue(InventoryItem invItem, PlayerStatsSO target, int beforeHP, int beforePP)
    {
        if (DialogueManager.Instance == null)
            return;

        string itemName = invItem.itemData.itemName;
        string targetName = target.characterName;

        int heal = invItem.itemData.healAmount;
        int pp = invItem.itemData.ppAmount;

        var lines = new System.Collections.Generic.List<NPCDialogue.DialogueLine>();

        lines.Add(new NPCDialogue.DialogueLine
        {
            dialogueText = $"You used the {itemName} on {targetName}."
        });

        int actualHeal = target.currentHealth - beforeHP;
        int actualPP = target.currentPP - beforePP;

        if (heal > 0)
        {
            lines.Add(new NPCDialogue.DialogueLine
            {
                dialogueText = actualHeal > 0
                    ? $"{targetName} recovered {actualHeal} HP."
                    : $"{targetName}'s HP was maxed out."
            });
        }

        if (pp > 0)
        {
            lines.Add(new NPCDialogue.DialogueLine
            {
                dialogueText = actualPP > 0
                    ? $"{targetName} recovered {actualPP} PP."
                    : $"{targetName}'s PP was maxed out."
            });
        }

        NPCDialogue resultDialogue = new NPCDialogue();

        DialogueManager.Instance.onDialogueEnded += HandleItemResultDialogueEnd;

        DialogueManager.Instance.StartDialogue(resultDialogue, lines.ToArray(), null);
    }

    void ApplyItemEffectToTarget(InventoryItem invItem, PlayerStatsSO targetStats)
    {
        if (targetStats == null)
            return;

        Item item = invItem.itemData;

        if (item.healAmount > 0)
        {
            targetStats.OverworldAddHP(item.healAmount);
        }

        if (item.ppAmount > 0)
        {
            targetStats.OverworldAddPP(item.ppAmount);
        }
    }

    public void DropSpecialAttack()
    {
        SetInputBlocked(true);

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

        tempDialogue.onChoiceMade += (string yes) =>
        {
            if (yes == "yes")
            {
                RemoveSpecial(invSpecial);

            }
        };

        CloseItemMenu();

        DialogueManager.Instance.onDialogueEnded += HandleItemResultDialogueEnd;

        DialogueManager.Instance.StartDialogue(
            tempDialogue,
            new NPCDialogue.DialogueLine[] { confirmLine },
            null
        );
    }

    public void UseSpecAttack()
    {
        SetInputBlocked(true);

        if (!(lockedActionData is InventorySpecialAttack invSpecAttack))
            return;

        if (DialogueManager.Instance == null)
            return;

        string itemName = invSpecAttack.attackData.specAttackName;

        NPCDialogue failDialogue = new NPCDialogue();

        NPCDialogue.DialogueLine line = new NPCDialogue.DialogueLine
        {
            dialogueText = $"You cannot use {itemName} outside of battle."
        };

        DialogueManager.Instance.onDialogueEnded += HandleItemResultDialogueEnd;

        CloseItemMenu();

        DialogueManager.Instance.StartDialogue(
            failDialogue,
            new NPCDialogue.DialogueLine[] { line },
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

        EventSystem.current.SetSelectedGameObject(rootFirstButton);
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
        if (partySelectMenu.activeSelf)
        {
            partySelectMenu.SetActive(false);
            pendingItemUse = null;

            EventSystem.current.SetSelectedGameObject(rootFirstButton);
            return;
        }

        if (!inventoryOpen)
            return;

        if (itemMenuOpen)
        {
            CloseItemMenu();
            SetInputBlocked(false);
            return;
        }

        if (screenLocked)
        {
            CloseScreen();
            SetInputBlocked(false);
        }
        else
        {
            CloseInventory();
            SetInputBlocked(false);
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