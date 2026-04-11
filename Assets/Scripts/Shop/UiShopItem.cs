using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component for displaying shop items
/// </summary>
public class UiShopItem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button itemButton;
    [SerializeField] private GameObject[] itemIconDisplayObjects;
    [SerializeField] private UiShopItemIconDisplay[] itemIconDisplays;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text ownedText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject soldOutOverlay;
    [SerializeField] private GameObject cannotAffordOverlay;
    [SerializeField] private UiResourceDisplay[] resourceCostDisplays;
    
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
        this.itemNameText.text = this.shopItemData.DisplayName;

        //hide all displays
        foreach (var iconDisplayObject in this.itemIconDisplayObjects)
        {
            iconDisplayObject.SetActive(false);
        }

        //show icons and quantity based on number of resources if it's a resource item, otherwise show single icon
        if(this.shopItemData.IsResource)
        {
            Debug.Log($"Configuring resource item with {this.shopItemData.ResourceItems.Length} resource types.  this.itemIconDisplayObjects length: {this.itemIconDisplayObjects.Length}, this.itemIconDisplays length: {this.itemIconDisplays.Length}");
            this.itemIconDisplayObjects[this.shopItemData.ResourceItems.Length - 1].SetActive(true);

            var indexOffset = this.shopItemData.ResourceItems.Length - 1;

            if (this.shopItemData.ResourceItems.Length == 3)
                indexOffset = 3;
            else if (this.shopItemData.ResourceItems.Length == 4)
                indexOffset = 6;
                
            for(int i = 0; i < this.shopItemData.ResourceItems.Length && i < this.itemIconDisplays.Length; i++)
            {
                var resourceItem = this.shopItemData.ResourceItems[i];
                var iconDisplay = this.itemIconDisplays[i + indexOffset];
                var resourceData = ResourceManager.IN.Database.GetResource(resourceItem.ResourceType);
                iconDisplay.Configure(resourceData.Icon, resourceItem.Amount, resourceData.IconColor, $"{resourceItem.ResourceType}");
            }
        }
        else
        {
            this.itemIconDisplayObjects[0].SetActive(true);
            var iconDisplay = this.itemIconDisplays[0];
            iconDisplay.Configure(this.shopItemData.Icon, 1, this.shopItemData.IconColor);
        }

        // Update background color
        this.backgroundImage.color = this.shopItemData.BgColor;

        // Update price display
        UpdatePriceDisplay();

        // Update availability overlays
        UpdateAvailabilityOverlays();

        if(this.ownedText)
            this.ownedText.text = this.shopItemData.MaxPurchases > 0 ? $"Owned: {this.shopItemData.CurrentPurchases}/{this.shopItemData.MaxPurchases}" : string.Empty;
        // var color = this.shopItemData.CanPurchase ? Color.white : Color.gray;
        // this.ownedText.color = color;
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
        foreach (var iconDisplay in this.itemIconDisplays)
        {
            iconDisplay.SetSpriteSaturation(canPurchase && canAfford);
        }
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