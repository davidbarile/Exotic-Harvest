using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemIconDisplayUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TooltipTrigger tooltipTrigger;

    public void Configure(Sprite inIconSprite, int inQuantity, Color iconColor, string tooltipText = "")
    {
        SpriteManager.SetImageSprite(this.itemIcon, inIconSprite);
        this.itemIcon.color = iconColor;
        this.quantityText.text = inQuantity > 1 ? $"x{inQuantity}" : string.Empty;

        var hasTooltip = !string.IsNullOrEmpty(tooltipText);

        if (this.tooltipTrigger && hasTooltip)
            this.tooltipTrigger.TooltipText = tooltipText;
    }
}