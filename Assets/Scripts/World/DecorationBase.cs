using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using static GlobalEnums;

public class DecorationBase : Draggable
{
    [Header("Inventory Item UI Elements")]

    [SerializeField] protected Image itemIcon;
    [SerializeField] protected Image shadow;

    public Image WorldProxy => this.worldProxy;
    [SerializeField] protected Image worldProxy;

    protected PassiveHarvester linkedPassiveHarvester;

    public InventoryItemData ItemData { get; private set; }

    [Header("Initialization Config - for setting up default decorations in the world")]
    [SerializeField] private InitInventoryItemData initItemData = new();

    protected virtual void Awake()
    {
        this.linkedPassiveHarvester = GetComponent<PassiveHarvester>();
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        if (this.initItemData.ShopItemConfig == null)
            return;

        this.initItemData.DisplayName = this.initItemData.ShopItemConfig.DisplayName;
        this.initItemData.Category = this.initItemData.ShopItemConfig.Category;
        this.initItemData.IconSpriteName = this.initItemData.ShopItemConfig.Icon != null ? this.initItemData.ShopItemConfig.Icon.name : string.Empty;
        this.initItemData.Scale = this.initItemData.ShopItemConfig.Scale;
        this.initItemData.DecorationData = DecorationData.Copy(this.initItemData.ShopItemConfig.DecorationData);

        this.highlightValidTargetsWhenDragged = this.initItemData.DecorationData.HighlightValidTargetsWhenDragged;

        if (this.initItemData.Quantity <= 0)
            this.initItemData.Quantity = 1;

        if (this.initItemData.MaxStack <= 0)
            this.initItemData.MaxStack = 1;

        Configure(this.initItemData);
    }

    public virtual void Configure(InventoryItemData inItemData)
    {
        this.ItemData = inItemData;

        this.transform.localScale = Vector3.one * inItemData.Scale;

        if (this.itemIcon)
        {
            var sprite = SpriteManager.GetSprite(inItemData.IconSpriteName);
            this.itemIcon.sprite = sprite;
            this.itemIcon.color = inItemData.IconColor;

            if (this.shadow)
                this.shadow.sprite = sprite;

            if (this.worldProxy)
                this.worldProxy.sprite = sprite;
        }

        // if (this.worldProxy)
        //     this.worldProxy.gameObject.SetActive(false);
        
        if(TryGetComponent<PassiveHarvester>(out var harvester))
        { 
            harvester.SetDecorationData(this.ItemData.DecorationData);
        }
    }
    
    public virtual void InitWorldPositionAndParent()
    {
        OnValidate(); // Ensure ItemData and DecorationData are set up based on initItemData

        this.ItemData.DecorationData.WorldSaveData.WorldPosition = this.transform.localPosition;
        this.ItemData.DecorationData.WorldSaveData.ParentGuid = this.transform.parent.GetInstanceID();
        this.ItemData.DecorationData.WorldSaveData.SiblingIndex = this.transform.GetSiblingIndex();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        DragManager.OnDragOverInventoryZoneActiveChanged?.Invoke(true);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        //detect if Inventory is open and we're over it, if so, add the item back to the inventory and destroy this world item
        DragManager.OnDragOverInventoryZoneActiveChanged?.Invoke(false);

        var parentCanvas = this.GetComponentInParent<Canvas>();

        // if(this.worldProxy && parentCanvas?.renderMode == RenderMode.WorldSpace)
        //     this.worldProxy.gameObject.SetActive(false);

        var inventoryPanel = UiManager.IN.InventoryPanel;

        if (inventoryPanel.IsShowing)
        {
            var cell = IsOverInventoryCell();

            var shouldAddToEmptyOrBounceBackToWorld = false;

            if (cell != null)
            {
                if (cell.Item == null)
                {
                    if (TryGetComponent<PassiveHarvester>(out var harvester))
                    {
                        harvester.CollectAll();
                    }

                    //if cell empty, add item to that cell
                    inventoryPanel.SpawnInventoryItemInCell(this.ItemData, cell.CellIndex);
                    SaveManager.Data.InventoryItems[cell.CellIndex] = InventoryItemData.Copy(this.ItemData);
                    InventoryManager.OnInventoryRefreshed?.Invoke();
                }
                else
                {
                    //if cell not empty
                    var existingItemData = cell.Item.ItemData;
                    var isSameItem = existingItemData.DisplayName == this.ItemData.DisplayName;//TODO: maybe add ItemID or something for better comparison

                    if (isSameItem && existingItemData.Quantity < existingItemData.MaxStack)
                    {
                        //same item, add quantity if not max
                        existingItemData.Quantity += 1;
                        SaveManager.Data.InventoryItems[cell.CellIndex] = InventoryItemData.Copy(existingItemData);
                        InventoryManager.OnInventoryRefreshed?.Invoke();
                    }
                    else
                    {
                        shouldAddToEmptyOrBounceBackToWorld = true;
                    }
                }
            }
            else
            {
                //not over cell
                shouldAddToEmptyOrBounceBackToWorld = true;
            }

            if (shouldAddToEmptyOrBounceBackToWorld)
            {
                //loop thru inventory and find first cell of same item with available quantity to stack into
                var cellWithSpace = inventoryPanel.GetFirstCellWithSpace(this.ItemData);
                if (cellWithSpace != null)
                {
                    var existingItemData = cellWithSpace.Item.ItemData;
                    existingItemData.Quantity += 1;
                    SaveManager.Data.InventoryItems[cellWithSpace.CellIndex] = InventoryItemData.Copy(existingItemData);
                    InventoryManager.OnInventoryRefreshed?.Invoke();
                }
                else
                {
                    var firstEmptyCell = inventoryPanel.GetFirstEmptyCell();
                    if (firstEmptyCell != null)
                    {
                        //if available cell with space, add to that cell
                        inventoryPanel.SpawnInventoryItemInCell(this.ItemData, firstEmptyCell.CellIndex);
                        SaveManager.Data.InventoryItems[firstEmptyCell.CellIndex] = InventoryItemData.Copy(this.ItemData);
                        InventoryManager.OnInventoryRefreshed?.Invoke();
                    }
                    else
                    {
                        //bounce back to original position in world and close InventoryPanel
                        SnapBackToWorldFromInventoryFail();
                        inventoryPanel.Hide();
                        return;
                    }
                }
            }

            SaveManager.Data.WorldItems.Remove(this.ItemData);
            Destroy(this.gameObject);
        }
        else
        {
            //maybe do it on base
            base.OnEndDrag(eventData);
        }
    }

    private void SnapBackToWorldFromInventoryFail()
    {
        //snap back to original position
        transform.DOMove(this.originalWorldPosition, 0.2f).OnComplete(() =>
        {
            this.targetRectTransform.SetParent(this.originalParent, true);
            this.targetRectTransform.SetSiblingIndex(this.originalSiblingIndex);

            SaveItemPosition();
        });
    }

    private void SnapBackToInventoryFromWorldFail()
    {
        UiManager.IN.InventoryPanel.Show();

        var origCell = this.originalParent.GetComponentInParent<UiInventoryCell>();
        
        this.transform.SetParent(UiManager.IN.DragCanvas, true);

        //snap back to original position
        this.transform.DOMove(origCell.transform.position, 0.2f).OnComplete(() =>
        {
            UiManager.IN.InventoryPanel.SpawnInventoryItemInCell(this.ItemData, origCell.CellIndex);
            Destroy(this.gameObject);
        });
    }

    public override void OnDragUpdate()
    {
        CheckIfOverInventoryZone();
    }

    public static bool CheckIfOverInventoryZone()
    {
        foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
        {
            if (possibleTarget != null && possibleTarget.TryGetComponent<DragTarget>(out var dragTarget) && dragTarget != null && dragTarget.IsDragOverOpenInventoryZone)
            {
                UiManager.IN.InventoryPanel.Show();
                UiManager.IN.InventoryPanel.SwitchCategory(EShopCategory.All);
                return true;
            }
        }

        return false;
    }

    public static UiInventoryCell IsOverInventoryCell()
    {
        foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
        {
            if (possibleTarget && possibleTarget.TryGetComponent<UiInventoryCell>(out var dragTargetCell) && dragTargetCell != null)
                return dragTargetCell;
        }

        return null;
    }

    public virtual void ConfigureFromDrag(InventoryItemData inItemData, Vector3 inOffsetFromCursor, Transform inOriginalParent = null, int inOriginalSiblingIndex = -1)
    {
        Configure(inItemData);

        // Mark as actively being dragged
        this.isDragging = true;

        if(this.worldProxy)
        {
            this.worldProxy.gameObject.SetActive(true);
            this.worldProxy.transform.localPosition = DragManager.ScreenToWorldCameraDelta / this.transform.localScale.x;
        }

        // Store drag state for proper cleanup on drag end
        this.offsetFromCursor = inOffsetFromCursor;
        this.originalWorldPosition = this.targetRectTransform.position;
        // this.originalSiblingIndex = 0;
        this.originalParent = inOriginalParent != null ? inOriginalParent : this.targetRectTransform.parent;
        this.originalSiblingIndex = inOriginalSiblingIndex != -1 ? inOriginalSiblingIndex : this.targetRectTransform.GetSiblingIndex();

        this.OnWorldDropFailed = SnapBackToInventoryFromWorldFail;

        DragManager.OnDragOverInventoryZoneActiveChanged?.Invoke(true);
    }

    protected override void SaveItemPosition()
    {
        this.ItemData.DecorationData.WorldSaveData.WorldPosition = this.transform.localPosition;
        this.ItemData.DecorationData.WorldSaveData.ParentGuid = this.targetRectTransform.parent.GetInstanceID();
        this.ItemData.DecorationData.WorldSaveData.SiblingIndex = this.targetRectTransform.GetSiblingIndex();

        //in case of stools and benches and such, we store a unique Guid
        if(this.targetRectTransform.parent.parent.TryGetComponent<DecorationBase>(out var decorationBase))
        {
            if(decorationBase.ItemData.DecorationData.IsDragZone)
            {
                this.ItemData.DecorationData.WorldSaveData.ParentGuid = decorationBase.ItemData.DecorationData.Guid;
            }
        }

        Debug.Log($"Saving position for {this.ItemData.DisplayName} at {this.ItemData.DecorationData.WorldSaveData.WorldPosition}. ParentGuid: {this.ItemData.DecorationData.WorldSaveData.ParentGuid}, SiblingIndex: {this.ItemData.DecorationData.WorldSaveData.SiblingIndex}");
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if(collision.TryGetComponent<Collectable>(out var collectible))
            {
                if (!this.linkedPassiveHarvester.CollectableResourceTypes.Contains(collectible.ResourceType))
                    return;

                var success = this.linkedPassiveHarvester.AddAmount(collectible.Amount);

                if (success)
                    collectible.Collect(false);
            }
        }
    }
}