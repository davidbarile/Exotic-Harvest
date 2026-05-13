using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveResourceDisplay : MonoBehaviour
{
    [SerializeField] private Image activeResourceIcon;
    [SerializeField] private Image resourceFillImage;
    [SerializeField] private TextMeshProUGUI resourceQuantityText;

    public void SetIcon(Sprite inIcon)
    {
        if (this.activeResourceIcon)
            this.activeResourceIcon.sprite = inIcon;
    }

    public void SetValue(int inCurrentAmount, int inMaxAmount)
    {
        if (this.resourceQuantityText)
            this.resourceQuantityText.text = $"{inCurrentAmount}";

        if (this.resourceFillImage)
            this.resourceFillImage.fillAmount = inMaxAmount > 0 ? (float)inCurrentAmount / inMaxAmount : 0f;
    }
}
