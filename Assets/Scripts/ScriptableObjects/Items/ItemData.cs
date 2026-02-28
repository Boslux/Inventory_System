using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public string Id;
    public string DisplayName;
    public Sprite Icon;
    public bool Stackable;
    public int MaxStack = 1;
    public float Weight = 1f;
}