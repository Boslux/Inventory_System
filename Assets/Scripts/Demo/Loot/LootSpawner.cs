using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    [SerializeField] private DropTable dropTable;
    [SerializeField] private PickupItem pickupPrefab;

    public void SpawnDrops(Vector3 position)
    {
        if (dropTable == null || pickupPrefab == null) return;

        foreach (var entry in dropTable.drops)
        {
            if (Random.value > entry.chance)
                continue;

            int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);

            // 3D'de küçük bir random offset
            Vector3 offset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                0.2f,
                Random.Range(-0.5f, 0.5f)
            );

            var pickup = Instantiate(pickupPrefab, position + offset, Quaternion.identity);
            pickup.Initialize(entry.itemId, amount);
        }
    }
}