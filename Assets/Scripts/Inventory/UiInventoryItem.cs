using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

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

    public void Delete()
    {
        this.ItemData = null;
        Destroy(this.gameObject);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        var inventoryPanel = UiManager.IN.InventoryPanel;
        if (inventoryPanel.IsShowing && inventoryPanel.CurrentCategory != EShopCategory.All)
            return;

        base.OnBeginDrag(eventData);
    }

    protected override bool DoOnBeginDrag()
    {
        var inventoryPanel = UiManager.IN.InventoryPanel;
        if (inventoryPanel.IsShowing && inventoryPanel.CurrentCategory != EShopCategory.All)
            return false;

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

                if(!string.IsNullOrEmpty(this.ItemData.DecorationData.PrefabName))
                {
                    // Spawn world item at current position
                    var worldItem = DecorationManager.IN.SpawnItemInWorld(this.ItemData, this.transform.position);
                    
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

                            if (this.ItemData.Quantity > 1)
                            {
                                var newItemData = InventoryItemData.Copy(this.ItemData);
                                newItemData.Quantity = 1;
                                this.ItemData.Quantity -= 1;
                                SaveManager.Data.WorldItems.Add(newItemData);
                            }
                            else
                            {
                                SaveManager.Data.WorldItems.Add(this.ItemData);

                                var origCell = this.originalParent.GetComponentInParent<UiInventoryCell>();
                                SaveManager.Data.AllInventoryItems[origCell.CellIndex] = null;
                            }
                            
                            // Destroy the inventory item
                            Delete();
                            
                            return false;
                        }
                    }
                }

                // Fallback: just destroy if spawn failed
                this.isDragging = false;
                Delete();
                return false;
            }
        }
        return true;
    }

    protected override bool TryToParentToDropTarget()
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
                        this.targetRectTransform.SetAsLastSibling();
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
    
    public override void OnEndDrag(PointerEventData eventData)
    {
        var inventoryPanel = UiManager.IN.InventoryPanel;
        if (inventoryPanel.IsShowing && inventoryPanel.CurrentCategory != EShopCategory.All)
            return;

        base.OnEndDrag(eventData);
    }
    
    protected override bool DoOnEndDrag()
    {
        UiInventoryPanel.OnDragOutOfInventoryZoneActiveChanged?.Invoke(false);

        var origCell = this.originalParent.GetComponentInParent<UiInventoryCell>();

        var cell = GetComponentInParent<UiInventoryCell>();
        if (cell != null)
        {
            var item = cell.Item;
            if (item == null)
            {
                // Cell is empty, add item to cell
                cell.AddItem(this, this.ItemData);

                if(cell != origCell)
                {
                    origCell.ClearItem(true);
                    SaveManager.Data.AllInventoryItems[origCell.CellIndex] = null;
                }
                
                SaveManager.Data.AllInventoryItems[cell.CellIndex] = this.ItemData;
            }
            else
            {
                if (item == this)
                {
                    // Dropped back on original cell, do nothing
                    return true;
                }
                else if (item.ItemData.IconSpriteName != this.ItemData.IconSpriteName)
                {
                    //swap items
                    cell.SwapItems(origCell, this);
                }
                else
                {
                    // //same item type, combine stacks if possible
                    cell.MergeItems(origCell, this);
                }
            }
            
            cell.SetSelected(true);
        }
        return true;
    }
}