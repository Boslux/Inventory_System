using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryViwe : MonoBehaviour
{
    [SerializeField] private InventorySlotView slotPref;
    [SerializeField] private Transform slotParent;
    [SerializeField] private TextMeshProUGUI weightText;

    private List<InventorySlotView> slotViews = new List<InventorySlotView>();

    public void Initialize(int slotCount)
    {
        for (int i = 0; i < slotCount; i++)
        {
            var slot = Instantiate(slotPref, slotParent);
            slotViews.Add(slot);
        }
    }
    public void UpdateView(
        IReadOnlyList<Slot> slots,
        Func<string, Sprite> iconResolver,
        float currentWeight,
        float maxWeight
        )
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
        weightText.text = $"{currentWeight}/ {maxWeight}";
    }
}
