using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]

public class PickupItem : MonoBehaviour
{
    public string itemId;
    public int amount = 1;

    public void Initialize(string id, int amt)
    {
        itemId = id;
        amount = Mathf.Max(1, amt);
    }
}