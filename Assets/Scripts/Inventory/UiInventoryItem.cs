using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiInventoryItem : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image shadow;
    [SerializeField] private TextMeshProUGUI itemQuantityText;

    public InventoryItemData ItemData { get; private set; }

    public void Setup(InventoryItemData itemData, int quantity)
    {
        this.ItemData = itemData;

        if (itemIcon != null)
        {
            var sprite = SpriteManager.GetSprite(itemData.IconSpriteName);
            itemIcon.sprite = sprite;
            shadow.sprite = sprite;
        }

        if (itemQuantityText != null)
        {
            itemQuantityText.text = quantity > 1 ? quantity.ToString() : string.Empty;
        }
    }
}