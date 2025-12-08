using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiInventoryItem : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image shadow;
    [SerializeField] private TextMeshProUGUI itemQuantityText;

    public void Setup(ShopItemData itemData, int quantity)
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = itemData.Icon;
        }

        if (itemQuantityText != null)
        {
            itemQuantityText.text = quantity.ToString();
        }
    }
}
