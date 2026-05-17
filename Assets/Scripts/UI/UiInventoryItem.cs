using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using static GlobalEnums;

public class UiInventoryItem : Draggable
{
    [Header("Inventory Item UI Elements")]

    [SerializeField] private Image itemIcon;
    [SerializeField] private Image shadow;
    [SerializeField] private TextMeshProUGUI itemQuantityText;

    public InventoryItemData ItemData { get; private set; }

    private Vector2 originalIconSize = Vector2.zero;

    public void Configure(InventoryItemData inItemData)
    {
        this.ItemData = inItemData;

        this.transform.localScale = Vector3.one * inItemData.Scale;

        this.highlightValidTargetsWhenDragged = this.ItemData.DecorationData.HighlightValidTargetsWhenDragged;

        if (this.itemIcon)
        {
            if (this.originalIconSize == Vector2.zero)
                this.originalIconSize = this.itemIcon.rectTransform.sizeDelta;

            this.itemIcon.rectTransform.sizeDelta = this.originalIconSize;
            
            var sprite = SpriteManager.GetSprite(inItemData.IconSpriteName);
            SpriteManager.SetImageSprite(this.itemIcon, sprite);
            this.itemIcon.color = inItemData.IconColor;

            SpriteManager.SetImageSprite(this.shadow, sprite);
        }

        if (this.itemQuantityText)
            this.itemQuantityText.text = inItemData.MaxStack > 1 ? $"{inItemData.Quantity}<size=80%>/{inItemData.MaxStack}</size>" : string.Empty;
    }

    public void Delete()
    {
        this.ItemData = null;
        Destroy(this.gameObject);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        //block dragging when not in "All" category of InventoryPanel
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

    public override void OnDragUpdate()
    {
        foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
        {
            if (possibleTarget.TryGetComponent<DragTarget>(out var dragTarget) && dragTarget.IsDragOutOfInventoryZone)
            {
                UiManager.IN.InventoryPanel.Hide();

                if(!string.IsNullOrEmpty(this.ItemData.DecorationData.PrefabName))
                {
                    var wData = this.ItemData.DecorationData.WorldSaveData;
                        
                    // Spawn world item at current position
                    var worldItem = DecorationManager.IN.SpawnItemInWorld(this.ItemData, this.transform.position, wData.Scale, wData.Rotation);
                    
                    if(worldItem.TryGetComponent<RectTransform>(out var worldItemRect))
                    {
                        UiInventoryPanel.OnDragOutOfInventoryZoneActiveChanged?.Invoke(false);

                        // Initialize the world item with drag state
                        worldItem.ConfigureFromDrag(this.ItemData, DragManager.IN.OffsetFromCursor, this.originalParent, this.originalSiblingIndex);

                        // Swap the dragged object to the new world item
                        DragManager.IN.SwapDraggedObject(worldItemRect);

                        Debug.Log($"Spawned world item [{worldItem.name}] from inventory item [{name}] at position {worldItem.transform.position}", worldItem);

                        // Mark as not dragging so OnDrag doesn't process
                        this.isDragging = false;

                        if (this.ItemData.Quantity > 1)
                        {
                            var newItemData = this.ItemData; //InventoryItemData.Copy(this.ItemData);
                            newItemData.Quantity = 1;
                            this.ItemData.Quantity -= 1;
                            SaveManager.Data.WorldItems.Add(newItemData);
                        }
                        else
                        {
                            SaveManager.Data.WorldItems.Add(this.ItemData);

                            var origCell = this.originalParent.GetComponentInParent<UiInventoryCell>();
                            SaveManager.Data.InventoryItems[origCell.CellIndex] = null;
                        }

                        // Destroy the inventory item
                        Delete();
                        return;
                    }
                }

                // Fallback: just destroy if spawn failed
                this.isDragging = false;
                Delete();
            }
        }
    }

    protected override void DoSnapBack()
    {
        if (this.originalParent != null)
        {
            if (this.onlyDragToTargets)
            {
                var destPosition = this.originalWorldPosition;
                //nots sure
                // if (this.originalParent.IsChildOf(UiManager.IN.WorldCanvas.transform))
                // {
                //     destPosition -= DragManager.ScreenToWorldCameraDelta;
                // }
        
                //snap back to original position
                this.targetRectTransform.DOMove(destPosition, 0.2f).OnComplete(() =>
                {
                    this.targetRectTransform.SetParent(this.originalParent, true);
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

    protected override bool TryParentToTarget(out bool foundDragTarget)
    {
        foundDragTarget = false;
        if (this.shouldDetectDropTargets)
        {
            foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
            {
                if (possibleTarget != null && possibleTarget.TryGetComponent<DragTarget>(out var dragTarget))
                {
                    var cell = possibleTarget.GetComponentInParent<UiInventoryCell>();
                    if (cell != null)
                    {
                        dragTarget.SetAsParent(this.targetRectTransform);
                        this.targetRectTransform.SetAsLastSibling();
                        dragTarget.SetHighlight(false);
                        foundDragTarget = true;
                        return true;//found drag target, reparent and exit
                    }
                    else
                        return false;//bounce back
                }
            }
        }

        return false;
    }
    
    protected override bool TryInventoryCellDrop()
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
                    SaveManager.Data.InventoryItems[origCell.CellIndex] = null;
                }
                
                SaveManager.Data.InventoryItems[cell.CellIndex] = this.ItemData;
            }
            else
            {
                if (item == this)
                {
                    // Dropped back on original cell, do nothing
                    return true;
                }
                else if (item.ItemData.Id != this.ItemData.Id)
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