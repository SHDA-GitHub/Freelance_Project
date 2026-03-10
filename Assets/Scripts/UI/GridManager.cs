using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform gridParent;
    public int maxItems = 8;

    private int currentItemCount = 0;

    public void AddItem()
    {
        if (currentItemCount < maxItems)
        {
            Instantiate(itemPrefab, gridParent);
            currentItemCount++;
        }
        else
        {
            Debug.Log("Maximum item limit reached!");
        }
    }

    public void RemoveItem(GameObject item)
    {
        Destroy(item);
        currentItemCount--;
    }
}