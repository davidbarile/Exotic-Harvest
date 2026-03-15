using System;
using System.Text;
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
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject soldOutOverlay;
    [SerializeField] private GameObject cannotAffordOverlay;
    
    private ShopItemData shopItemData;
    // private ShopItemDefinition itemDefinition;
    
    public event Action<ShopItemData> OnItemSelected;
    
    private void Awake()
    {
        if (this.itemButton != null)
        {
            this.itemButton.onClick.AddListener(SelectItem);
        }
    }
    
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

        Debug.Log("Updating display for shop itemData: " + this.shopItemData.DisplayName);
        
        // Update itemData name
        if (this.itemNameText != null)
        {
            this.itemNameText.text = this.shopItemData.DisplayName;
        }
        
        // Update icon
        if (this.itemIcon != null)
        {
            this.itemIcon.sprite = this.shopItemData.Icon;
        }
        
        // Update background color
        if (this.backgroundImage != null)
        {
            this.backgroundImage.color = this.shopItemData.BackgroundColor;
        }
        
        // Update price display
        UpdatePriceDisplay();
        
        // Update availability overlays
        UpdateAvailabilityOverlays();
    }
    
    private void UpdatePriceDisplay()
    {
        if (this.priceText == null || this.shopItemData?.Cost == null) return;

        var sb = new StringBuilder();
        bool canAfford = true;

        foreach (var resource in this.shopItemData.Cost.RequiredResources)
        {
            if (sb.Length > 0) sb.Append(" ");

            bool hasEnough = ResourceManager.IN?.HasResource(resource.Type, resource.Amount) ?? false;
            if (!hasEnough) canAfford = false;

            string color = hasEnough ? "white" : "red";
            sb.AppendFormat("<color={0}>{1} {2}</color>", color, resource.Amount, resource.DisplayName);
        }

        this.priceText.text = sb.ToString();
    }
    
    private void UpdateAvailabilityOverlays()
    {
        bool canPurchase = this.shopItemData != null && this.shopItemData.CanPurchase;
        bool canAfford = this.shopItemData?.Cost?.CanAfford(ResourceManager.IN) ?? false;
        
        // Show sold out overlay
        if (this.soldOutOverlay != null)
        {
            this.soldOutOverlay.SetActive(!canPurchase);
        }
        
        // Show cannot afford overlay
        if (this.cannotAffordOverlay != null)
        {
            this.cannotAffordOverlay.SetActive(canPurchase && !canAfford);
        }
        
        // // Update button interactability
        // if (itemButton != null)
        // {
        //     itemButton.interactable = canPurchase && canAfford;
        // }
    }
    
    private void SelectItem()
    {
        OnItemSelected?.Invoke(this.shopItemData);
    }
    
    public void Refresh()
    {
        UpdateDisplay();
    }
}