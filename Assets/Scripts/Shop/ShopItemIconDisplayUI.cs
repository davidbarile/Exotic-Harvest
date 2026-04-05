using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemIconDisplayUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text quantityText;

    public void Configure(Sprite inIconSprite, int inQuantity, Color iconColor)
    {
        SpriteManager.SetImageSprite(this.itemIcon, inIconSprite);
        this.itemIcon.color = iconColor;
        this.quantityText.text = inQuantity > 1 ? $"x{inQuantity}" : string.Empty;
    }
}