[System.Serializable]
public class InventoryItem
{
    public Item itemData;

    public InventoryItem(Item item)
    {
        itemData = item;
    }
}