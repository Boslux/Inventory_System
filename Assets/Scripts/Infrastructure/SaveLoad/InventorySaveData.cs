
using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public int slotCount;
    public float maxWeight;
    public List<SlotSaveData> slots = new List<SlotSaveData>();
}
[Serializable]
public class SlotSaveData
{
    public string itemId; // boşsa null veya ""
    public int amount;    // boşsa 0
}