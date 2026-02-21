using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;

public class Inventory
{
    public int SlotCount { get; private set; }
    public float MaxWeight { get; private set; }

    private List<Slot> slots = new List<Slot>();

    public Inventory(int slotCount, float maxWeight)
    {
        SlotCount = slotCount;
        MaxWeight = maxWeight;

        for (int i = 0; i < slotCount; i++)
        {
            slots.Add(new Slot());
        }

    }
    public IReadOnlyList<Slot> Slots => slots;

    public float GetCurrentWeight(System.Func<string, float> weightResolver)
    {
        float total = 0f;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
            {
                float weightPerItem = weightResolver(slot.Stack.ItemId);
                total += weightPerItem * slot.Stack.Amount;
            }
        }
        return total;
    }
    public int AddITem(
        string itemId,
        int amount,
        System.Func<string, int> maxStackResolver,
        System.Func<string, bool> stackableResolver)
    {
        int remaining = amount;

        // önce mevcut sitekleri doldur.

        if (stackableResolver(itemId))
        {
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && slot.Stack.ItemId == itemId)
                {
                    int maxStack = maxStackResolver(itemId);
                    int space = maxStack - slot.Stack.Amount;

                    if (space > 0)
                    {
                        int addAmount = System.Math.Min(space, remaining);
                        slot.Stack.Amount += addAmount;
                        remaining -= addAmount;

                        if (remaining <= 0)
                        {
                            return 0;
                        }
                    }
                }
            }
        }
        //boş slotlara ekle
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                int maxStack = stackableResolver(itemId) ? maxStackResolver(itemId) : 1;
                int addAmount = System.Math.Min(maxStack, remaining);

                slot.Stack = new ItemStack(itemId, addAmount);
                remaining -= addAmount;

                if (remaining <= 0)
                {
                    return 0;
                }
            }
        }
        return remaining;
    }
}