[System.Serializable]
public class InventorySpecialAttack
{
    public SpecialAttack attackData;
    public int quantity;

    public InventorySpecialAttack(SpecialAttack attack, int amount = 1)
    {
        attackData = attack;
        quantity = amount;
    }
}