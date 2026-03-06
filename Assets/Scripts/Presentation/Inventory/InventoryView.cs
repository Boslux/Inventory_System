using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private InventorySlotView slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private TextMeshProUGUI weightText;

    private readonly List<InventorySlotView> slotViews = new List<InventorySlotView>();
    private Action<int> onSlotClicked;

    public void Initialize(int slotCount, Action<int> slotClickedCallback)
    {
        onSlotClicked = slotClickedCallback;

        for (int i = 0; i < slotCount; i++)
        {
            var slot = Instantiate(slotPrefab, slotParent);
            slot.Bind(i, HandleSlotClicked);
            slotViews.Add(slot);
        }
    }

    private void HandleSlotClicked(int index)
    {
        onSlotClicked?.Invoke(index);
    }

    public void SetSelectedIndex(int selectedIndex)
    {
        for (int i = 0; i < slotViews.Count; i++)
            slotViews[i].SetSelected(i == selectedIndex);
    }

    public void UpdateView(
        IReadOnlyList<Slot> slots,
        Func<string, Sprite> iconResolver,
        float currentWeight,
        float maxWeight)
    {
        for (int i = 0; i < slotViews.Count; i++)
        {
            var slotView = slotViews[i];
            var slot = slots[i];

            if (slot.IsEmpty)
            {
                slotView.SetEmpty();
            }
            else
            {
                var sprite = iconResolver(slot.Stack.ItemId);
                slotView.SetItem(sprite, slot.Stack.Amount);
            }
        }

        // Basit format. İstersen 0.0 formatlayabiliriz.
        weightText.text = $"{currentWeight} / {maxWeight}";
    }
}