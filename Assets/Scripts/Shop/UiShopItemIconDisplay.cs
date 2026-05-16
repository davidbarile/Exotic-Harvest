using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiShopItemIconDisplay : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TooltipTrigger tooltipTrigger;

    private Vector2 originalIconSize = Vector2.zero;

    private static readonly int desaturateID = Shader.PropertyToID("_DesaturateStrength");

    public void Configure(Sprite inIconSprite, int inQuantity, Color iconColor, string tooltipText = "")
    {
        if (this.originalIconSize == Vector2.zero)
            this.originalIconSize = this.itemIcon.rectTransform.sizeDelta;

        this.itemIcon.rectTransform.sizeDelta = this.originalIconSize;
            
        SpriteManager.SetImageSprite(this.itemIcon, inIconSprite);
        this.itemIcon.color = iconColor;
        this.quantityText.text = inQuantity > 1 ? $"x{inQuantity}" : string.Empty;

        var hasTooltip = !string.IsNullOrEmpty(tooltipText);

        if (this.tooltipTrigger && hasTooltip)
            this.tooltipTrigger.TooltipText = tooltipText;
    }

    public void SetSpriteSaturation(bool inIsSaturated)
    {
        if (this.itemIcon != null)
        {
            // this way we don't create instances
            this.itemIcon.material = inIsSaturated ? ColorManager.IN.SaturateMaterial : ColorManager.IN.DesaturateMaterial;
        }
    }
}