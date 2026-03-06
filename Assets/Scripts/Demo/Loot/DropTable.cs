using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/DropTable")]
public class DropTable : ScriptableObject
{
    public List<DropEntry> drops = new List<DropEntry>();
}