using System.IO;
using UnityEngine;

public static class InventoryJsonStorage
{
    public static void Save(string fileName, InventorySaveData data)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(path, json);
    }

    public static InventorySaveData Load(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<InventorySaveData>(json);
    }
}