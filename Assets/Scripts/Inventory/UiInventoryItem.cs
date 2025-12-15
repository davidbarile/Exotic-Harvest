using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

        if (this.itemIcon != null)
        {
            var sprite = SpriteManager.GetSprite(inItemData.IconSpriteName);
            this.itemIcon.sprite = sprite;
            this.shadow.sprite = sprite;
        }

        if (this.itemQuantityText != null)
        {
            this.itemQuantityText.text = inItemData.Quantity > 1 ? inItemData.Quantity.ToString() : string.Empty;
        }
    }

    protected override bool DoOnBeginDrag()
    {
        UiInventoryPanel.OnDragOutOfInventoryZoneActiveChanged?.Invoke(true);

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
        foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
        {
            if (possibleTarget.TryGetComponent<UiDragTarget>(out var dragTarget) && dragTarget.IsDragOutOfInventoryZone && this.ItemData.CanDragToWorld)
            {
                UiManager.IN.InventoryPanel.Hide();

                if(!string.IsNullOrEmpty(this.ItemData.WorldPrefabName))
                {
                    // Spawn world item at current position
                    var worldItem = InventoryManager.IN.SpawnItemInWorldWithReturn(this.ItemData, this.transform.position);
                    
                    if (worldItem != null)
                    {
                        // Transfer drag to the newly spawned world item
                        var worldItemRect = worldItem.GetComponent<RectTransform>();
                        if (worldItemRect != null)
                        {
                            UiInventoryPanel.OnDragOutOfInventoryZoneActiveChanged?.Invoke(false);

                            // Initialize the world item with drag state
                            worldItem.InitializeFromDrag(this.ItemData, DragManager.IN.DragOffset);
                            
                            // Swap the dragged object to the new world item
                            DragManager.IN.SwapDraggedObject(worldItemRect);

                            // Mark as not dragging so OnEndDrag doesn't process
                            this.isDragging = false;

                            this.ItemData.DecorationData.IsInInventory = false;
                            
                            
                            // Destroy the inventory item
                            Destroy(this.gameObject);
                            return false;
                        }
                    }
                }

                // Fallback: just destroy if spawn failed
                this.isDragging = false;
                Destroy(this.gameObject);
                return false;
            }
        }
        return true;
    }

    protected override bool TryReparentToDropTarget()
    {
        if (this.shouldDetectDropTargets)
        {
            foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
            {
                if (possibleTarget.TryGetComponent<UiDragTarget>(out var dragTarget))
                {
                    var cell = possibleTarget.GetComponentInParent<UiInventoryCell>();
                    if (cell != null)
                    {
                        dragTarget.SetAsParent(this.targetRectTransform);
                        dragTarget.SetHighlight(false);
                        return false;//found drag target, reparent and exit
                    }
                    else
                    {
                        if (!this.ItemData.CanDragToWorld)
                        {
                            return true;//bounce back
                        }
                    }
                }
            }
        }

        return true;
    }
    
    protected override void DoSnapBack()
    {
        if (this.originalParent != null)
        {
            if (this.onlyDragToTargets)
            {
                //snap back to original position
                transform.DOMove(this.originalWorldPosition, 0.2f).OnComplete(() =>
                {
                    var originalParent = this.shouldReturnToOriginalParent ? this.originalParent : DragManager.IN.DefaultParent;
                    this.targetRectTransform.SetParent(originalParent, true);

                    if (this.shouldReturnToOriginalParent)
                        this.targetRectTransform.SetSiblingIndex(this.originalSiblingIndex);

                    var origCell = this.originalParent.GetComponentInParent<UiInventoryCell>();
                    if (origCell != null)
                    {
                        //origCell.SetSelected(true);
                        origCell.AddItem(this, this.ItemData);
                    }
                });
            }
        }
    }
    
    protected override bool DoOnEndDrag()
    {
        UiInventoryPanel.OnDragOutOfInventoryZoneActiveChanged?.Invoke(false);
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