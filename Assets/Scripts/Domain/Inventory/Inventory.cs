using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
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

    #region Current Weight
    public float GetCurrentWeight(Func<string, float> weightResolver)
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
    #endregion

    private Slot FindFirstNotFullStackSlot(string itemId, int maxStack)
    {
        foreach (var s in slots)
        {
            if (!s.IsEmpty && s.Stack.ItemId == itemId && s.Stack.Amount < maxStack)
            {
                return s;
            }
        }
        return null;
    }
    private Slot FindFirstEmptySlot()
    {
        foreach (var s in slots)
        {
            if (s.IsEmpty) return s;
        }
        return null;
    }

    public int CanAdd(string itemId, int amount, ItemRules rules)
    {
        if (amount <= 0) return 0;

        bool stackable = rules.IsStackable(itemId);
        int maxStack = stackable ? Math.Max(1, rules.MaxStack(itemId)) : 1;

        int capacity = 0;

        // mevcut stacklerde boş yer
        if (stackable)
        {
            foreach (var s in slots)
            {
                if (!s.IsEmpty && s.Stack.ItemId == itemId)
                {
                    capacity += (maxStack - s.Stack.Amount);
                }
            }
        }
        foreach (var s in slots)
        {
            if (s.IsEmpty)
            {
                capacity += maxStack;
            }
        }
        // istenenden fazla kapasite varsa amount kadar eklenebilir
        return Math.Min(amount, capacity);
    }
    public int TryAdd(string itemId, int amount, ItemRules rules)
    {
        if (amount <= 0) return 0;

        bool stackable = rules.IsStackable(itemId);
        int maxStack = stackable ? Math.Max(1, rules.MaxStack(itemId)) : 1;

        int remaining = amount;

        // önce mevcut stack'leri doldur
        if (stackable)
        {
            while (remaining > 0)
            {
                var target = FindFirstNotFullStackSlot(itemId, maxStack);
                if (target == null) break;

                int space = maxStack - target.Stack.Amount;
                int add = Math.Min(space, remaining);

                target.Stack.Amount += add;
                remaining -= add;
            }
        }
        while (remaining > 0)
        {
            var empty = FindFirstEmptySlot();
            if (empty == null) break;

            int add = Math.Min(maxStack, remaining);
            empty.Stack = new ItemStack(itemId, add);
            remaining -= add;

            // stacklenmeyen ise zaten maxStack = 1, loop slot slot gider.
        }
        return remaining;
    }
    public bool TryRemove(string itemId, int amount)
    {
        if (amount <= 0) return true;

        // 1) Ön kontrol (atomic davranış)
        if (CountItem(itemId) < amount)
            return false;

        // 2) Gerçek çıkarma
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
                return true;
        }

        // Buraya teorik olarak düşmemeli (çünkü ön kontrolden geçti)
        return true;
    }
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
}