using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UiWorldItemBase : UiDraggable
{
    [Header("Inventory Item UI Elements")]

    [SerializeField] private Image itemIcon;
    [SerializeField] private Image shadow;

    public InventoryItemData ItemData { get; private set; }

    public virtual void Configure(InventoryItemData inItemData)
    {
        this.ItemData = inItemData;

        if (this.itemIcon != null)
        {
            var sprite = SpriteManager.GetSprite(inItemData.IconSpriteName);
            this.itemIcon.sprite = sprite;
            this.shadow.sprite = sprite;
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        DragManager.OnDragOverInventoryZoneActiveChanged?.Invoke(true);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        DragManager.OnDragOverInventoryZoneActiveChanged?.Invoke(false);
    }

    protected override bool DoOnDrag()
    {
        if (CheckIfOverInventoryZone())
        {
            return false;
        }

        return true;
    }

    public static bool CheckIfOverInventoryZone()
    {
        foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
        {
            if (possibleTarget.TryGetComponent<UiDragTarget>(out var dragTarget) && dragTarget.IsDragOverOpenInventoryZone)
            {
                UiManager.IN.InventoryPanel.Show();
                return true;
            }
        }

        return false;
    }

    public virtual void InitializeFromDrag(InventoryItemData inItemData, Vector2 dragOffset)
    {
        Configure(inItemData);

        // Mark as actively being dragged
        this.isDragging = true;

        // Store drag state for proper cleanup on drag end
        this.originalLocalPointerPosition = dragOffset;
        this.originalLocalPosition = this.targetRectTransform.localPosition;
        this.originalWorldPosition = this.targetRectTransform.position;
        this.originalParent = DragManager.IN.DefaultParent;
        this.originalSiblingIndex = 0;

        DragManager.OnDragOverInventoryZoneActiveChanged?.Invoke(true);
    }

    protected override void SaveItemPosition()
    {
        if (this.ItemData != null)
            this.ItemData.DecorationData.WorldPosition = this.transform.position;
    }
}