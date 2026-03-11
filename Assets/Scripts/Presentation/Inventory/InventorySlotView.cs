using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;
    

    public void SetEmpty()
    {
        icon.enabled = false;
        amountText.text = "";
    }
    public void SetItem(Sprite sprite, int amount)
    {
        icon.enabled = true;
        icon.sprite = sprite;

        amountText.text = amount > 1 ? amount.ToString() : "";
    }
}