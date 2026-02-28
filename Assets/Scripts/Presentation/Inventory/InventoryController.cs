using UnityEngine;

class InventoryController : MonoBehaviour
{
    [SerializeField] private InventoryViwe view;
    [SerializeField] private ItemDatabase itemData;

    private Inventory inventory;
    private ItemRules rules;

    private void Start()
    {
        inventory = new Inventory(20, 50f);

        rules = new ItemRules
        (
            itemData.IsStackable,
            itemData.GetMaxStack,
            itemData.GetWeight
        );
        view.Initialize(inventory.SlotCount);

        inventory.OnChanged += Refresh;
        Refresh();
    }
    void OnDestroy()
    {
        if (inventory != null)
            inventory.OnChanged -= Refresh;
    }
    #region Add Item
    public void AddItem(string itemId, int amount)
    {
        inventory.TryAdd(itemId, amount, rules);
    }
    #endregion
    #region Refresh 
    private void Refresh()
    {
        float currentWeight = inventory.GetCurrentWeight(rules.Weight);

        view.UpdateView(
            inventory.Slots,
            itemData.GetIcon,
            currentWeight,
            inventory.MaxWeight
        );
    }
    #endregion
    #region Save Inventory
    public void SaveInventory()
    {
        var data = inventory.ToSaveData();
        InventoryJsonStorage.Save("inventory.json", data);
    }
    public void LoadInventory()
    {
        var data = InventoryJsonStorage.Load("inventory.json");
        if (data == null) return;

        inventory.LoadFromSaveData(data,rules);
    }
    #endregion
}