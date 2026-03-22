using System;
using System.Collections.Generic;
using System.Data;

public class Inventory
{
    public event Action OnChanged;

    public int SlotCount { get; private set; }
    public float MaxWeight { get; private set; }

    // Internal storage (mutable). Exposed as IReadOnlyList to callers.
    private readonly List<Slot> slots = new List<Slot>();

    public Inventory(int slotCount, float maxWeight)
    {
        SlotCount = slotCount;
        MaxWeight = maxWeight;

        // Initialize all slots up front (fixed slot count).
        for (int i = 0; i < slotCount; i++)
        {
            slots.Add(new Slot());
        }
    }

    // Expose as read-only so external code cannot Add/Remove slots.
    public IReadOnlyList<Slot> Slots => slots;

    #region Current Weight

    /// <summary>
    /// Calculates the inventory's current total weight.
    /// Inventory does not know ItemData, so it resolves itemId -> weight via weightResolver.
    /// </summary>
    public float GetCurrentWeight(Func<string, float> weightResolver)
    {
        float total = 0f;

        foreach (var s in slots)
        {
            if (s.IsEmpty) continue;

            float weightPerItem = weightResolver(s.Stack.ItemId);
            total += weightPerItem * s.Stack.Amount;
        }

        return total;
    }

    #endregion

    #region Slot Find Helpers

    /// <summary>
    /// Finds the first slot with the same itemId that is not at max stack.
    /// </summary>
    private Slot FindFirstNotFullStackSlot(string itemId, int maxStack)
    {
        foreach (var s in slots)
        {
            if (!s.IsEmpty && s.Stack.ItemId == itemId && s.Stack.Amount < maxStack)
                return s;
        }
        return null;
    }

    /// <summary>
    /// Finds the first empty slot.
    /// </summary>
    private Slot FindFirstEmptySlot()
    {
        foreach (var s in slots)
        {
            if (s.IsEmpty) return s;
        }
        return null;
    }

    #endregion

    #region Can Add (Slot/Stack Capacity)

    /// <summary>
    /// Based on slot + stack rules (ignoring weight), how many of this item can be added?
    /// Returns a value capped by amount.
    /// </summary>
    public int CanAdd(string itemId, int amount, ItemRules rules)
    {
        if (amount <= 0) return 0;

        bool stackable = rules.IsStackable(itemId);
        int maxStack = stackable ? Math.Max(1, rules.MaxStack(itemId)) : 1;

        int capacity = 0;

        // 1) Available space in existing stacks
        if (stackable)
        {
            foreach (var s in slots)
            {
                if (!s.IsEmpty && s.Stack.ItemId == itemId)
                    capacity += (maxStack - s.Stack.Amount);
            }
        }

        // 2) Capacity from empty slots
        foreach (var s in slots)
        {
            if (s.IsEmpty)
                capacity += maxStack;
        }

        // If capacity exceeds the request, cap it at amount.
        return Math.Min(amount, capacity);
    }

    #endregion

    #region Weight Limit

    /// <summary>
    /// Based on weight limit, how many of this item can be added at most?
    /// Returns a value capped by amount.
    /// </summary>
    public int CanAddByWeight(string itemId, int amount, ItemRules rules)
    {
        float currentWeight = GetCurrentWeight(rules.Weight);
        float itemWeight = rules.Weight(itemId);

        // Treat items with zero or negative weight as effectively unlimited.
        if (itemWeight <= 0f)
            return amount;

        float remainingCapacity = MaxWeight - currentWeight;

        if (remainingCapacity <= 0f)
            return 0;

        // Remaining capacity / item weight => maximum addable amount
        int maxByWeight = (int)(remainingCapacity / itemWeight);
        return Math.Min(amount, maxByWeight);
    }

    #endregion

    #region Try Add & Remove

    /// <summary>
    /// Tries to add an item.
    /// Returned int: amount that could not be added (leftover).
    /// 0 means the full amount was added.
    /// </summary>
    public int TryAdd(string itemId, int amount, ItemRules rules)
    {
        if (amount <= 0) return 0;

        // 1) Calculate slot/stack capacity and weight capacity separately
        int maxByWeight = CanAddByWeight(itemId, amount, rules);
        int maxBySlots = CanAdd(itemId, amount, rules);

        // 2) Actual addable amount = minimum of both limits
        int allowed = Math.Min(maxByWeight, maxBySlots);

        // If nothing can be added, the full requested amount remains as leftover.
        if (allowed <= 0) return amount;

        bool stackable = rules.IsStackable(itemId);
        int maxStack = stackable ? Math.Max(1, rules.MaxStack(itemId)) : 1;

        int toAdd = allowed;
        int leftover = amount - allowed;

        // 3) Fill existing stacks first
        if (stackable)
        {
            while (toAdd > 0)
            {
                var target = FindFirstNotFullStackSlot(itemId, maxStack);
                if (target == null) break;

                int space = maxStack - target.Stack.Amount;
                int add = Math.Min(space, toAdd);

                target.Stack.Amount += add;
                toAdd -= add;
            }
        }

        // 4) Then distribute into empty slots
        while (toAdd > 0)
        {
            var empty = FindFirstEmptySlot();
            if (empty == null) break; // Theoretically CanAdd should prevent this, but keep it safe.

            int add = Math.Min(maxStack, toAdd);
            empty.Stack = new ItemStack(itemId, add);
            toAdd -= add;
        }

        // Return the amount that could not be added.
        RaiseChanged();
        return leftover;
    }

    /// <summary>
    /// Atomic remove: if there are not enough items, change nothing and return false.
    /// </summary>
    public bool TryRemove(string itemId, int amount)
    {
        if (amount <= 0) return true;

        // 1) Pre-check: is the total amount sufficient?
        if (CountItem(itemId) < amount)
            return false;

        // 2) Actual removal
        int remaining = amount;

        foreach (var s in slots)
        {
            if (s.IsEmpty) continue;
            if (s.Stack.ItemId != itemId) continue;

            int take = Math.Min(s.Stack.Amount, remaining);
            s.Stack.Amount -= take;
            remaining -= take;

            if (s.Stack.Amount <= 0)
                s.Clear();

            if (remaining <= 0)
            {
                RaiseChanged();
                return true;
            }
        }

        // This path should normally never be reached after the pre-check.
        return true;
    }

    #endregion

    #region Count

    /// <summary>
    /// Returns the total amount of the given itemId in the inventory.
    /// </summary>
    private int CountItem(string itemId)
    {
        int total = 0;

        foreach (var s in slots)
        {
            if (s.IsEmpty) continue;
            if (s.Stack.ItemId != itemId) continue;

            total += s.Stack.Amount;
        }

        return total;
    }
    #endregion

    #region SaveData
    public InventorySaveData ToSaveData()
    {
        var data = new InventorySaveData
        {
            slotCount = SlotCount,
            maxWeight = MaxWeight,
            slots = new List<SlotSaveData>(SlotCount)
        };

        foreach (var s in slots)
        {
            if (s.IsEmpty)
            {
                data.slots.Add(new SlotSaveData { itemId = "", amount = 0 });
            }
            else
            {
                data.slots.Add(new SlotSaveData { itemId = s.Stack.ItemId, amount = s.Stack.Amount });
            }
        }
        return data;
    }

    public void LoadFromSaveData(InventorySaveData data, ItemRules rules)
    {
        // Safety: exit if data is missing.
        if (data == null || data.slots == null) return;

        int count = Math.Min(SlotCount, data.slots.Count);

        // Clear existing data first
        for (int i = 0; i < SlotCount; i++)
            slots[i].Clear();

        // Then populate slots
        for (int i = 0; i < count; i++)
        {
            var sd = data.slots[i];
            if (string.IsNullOrEmpty(sd.itemId) || sd.amount <= 0)
                continue;

            bool stackable = rules.IsStackable(sd.itemId);
            int maxStack = stackable ? Math.Max(1, rules.MaxStack(sd.itemId)) : 1;

            int clampedAmount = Math.Max(sd.amount, maxStack);
            slots[i].Stack = new ItemStack(sd.itemId, clampedAmount);
        }

        // Trigger event so UI updates automatically.
        OnChanged?.Invoke();
    }
    #endregion

    private void RaiseChanged()
    {
        OnChanged?.Invoke();
    }
}
