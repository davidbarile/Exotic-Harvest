using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Lean.Pool;
using static ColorPalette;
using static GlobalEnums;

/// <summary>
/// UI component for displaying a single resource amount
/// </summary>
public class UiResourceDisplay : MonoBehaviour, IPoolable
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private UiTickTextToValue tickTextComponent;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private bool isShopDisplay; // Set to false for static displays (e.g. shop costs)
    [Space, SerializeField] private TooltipTrigger tooltipTrigger;
    
    private EResourceType resourceType;
    private ResourceConfig resourceConfig;

    private bool isResourcesDisplay;

    private int costAmount; // For shop displays, the amount required (not current amount)

    public void Configure(EResourceType inType, ResourceConfig inConfig, bool inIsResourcesDisplay = false)
    {
        this.resourceType = inType;
        this.resourceConfig = inConfig;
        this.isResourcesDisplay = inIsResourcesDisplay;

        UpdateDisplay(false);

        ResourceManager.OnResourceChanged -= OnResourceChanged;
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
        ResourceManager.OnResourceChanged -= OnResourceChanged;
    }
    
    public void OnSpawn()
    {
        ResourceManager.OnResourceChanged -= OnResourceChanged;
        ResourceManager.OnResourceChanged += OnResourceChanged;
    }

    public void OnDespawn()
    {
        ResourceManager.OnResourceChanged -= OnResourceChanged;
    }

    private void OnResourceChanged(EResourceType type, int newAmount)
    {
        if (type == this.resourceType)
        {
            UpdateDisplay();
        }
    }

    private void UpdateDisplay(bool inShouldTickToNewValue = true)
    {
        if (ResourceManager.IN == null) return;

        int currentAmount = ResourceManager.IN.GetResourceAmount(this.resourceType);

        var displayAmount = this.isShopDisplay ? this.costAmount : currentAmount;

        // Update amount text
        if (inShouldTickToNewValue)
            this.tickTextComponent.SetValue(displayAmount);
        else
            this.amountText.text = displayAmount.ToString();

        this.amountText.color = Color.white;

        bool hasEnough = true;

        if (this.isShopDisplay)
        {
            hasEnough = ResourceManager.IN?.HasResource(this.resourceType, this.costAmount) ?? false;

            if (!hasEnough)
                this.amountText.color = Color.red;
        }

        // Update icon
        this.iconImage.sprite = this.resourceConfig.Icon;
        this.iconImage.color = this.resourceConfig.IconColor;

        // Update background color based on resource category
        var bgColor = ColorManager.IN.GetResourceCategoryColor(this.resourceConfig.Category, EColorType.Dark);

        // If this is a resources display, gray out display if amount is 0
        if (this.isResourcesDisplay && currentAmount == 0)
        {
            this.amountText.color = Color.Lerp(Color.grey, Color.white, 0.6f);
            bgColor = ColorManager.IN.GetResourceCategoryColor(this.resourceConfig.Category, EColorType.Disabled);

            var iconColor = Color.grey;
            iconColor.a = 0.6f;
            this.iconImage.color = iconColor;
        }

        //bgColor.a = 0.3f;
        this.backgroundImage.color = bgColor;

        if (this.tooltipTrigger != null)
        {
            if (this.isShopDisplay)
            {
                if (hasEnough)
                    this.tooltipTrigger.TooltipText = $"{this.resourceConfig.DisplayName}";
                else
                    this.tooltipTrigger.TooltipText = $"{this.resourceConfig.DisplayName}\n<color=red>{currentAmount}/{this.costAmount}</color>";
            }
            else
                this.tooltipTrigger.TooltipText = $"{this.resourceConfig.DisplayName}\n<size=28>{this.resourceConfig.Description}</size>";
        }
    }

    //called from ClickableObject script on prefab
    public void HandleRightClick()
    {
        //TODO: disable in final build
        var amount = ResourceManager.IN.DebugAddAmount;
        ResourceManager.IN.AddResource(this.resourceType, amount);
    }

    public void HandleMiddleClick()
    {
        //TODO: disable in final build
        var amount = ResourceManager.IN.DebugAddAmount;
        var cost = new ResourceCost(this.resourceType, amount);
        ResourceManager.IN.SpendResources(cost);
    }

    public void HandleLeftClick()
    {
        Debug.Log($"You have  {ResourceManager.IN.GetResourceAmount(this.resourceType)} {this.resourceType}");
    }
}