using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> items;

    private Dictionary<string, ItemData> lookup;

    private void OnEnable()
    {
        lookup = new Dictionary<string, ItemData>();

        foreach (var item in items)
        {
            lookup[item.Id] = item;
        }
    }

    public bool IsStackable(string id)
        => lookup[id].Stackable;

    public int GetMaxStack(string id)
        => lookup[id].MaxStack;

    public float GetWeight(string id)
        => lookup[id].Weight;

    public Sprite GetIcon(string id)
        => lookup[id].Icon;
}