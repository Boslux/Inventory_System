using UnityEngine;

class InventoryController : MonoBehaviour
{
    private Inventory inventory;
    void Start()
    {
        inventory=new Inventory(20,50f);
        Debug.Log(inventory.SlotCount);
    }
}