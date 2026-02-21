[System.Serializable]
public class ItemStack
{
    public string ItemId;
    public int Amount;

    public ItemStack(string itemID, int amount)
    {
        ItemId = itemID;
        Amount = amount;
    }
}