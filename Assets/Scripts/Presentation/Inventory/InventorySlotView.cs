using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class InventorySlotView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image highlight;     // seçili efekti için (Image)
    [SerializeField] private Button button;       // slot butonu

    private int index;
    private Action<int> onClicked;

    // Controller/View bu slotun indexini ve click callback'ini buradan bağlar
    public void Bind(int slotIndex, Action<int> clickCallback)
    {
        index = slotIndex;
        onClicked = clickCallback;

        // Double subscribe olmasın diye önce temizle
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClicked?.Invoke(index));
    }

    public void SetSelected(bool selected)
    {
        if (highlight != null)
            highlight.enabled = selected;
    }

    public void SetEmpty()
    {
        icon.enabled = false;
        amountText.text = "";
        SetSelected(false);
    }

    public void SetItem(Sprite sprite, int amount)
    {
        icon.enabled = true;
        icon.sprite = sprite;
        amountText.text = amount > 1 ? amount.ToString() : "";
    }
}