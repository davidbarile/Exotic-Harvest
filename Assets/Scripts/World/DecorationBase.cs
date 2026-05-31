using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using static GlobalEnums;

public class DecorationBase : Draggable
{
    public Action OnDeinitialize;

    [Header("Inventory Item UI Elements")]
    [SerializeField] protected Image itemIcon;
    [SerializeField] protected Image shadow;

    public Image WorldProxy => this.worldProxy;
    [SerializeField] protected Image worldProxy;

    public DragTarget ChildDragTarget => this.childDragTarget;
    [SerializeField] protected DragTarget childDragTarget;

    protected PassiveHarvester linkedPassiveHarvester;
    protected Attractor attractor;

    private Vector2 originalIconSize = Vector2.zero;

    public InventoryItemData ItemData { get; private set; }

    [Header("Initialization Config - for setting up default decorations in the world")]
    [SerializeField] private InitInventoryItemData initItemData = new();

    protected virtual void Awake()
    {
        this.linkedPassiveHarvester = GetComponent<PassiveHarvester>();

        if(this.worldProxy)
            this.attractor = this.worldProxy.GetComponent<Attractor>();
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        if (this.initItemData.ShopItemConfig == null)
            return;

        this.initItemData.DisplayName = this.initItemData.ShopItemConfig.DisplayName;
        this.initItemData.Category = this.initItemData.ShopItemConfig.Category;
        this.initItemData.IconSpriteName = this.initItemData.ShopItemConfig.Icon != null ? this.initItemData.ShopItemConfig.Icon.name : string.Empty;
        this.initItemData.WorldIconSpriteName = this.initItemData.ShopItemConfig.WorldSprite != null ? this.initItemData.ShopItemConfig.WorldSprite.name : string.Empty;
        this.initItemData.Scale = this.initItemData.ShopItemConfig.Scale;
        this.initItemData.DecorationData = DecorationData.Copy(this.initItemData.ShopItemConfig.DecorationData);

        if (this.initItemData.Quantity <= 0)
            this.initItemData.Quantity = 1;

        if (this.initItemData.MaxStack <= 0)
            this.initItemData.MaxStack = 1;

        if(Application.isPlaying)
            Configure(this.initItemData);
    }

    public virtual void Configure(InventoryItemData inItemData)
    {
        this.ItemData = inItemData;

        this.transform.localScale = Vector3.one * inItemData.Scale;

        this.highlightValidTargetsWhenDragged = inItemData.DecorationData.HighlightValidTargetsWhenDragged;

        if (this.itemIcon)
        {
            if (this.originalIconSize == Vector2.zero)
                this.originalIconSize = this.itemIcon.rectTransform.sizeDelta;

            this.itemIcon.rectTransform.sizeDelta = this.originalIconSize;

            var wData = inItemData.DecorationData.WorldSaveData;

            Debug.Log($"Configuring {this.name}  this.itemIcon.transform.localScale = {itemIcon.transform.localScale}    wData.Scale = {wData.Scale}");

            this.itemIcon.transform.localScale = Vector3.one * wData.Scale;
            this.itemIcon.transform.rotation = Quaternion.Euler(0f, 0f, wData.Rotation);

            if (this.shadow)
               this.shadow.rectTransform.sizeDelta = this.originalIconSize;

            if (this.worldProxy)
                this.worldProxy.rectTransform.sizeDelta = this.originalIconSize;

            // if(worldProxy)
            //     Debug.Log($"Configuring {this.name}  this.itemIcon.transform.localScale = {itemIcon.transform.localScale}    wData.Scale = {wData.Scale}   this.worldProxy.rectTransform = { this.worldProxy.rectTransform.rect.size}");
                
            this.itemIcon.color = inItemData.IconColor;

            var worldSprite = inItemData.IconSpriteName;//default to using the regular icon if no world icon specified
            if(!string.IsNullOrEmpty(inItemData.WorldIconSpriteName))
                worldSprite = inItemData.WorldIconSpriteName;
            
            var sprite = SpriteManager.GetSprite(worldSprite);

            SpriteManager.SetImageSprite(this.itemIcon, sprite);
            SpriteManager.SetImageSprite(this.shadow, sprite);
            SpriteManager.SetImageSprite(this.worldProxy, sprite);
        }

        // if (this.worldProxy)
        //     this.worldProxy.gameObject.SetActive(false);

        if (TryGetComponent<PassiveHarvester>(out var harvester))
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
        this.ItemData.DecorationData.WorldSaveData.Scale = this.itemIcon ? this.itemIcon.transform.localScale.x : 1f;
        this.ItemData.DecorationData.WorldSaveData.Rotation = this.transform.localRotation.eulerAngles.z;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        DragManager.OnDragOverInventoryZoneActiveChanged?.Invoke(true);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (!this.isDragging)
            return;

        this.isDragging = false;//this is a hack to prevent double-firing from DragManager.TriggerEndDragOnCurrentObject

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
            this.OnDeinitialize?.Invoke();
            this.OnDeinitialize = null;
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
        transform.DOMove(DragManager.SnapBackStartPos, 0.2f).OnComplete(() =>
        {
            this.targetRectTransform.SetParent(this.originalParent, true);
            this.targetRectTransform.SetSiblingIndex(this.originalSiblingIndex);

            if (this.endCanvas == UiManager.IN.WorldCanvas)
                this.transform.localScale /= DragManager.UiCanvasScaleFactor;

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

        //maybe inefficient
        if(UiManager.IN.ResourcesPanel.IsShowing)
        {
            foreach (var obj in InputManager.ObjectsUnderMouse)
            {
                if(obj.transform.IsChildOf(UiManager.IN.ResourcesPanel.transform))
                {
                    if (TryGetComponent<PassiveHarvester>(out var harvester))
                    {
                        harvester.CollectAll();
                    }
                    break;
                }
            }
        }
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

    public virtual void ConfigureFromDrag(InventoryItemData inItemData, Transform inOriginalParent = null, int inOriginalSiblingIndex = -1)
    {
        Configure(inItemData);

        // Mark as actively being dragged
        this.isDragging = true;

        if (this.worldProxy)
        {
            this.worldProxy.gameObject.SetActive(true);
            this.worldProxy.transform.localPosition = DragManager.ScreenToWorldCameraDelta / this.transform.localScale.x;
        }
        
        // Store drag state for proper cleanup on drag end
        this.originalWorldPosition = this.targetRectTransform.position;
        // this.originalSiblingIndex = 0;
        this.originalParent = inOriginalParent != null ? inOriginalParent : this.targetRectTransform.parent;
        this.originalSiblingIndex = inOriginalSiblingIndex != -1 ? inOriginalSiblingIndex : this.targetRectTransform.GetSiblingIndex();

        this.OnWorldDropFailed = SnapBackToInventoryFromWorldFail;

        DragManager.OnDragOverInventoryZoneActiveChanged?.Invoke(true);
    }

    public override void SaveItemPosition()
    {
        this.ItemData.DecorationData.WorldSaveData.WorldPosition = this.transform.localPosition;
        this.ItemData.DecorationData.WorldSaveData.ParentGuid = this.targetRectTransform.parent.GetInstanceID();
        this.ItemData.DecorationData.WorldSaveData.SiblingIndex = this.targetRectTransform.GetSiblingIndex();
        this.ItemData.DecorationData.WorldSaveData.Scale = this.itemIcon ? this.itemIcon.transform.localScale.x : 1f;
        this.ItemData.DecorationData.WorldSaveData.Rotation = this.transform.localRotation.eulerAngles.z;

        var decorationBase = this.targetRectTransform.transform.parent.GetComponentInParent<DecorationBase>();

        //in case of stools and benches and such, we store a unique Guid
        if (decorationBase != null && decorationBase.ChildDragTarget != null)
            this.ItemData.DecorationData.WorldSaveData.ParentGuid = decorationBase.ItemData.DecorationData.Guid;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.TryGetComponent<Collectable>(out var collectible))
            {
                // if (!this.linkedPassiveHarvester.CanCollectResourceType(collectible.ResourceType))
                //     return;
                    
                this.linkedPassiveHarvester.TrySetActiveResourceType(collectible.ResourceType);

                float amountToAdd = collectible.Amount;
                if (this.linkedPassiveHarvester.ActiveResourceData != null)
                    amountToAdd *= this.linkedPassiveHarvester.ActiveResourceData.ConversionRatio;

                var success = this.linkedPassiveHarvester.TryAddAmount(amountToAdd, collectible.ResourceType);

                if (success)
                    collectible.Collect(false);
            }
        }
    }
}