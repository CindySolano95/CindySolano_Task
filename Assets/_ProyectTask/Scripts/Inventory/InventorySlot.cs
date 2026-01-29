using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image slotBackground; // Your Slot Image (optional)

    public int SlotIndex { get; private set; } = -1;

    public InventoryItemView CurrentItem { get; private set; }
    public bool IsEmpty => CurrentItem == null;

    public void SetIndex(int index) => SlotIndex = index;

    public void Attach(InventoryItemView item)
    {
        CurrentItem = item;

        if (item != null)
        {
            item.transform.SetParent(transform, worldPositionStays: false);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
        }

        SetFillCenter(item != null);
    }

    public InventoryItemView Detach()
    {
        var item = CurrentItem;
        CurrentItem = null;
        SetFillCenter(false);
        return item;
    }

    private void SetFillCenter(bool value)
    {
        if (slotBackground != null)
            slotBackground.fillCenter = value;
    }
}
