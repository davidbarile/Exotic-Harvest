using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;

/// <summary>
/// UI component for displaying shop items
/// </summary>
public class ShopItemUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button itemButton;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject soldOutOverlay;
    [SerializeField] private GameObject cannotAffordOverlay;
    
    private ShopItem shopItem;
    private ShopItemDefinition itemDefinition;
    
    public event Action<ShopItem> OnItemSelected;
    
    private void Awake()
    {
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(SelectItem);
        }
    }
    
    public void Initialize(ShopItem item, ShopItemDefinition definition)
    {
        shopItem = item;
        itemDefinition = definition;
        
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
    
    private void OnResourceChanged(ResourceType type, int newAmount)
    {
        // Update display when resources change (affects affordability)
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (shopItem == null || itemDefinition == null) return;

        Debug.Log("Updating display for shop item: " + shopItem.displayName);
        
        // Update item name
        if (itemNameText != null)
        {
            itemNameText.text = itemDefinition.displayName;
        }
        
        // Update icon
        if (itemIcon != null)
        {
            itemIcon.sprite = itemDefinition.icon;
        }
        
        // Update background color
        if (backgroundImage != null)
        {
            backgroundImage.color = itemDefinition.backgroundColor;
        }
        
        // Update price display
        UpdatePriceDisplay();
        
        // Update availability overlays
        UpdateAvailabilityOverlays();
    }
    
    private void UpdatePriceDisplay()
    {
        if (priceText == null || shopItem?.cost == null) return;

        var sb = new StringBuilder();
        bool canAfford = true;

        foreach (var resource in shopItem.cost.RequiredResources)
        {
            if (sb.Length > 0) sb.Append(" ");

            bool hasEnough = ResourceManager.IN?.HasResource(resource.type, resource.amount) ?? false;
            if (!hasEnough) canAfford = false;

            string color = hasEnough ? "white" : "red";
            sb.AppendFormat("<color={0}>{1} {2}</color>", color, resource.amount, resource.DisplayName);
        }

        priceText.text = sb.ToString();
    }
    
    private void UpdateAvailabilityOverlays()
    {
        bool canPurchase = shopItem != null && shopItem.CanPurchase;
        bool canAfford = shopItem?.cost?.CanAfford(ResourceManager.IN) ?? false;
        
        // Show sold out overlay
        if (soldOutOverlay != null)
        {
            soldOutOverlay.SetActive(!canPurchase);
        }
        
        // Show cannot afford overlay
        if (cannotAffordOverlay != null)
        {
            cannotAffordOverlay.SetActive(canPurchase && !canAfford);
        }
        
        // Update button interactability
        if (itemButton != null)
        {
            itemButton.interactable = canPurchase && canAfford;
        }
    }
    
    private void SelectItem()
    {
        OnItemSelected?.Invoke(shopItem);
    }
    
    public void Refresh()
    {
        UpdateDisplay();
    }
}