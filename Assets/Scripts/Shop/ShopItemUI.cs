using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component for displaying shop items
/// </summary>
public class ShopItemUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button itemButton;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject soldOutOverlay;
    [SerializeField] private GameObject cannotAffordOverlay;
    [SerializeField] private ResourceDisplayUI[] resourceCostDisplays;
    
    private ShopItemData shopItemData;
    // private ShopItemDefinition itemDefinition;
    
    public event Action<ShopItemData> OnItemSelected;
    
    public void Initialize(ShopItemData itemData)
    {
        this.shopItemData = itemData;
        //itemDefinition = definition;
        
        UpdateDisplay();
        
        // Subscribe to relevant events
        if (ResourceManager.IN != null)
        {
            ResourceManager.OnResourceChanged += OnResourceChanged;
        }
    }
    
    private void OnDestroy()
    {
        if (ResourceManager.IN != null)
        {
            ResourceManager.OnResourceChanged -= OnResourceChanged;
        }
    }
    
    private void OnResourceChanged(EResourceType type, int newAmount)
    {
        // Update display when resources change (affects affordability)
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (this.shopItemData == null) return;
        
        // Update itemData name
        if (this.itemNameText != null)
        {
            this.itemNameText.text = this.shopItemData.DisplayName;
        }
        
        // Update icon
        if (this.itemIcon)
        {
            this.itemIcon.sprite = this.shopItemData.Icon;

            if (this.shopItemData.IsResource)
                this.itemIcon.color = ResourceManager.IN.Database.GetResource(this.shopItemData.ResourceItems[0].ResourceType)?.IconColor ?? this.shopItemData.IconColor;
            else
                this.itemIcon.color = this.shopItemData.IconColor;
        }
        
        // Update background color
        this.backgroundImage.color = this.shopItemData.BgColor;
        
        // Update price display
        UpdatePriceDisplay();
        
        // Update availability overlays
        UpdateAvailabilityOverlays();
    }
    
    private void UpdatePriceDisplay()
    {
        if (this.shopItemData?.Cost == null) return;

        for(int i = 0; i < resourceCostDisplays.Length; i++)
        {
            var costDisplay = resourceCostDisplays[i];
            var shouldShow = i < this.shopItemData.Cost.RequiredResources.Count;
            costDisplay.gameObject.SetActive(shouldShow);
            if (shouldShow)
            {
                var costResourceData = this.shopItemData.Cost.RequiredResources[i];
                costDisplay.Configure(costResourceData.Type, costResourceData);
                costDisplay.gameObject.SetActive(true);
            }
        }
    }

    private void UpdateAvailabilityOverlays()
    {
        bool canPurchase = this.shopItemData != null && this.shopItemData.CanPurchase;
        bool canAfford = this.shopItemData?.Cost?.CanAfford() ?? false;

        // Show sold out overlay
        this.soldOutOverlay.SetActive(!canPurchase);

        // Show cannot afford overlay
        this.cannotAffordOverlay.SetActive(canPurchase && !canAfford);

        //this.itemButton.interactable = canPurchase && canAfford;
    }
    
    //called by Button and other children with ClickableObject script on prefab
    public void SelectItem()
    {
        OnItemSelected?.Invoke(this.shopItemData);
    }
    
    public void Refresh()
    {
        UpdateDisplay();
    }
}