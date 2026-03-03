[System.Serializable]
public class InventoryItem
{
    public Item itemData;
    public int quantity;

    public InventoryItem(Item item)
    {
        itemData = item;
    }
}