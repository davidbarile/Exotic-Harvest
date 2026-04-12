using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GlobalEnums;

public class UiCostItem : MonoBehaviour
{
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image resourceIcon;

    public void Configure(EResourceType resourceType, int amount)
    {
        bool hasEnough = ResourceManager.IN.HasResource(resourceType, amount);
        string color = hasEnough ? "white" : "red";
        costText.text = $"<color={color}>{amount}\n{resourceType}</color>";
        
        //this.resourceIcon.sprite = SpriteManager.GetSprite(resourceType.ToString());
    }
}
