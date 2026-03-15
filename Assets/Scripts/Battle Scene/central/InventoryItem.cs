[System.Serializable]
public class InventoryItem
{
    public Item itemData;
    public int quantity;

    public InventoryItem(Item item, int amount = 1)
    {
        itemData = item;
        quantity = amount;
    }
}