using System;
using System.Collections.Generic;

public class Inventory
{
    public int SlotCount { get; private set; }
    public float MaxWeight { get; private set; }

    // Internal storage (mutable). Dışarıya IReadOnlyList ile açıyoruz.
    private readonly List<Slot> slots = new List<Slot>();

    public Inventory(int slotCount, float maxWeight)
    {
        SlotCount = slotCount;
        MaxWeight = maxWeight;

        // Slotları baştan oluşturuyoruz (sabit slot sayısı)
        for (int i = 0; i < slotCount; i++)
        {
            slots.Add(new Slot());
        }
    }

    // Dışarıdan slots listesine Add/Remove yapılmasın diye readonly expose ediyoruz.
    public IReadOnlyList<Slot> Slots => slots;

    #region Current Weight

    /// <summary>
    /// Envanterin mevcut toplam ağırlığını hesaplar.
    /// Inventory ItemData bilmediği için weightResolver ile itemId -> weight çözer.
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
    /// Aynı itemId'ye sahip ve maxStack dolmamış ilk slotu bulur.
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
    /// İlk boş slotu bulur.
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
    /// Slot + Stack kurallarına göre (weight'i hesaba katmadan) bu itemdan en fazla kaç adet eklenebilir?
    /// amount ile sınırlandırılmış olarak döner.
    /// </summary>
    public int CanAdd(string itemId, int amount, ItemRules rules)
    {
        if (amount <= 0) return 0;

        bool stackable = rules.IsStackable(itemId);
        int maxStack = stackable ? Math.Max(1, rules.MaxStack(itemId)) : 1;

        int capacity = 0;

        // 1) Mevcut stack'lerde boş yer
        if (stackable)
        {
            foreach (var s in slots)
            {
                if (!s.IsEmpty && s.Stack.ItemId == itemId)
                    capacity += (maxStack - s.Stack.Amount);
            }
        }

        // 2) Boş slot kapasitesi
        foreach (var s in slots)
        {
            if (s.IsEmpty)
                capacity += maxStack;
        }

        // İstenen miktardan fazlaysa amount kadar eklenebilir.
        return Math.Min(amount, capacity);
    }

    #endregion

    #region Weight Limit

    /// <summary>
    /// Ağırlık limitine göre bu itemdan en fazla kaç adet eklenebilir?
    /// amount ile sınırlandırılmış olarak döner.
    /// </summary>
    public int CanAddByWeight(string itemId, int amount, ItemRules rules)
    {
        float currentWeight = GetCurrentWeight(rules.Weight);
        float itemWeight = rules.Weight(itemId);

        // Ağırlığı 0 veya negatif olan itemları "sınırsız" kabul ediyoruz.
        if (itemWeight <= 0f)
            return amount;

        float remainingCapacity = MaxWeight - currentWeight;

        if (remainingCapacity <= 0f)
            return 0;

        // Kalan kapasite / item ağırlığı => eklenebilecek maksimum adet
        int maxByWeight = (int)(remainingCapacity / itemWeight);
        return Math.Min(amount, maxByWeight);
    }

    #endregion

    #region Try Add & Remove

    /// <summary>
    /// Item eklemeyi dener.
    /// Dönen int: Eklenemeyen (geriye kalan) miktar.
    /// 0 ise tamamı eklenmiştir.
    /// </summary>
    public int TryAdd(string itemId, int amount, ItemRules rules)
    {
        if (amount <= 0) return 0;

        // 1) Slot/Stack kapasitesi ile weight kapasitesini ayrı ayrı hesapla
        int maxByWeight = CanAddByWeight(itemId, amount, rules);
        int maxBySlots = CanAdd(itemId, amount, rules);

        // 2) Gerçek eklenebilir miktar = iki limitin minimumu
        int allowed = Math.Min(maxByWeight, maxBySlots);

        // Hiç eklenemiyorsa, istenen miktarın tamamı "kalan"dır.
        if (allowed <= 0) return amount;

        bool stackable = rules.IsStackable(itemId);
        int maxStack = stackable ? Math.Max(1, rules.MaxStack(itemId)) : 1;

        // ✅ ÖNEMLİ DÜZELTME:
        // toAdd: gerçekten ekleyeceğimiz adet
        // leftover: eklenemeyen (return edeceğimiz) adet
        int toAdd = allowed;
        int leftover = amount - allowed;

        // 3) Önce mevcut stack'leri doldur
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

        // 4) Sonra boş slotlara dağıt
        while (toAdd > 0)
        {
            var empty = FindFirstEmptySlot();
            if (empty == null) break; // Teorik olarak CanAdd sayesinde buraya düşmemeli, ama güvenli.

            int add = Math.Min(maxStack, toAdd);
            empty.Stack = new ItemStack(itemId, add);
            toAdd -= add;
        }

        // Eklenemeyen miktarı döndür.
        return leftover;
    }

    /// <summary>
    /// Atomic remove: Yeterli item yoksa hiç dokunmaz ve false döner.
    /// </summary>
    public bool TryRemove(string itemId, int amount)
    {
        if (amount <= 0) return true;

        // 1) Ön kontrol: toplam yeterli mi?
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

        // Ön kontrol geçtiği için buraya normalde düşmemeli.
        return true;
    }

    #endregion

    #region Count

    /// <summary>
    /// Envanterde belirli itemId'den toplam kaç adet var?
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
}