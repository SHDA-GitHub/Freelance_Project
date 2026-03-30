using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopUIController : MonoBehaviour
{
    [Header("Grids")]
    [SerializeField] private Transform grid1;
    [SerializeField] private Transform grid2;
    [SerializeField] private Transform grid3;

    [SerializeField] private GameObject itemButtonPrefab;

    [Header("Navigation Buttons")]
    [SerializeField] private GameObject leftButton;
    [SerializeField] private GameObject rightButton;

    private const int maxPerGrid = 8;

    private int currentGrid = 0;
    private List<ShopItem> currentItems;

    public void OpenShop(ShopStock stock)
    {
        currentItems = stock.items;
        currentGrid = 0;

        RefreshUI();
        ShowGrid(0);
    }

    void RefreshUI()
    {
        ClearGrid(grid1);
        ClearGrid(grid2);
        ClearGrid(grid3);

        for (int i = 0; i < currentItems.Count; i++)
        {
            Transform targetGrid = GetGridByIndex(i);

            var button = Instantiate(itemButtonPrefab, targetGrid);

            button.GetComponent<ShopItemButton>().Setup(currentItems[i], this);
        }

        UpdateNavButtons();
    }

    Transform GetGridByIndex(int index)
    {
        if (index < maxPerGrid) return grid1;
        if (index < maxPerGrid * 2) return grid2;
        return grid3;
    }

    void ShowGrid(int index)
    {
        grid1.gameObject.SetActive(index == 0);
        grid2.gameObject.SetActive(index == 1);
        grid3.gameObject.SetActive(index == 2);

        currentGrid = index;
        UpdateNavButtons();
    }

    void UpdateNavButtons()
    {
        int maxGridIndex = Mathf.CeilToInt((float)currentItems.Count / maxPerGrid) - 1;

        leftButton.SetActive(currentGrid > 0);
        rightButton.SetActive(currentGrid < maxGridIndex);
    }

    public void NextGrid()
    {
        int maxGridIndex = Mathf.CeilToInt((float)currentItems.Count / maxPerGrid) - 1;
        currentGrid = Mathf.Min(currentGrid + 1, maxGridIndex);
        ShowGrid(currentGrid);
    }

    public void PreviousGrid()
    {
        currentGrid = Mathf.Max(currentGrid - 1, 0);
        ShowGrid(currentGrid);
    }

    void ClearGrid(Transform grid)
    {
        foreach (Transform child in grid)
            Destroy(child.gameObject);
    }

    public void TryBuyItem(ShopItem item)
    {
        if (DialogueManager.Instance == null)
            return;

        NPCDialogue tempDialogue = new NPCDialogue();

        string itemName = (item.type == ShopItemType.Item)
            ? item.item.itemName
            : item.specialAttack.specAttackName;

        NPCDialogue.DialogueLine confirmLine = new NPCDialogue.DialogueLine
        {
            dialogueText = $"Do you want to buy {itemName} for {item.price}?",
            isChoiceActive = true,
            yesButtonText = "Yes",
            noButtonText = "No"
        };

        tempDialogue.onChoiceMade += (string choiceID) =>
        {
            if (choiceID == "yes")
            {
                BuyResult result = BuyItem(item);

                string resultText = "";

                switch (result)
                {
                    case BuyResult.Success:
                        resultText = $"You bought {itemName}.";
                        break;

                    case BuyResult.NotEnoughMoney:
                        resultText = "You don't have enough money to buy this item.";
                        break;

                    case BuyResult.InventoryFull:
                        resultText = "Your inventory is too full to buy this item.";
                        break;
                }

                ShowResultDialogue(resultText);
            }
        };

        confirmLine.noDialogueLines = new NPCDialogue.DialogueLine[]
        {
        new NPCDialogue.DialogueLine
        {
            dialogueText = $"You decided not to buy {itemName}."
        }
        };

        DialogueManager.Instance.StartDialogue(
            tempDialogue,
            new NPCDialogue.DialogueLine[] { confirmLine },
            null
        );
    }

    enum BuyResult
    {
        Success,
        NotEnoughMoney,
        InventoryFull
    }

    BuyResult BuyItem(ShopItem item)
    {
        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("CurrencyManager not found!");
            return BuyResult.NotEnoughMoney;
        }

        float price = item.price;

        if (!CurrencyManager.Instance.SpendCoins(price))
        {
            return BuyResult.NotEnoughMoney;
        }

        bool addedSuccessfully = false;

        switch (item.type)
        {
            case ShopItemType.Item:
                addedSuccessfully = Inventory.Instance.AddItem(item.item);
                break;

            case ShopItemType.SpecialAttack:
                addedSuccessfully = Inventory.Instance.AddSpecialAttack(item.specialAttack);
                break;
        }

        if (!addedSuccessfully)
        {
            CurrencyManager.Instance.AddCoins(price);
            return BuyResult.InventoryFull;
        }

        return BuyResult.Success;
    }

    void ShowResultDialogue(string text)
    {
        if (DialogueManager.Instance == null) return;

        NPCDialogue.DialogueLine resultLine = new NPCDialogue.DialogueLine
        {
            dialogueText = text
        };

        DialogueManager.Instance.InjectDialogueLine(resultLine);
    }
}