using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for displaying a single resource amount
/// </summary>
public class ResourceDisplayUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image backgroundImage;

    [SerializeField] private bool isShopDisplay; // Set to false for static displays (e.g. shop costs)

    [Space, SerializeField] private TooltipTrigger tooltipTrigger;
    
    private EResourceType resourceType;
    private ResourceConfig resourceConfig;

    private int costAmount; // For shop displays, the amount required (not current amount)

    public void Configure(EResourceType type, ResourceConfig config)
    {
        this.resourceType = type;
        this.resourceConfig = config;

        if (this.tooltipTrigger != null)
        {
            this.tooltipTrigger.TooltipText = config.DisplayName;//$"{config.DisplayName}\n{config.Description}";
        }

        UpdateDisplay();

        // Subscribe to resource changes
        if (ResourceManager.IN != null)
            ResourceManager.OnResourceChanged += OnResourceChanged;
    }
    
    public void Configure(EResourceType type, ResourceData data)
    {
        this.resourceConfig = data.GetConfig();
        this.costAmount = data.Amount;
        Configure(type, this.resourceConfig);
    }
    
    private void OnDestroy()
    {
        if (ResourceManager.IN != null)
            ResourceManager.OnResourceChanged -= OnResourceChanged;
    }

    private void OnResourceChanged(EResourceType type, int newAmount)
    {
        if (type == this.resourceType)
        {
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (ResourceManager.IN == null) return;

        int currentAmount = ResourceManager.IN.GetResourceAmount(this.resourceType);

        // Update amount text
        if (this.amountText != null)
        {
            var displayAmount = isShopDisplay ? this.costAmount : currentAmount;
            this.amountText.text = displayAmount.ToString();
            this.amountText.color = this.resourceConfig?.UiColor ?? Color.white;

            if(isShopDisplay)
            {
                bool hasEnough = ResourceManager.IN?.HasResource(this.resourceType, this.costAmount) ?? false;

                if(!hasEnough)
                    this.amountText.color = Color.red;
            }
        }

        // Update icon
        if (this.iconImage != null && this.resourceConfig != null)
        {
            this.iconImage.sprite = this.resourceConfig.Icon;
            this.iconImage.color = this.resourceConfig.UiColor;
        }

        // Update background color based on resource category
        if (this.backgroundImage != null && this.resourceConfig != null)
        {
            Color bgColor = GetCategoryColor(this.resourceConfig.Category);
            bgColor.a = 0.3f;
            this.backgroundImage.color = bgColor;
        }
    }
    
    //TODO: Move this to a color manager
    private Color GetCategoryColor(EResourceCategory category)
    {
        return category switch
        {
            EResourceCategory.Primary => new Color(0.2f, 0.6f, 1f), // Blue
            EResourceCategory.Bugs => new Color(0.8f, 0.4f, 0.2f), // Orange
            EResourceCategory.Nature => new Color(0.2f, 0.8f, 0.2f), // Green
            EResourceCategory.NightSky => new Color(0.4f, 0.2f, 0.8f), // Purple
            EResourceCategory.Valuables => new Color(1f, 0.8f, 0.2f), // Gold
            EResourceCategory.Abstract => new Color(0.6f, 0.6f, 0.6f), // Gray
            EResourceCategory.Special => new Color(1f, 0.2f, 0.8f), // Pink
            EResourceCategory.Premium => new Color(0.8f, 0.2f, 0.2f), // Red
            _ => Color.white
        };
    }
}