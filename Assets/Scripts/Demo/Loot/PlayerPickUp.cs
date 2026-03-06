using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [SerializeField] private InventoryController inventoryController;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PickupItem pickup)) return;

        // Envantere eklemeyi dene (leftover = eklenemeyen)
        int leftover = inventoryController.TryAddItem(pickup.itemId, pickup.amount);

        if (leftover <= 0)
        {
            Destroy(other.gameObject);
        }
        else
        {
            // Eklenemeyen miktar varsa pickup world'de kalır
            pickup.amount = leftover;
        }
    }
}