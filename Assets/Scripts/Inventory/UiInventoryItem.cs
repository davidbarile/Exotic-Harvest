using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiInventoryItem : UiDraggable
{
    [Header("Inventory Item UI Elements")]

    [SerializeField] private Image itemIcon;
    [SerializeField] private Image shadow;
    [SerializeField] private TextMeshProUGUI itemQuantityText;

    public InventoryItemData ItemData { get; private set; }

    public void Configure(InventoryItemData inItemData)
    {
        this.ItemData = inItemData;

        if (itemIcon != null)
        {
            var sprite = SpriteManager.GetSprite(inItemData.IconSpriteName);
            itemIcon.sprite = sprite;
            shadow.sprite = sprite;
        }

        if (itemQuantityText != null)
        {
            itemQuantityText.text = inItemData.Quantity > 1 ? inItemData.Quantity.ToString() : string.Empty;
        }
    }

    protected override bool DoOnBeginDrag()
    {
        var cell = GetComponentInParent<UiInventoryCell>();
        if (cell != null)
        {
            cell.ClearItem(false);
            cell.SetSelected(false);
        }

        return true;
    }

    protected override bool DoOnDrag()
    {
        return true;
    }
    
    protected override bool DoOnEndDrag()
    {
        var cell = GetComponentInParent<UiInventoryCell>();
        if (cell != null)
        {
            var item = cell.Item;
            if (item == null)
            {
                // Cell is empty, add item to cell
                cell.AddItem(this, this.ItemData);
            }
            else
            {
                var originalCell = this.originalParent.GetComponentInParent<UiInventoryCell>();
                
                if (item == this)
                {
                    // Dropped back on original cell, do nothing
                    return true;
                }
                else if (item.ItemData.IconSpriteName != this.ItemData.IconSpriteName)
                {
                    //swap items
                    cell.SwapItems(originalCell, this);
                }
                else
                {
                    // //same item type, combine stacks if possible
                    cell.MergeItems(originalCell, this);
                }
            }
            
            cell.SetSelected(true);
        }
        return true;
    }
}