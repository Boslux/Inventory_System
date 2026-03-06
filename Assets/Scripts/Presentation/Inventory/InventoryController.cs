using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private InventoryView view;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private int slotCount;
    [SerializeField] private bool openOnStart = false;

    private Inventory inventory;
    private ItemRules rules;
    private Inputs input;
    private InputAction inventoryToggleAction;


    private int selectedIndex = -1;

    private void Awake()
    {
        input = new Inputs();
        BindInventoryToggleAction();
    }

    private void OnEnable()
    {
        if (input == null) return;

        input.Player.Enable();
        if (inventoryToggleAction != null)
            inventoryToggleAction.performed += OnInventoryTogglePerformed;
    }

    private void OnDisable()
    {
        if (inventoryToggleAction != null)
            inventoryToggleAction.performed -= OnInventoryTogglePerformed;

        if (input != null)
            input.Player.Disable();
    }

    private void Start()
    {
        inventory = new Inventory(slotCount, 50f);

        rules = new ItemRules(
            itemDatabase.IsStackable,
            itemDatabase.GetMaxStack,
            itemDatabase.GetWeight
        );

        // Event-driven refresh
        inventory.OnChanged += Refresh;

        // View init + click callback
        view.Initialize(inventory.SlotCount, OnSlotClicked);

        Refresh();
        SetInventoryVisible(openOnStart);
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnChanged -= Refresh;

        input?.Dispose();
    }

    private void BindInventoryToggleAction()
    {
        InputActionMap playerMap = input.asset.FindActionMap("Player", throwIfNotFound: true);
        inventoryToggleAction = playerMap.FindAction("Inventory", throwIfNotFound: false);

        // If the action is missing in the generated Inputs asset, create it at runtime.
        if (inventoryToggleAction == null)
            inventoryToggleAction = playerMap.AddAction("Inventory", InputActionType.Button, "<Keyboard>/i");
    }

    private void OnInventoryTogglePerformed(InputAction.CallbackContext context)
    {
        ToggleInventory();
    }

    public void ToggleInventory()
    {
        if (view == null) return;
        SetInventoryVisible(!view.gameObject.activeSelf);
    }

    public void OpenInventory()
    {
        SetInventoryVisible(true);
    }

    public void CloseInventory()
    {
        SetInventoryVisible(false);
    }

    private void SetInventoryVisible(bool isVisible)
    {
        if (view == null) return;
        view.gameObject.SetActive(isVisible);
    }

    private void OnSlotClicked(int index)
    {
        // 1) Hiç seçim yoksa seç
        if (selectedIndex < 0)
        {
            selectedIndex = index;
            view.SetSelectedIndex(selectedIndex);
            return;
        }

        // 2) Aynı slota tıklandıysa seçimi kaldır
        if (selectedIndex == index)
        {
            selectedIndex = -1;
            view.SetSelectedIndex(selectedIndex);
            return;
        }

        // 3) İkinci slota tıklandı: Move dene
        inventory.Move(selectedIndex, index, rules);

        // Seçimi sıfırla (Move başarısız olsa bile UX basit kalır)
        selectedIndex = -1;
        view.SetSelectedIndex(selectedIndex);
    }

    public void AddItem(string itemId, int amount)
    {
        inventory.TryAdd(itemId, amount, rules);
        // Refresh yok, event tetikler
    }

    public void SaveInventory()
    {
        var data = inventory.ToSaveData();
        InventoryJsonStorage.Save("inventory.json", data);
    }

    public void LoadInventory()
    {
        var data = InventoryJsonStorage.Load("inventory.json");
        if (data == null) return;

        inventory.LoadFromSaveData(data, rules);
        // Refresh yok, load içinden event tetikler
    }

    private void Refresh()
    {
        float currentWeight = inventory.GetCurrentWeight(rules.Weight);

        view.UpdateView(
            inventory.Slots,
            itemDatabase.GetIcon,
            currentWeight,
            inventory.MaxWeight
        );

        // Refresh sırasında selection UI kaybolmasın diye tekrar uygula
        view.SetSelectedIndex(selectedIndex);
    }
    public int TryAddItem(string itemId, int amount)
    {
        return inventory.TryAdd(itemId, amount, rules);
    }
}
