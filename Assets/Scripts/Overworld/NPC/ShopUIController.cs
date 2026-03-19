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

            button.GetComponent<ShopItemButton>().Setup(currentItems[i]);
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
}