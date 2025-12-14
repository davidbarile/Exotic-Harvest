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
}