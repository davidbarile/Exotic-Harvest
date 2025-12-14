using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiWorldItemBase : UiDraggable
{
    [Header("Inventory Item UI Elements")]

    [SerializeField] private Image itemIcon;
    [SerializeField] private Image shadow;
    [SerializeField] private TextMeshProUGUI itemQuantityText;

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

        if (this.itemQuantityText != null)
        {
            this.itemQuantityText.text = inItemData.Quantity > 1 ? inItemData.Quantity.ToString() : string.Empty;
        }
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
    }
}