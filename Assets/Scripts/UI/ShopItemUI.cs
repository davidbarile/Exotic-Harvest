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
    
    private ShopItemData shopItemData;
    // private ShopItemDefinition itemDefinition;
    
    public event Action<ShopItemData> OnItemSelected;
    
    private void Awake()
    {
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(SelectItem);
        }
    }
    
    public void Initialize(ShopItemData itemData)
    {
        shopItemData = itemData;
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
    
    private void OnResourceChanged(ResourceType type, int newAmount)
    {
        // Update display when resources change (affects affordability)
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (shopItemData == null) return;

        Debug.Log("Updating display for shop itemData: " + shopItemData.DisplayName);
        
        // Update itemData name
        if (itemNameText != null)
        {
            itemNameText.text = shopItemData.DisplayName;
        }
        
        // Update icon
        if (itemIcon != null)
        {
            itemIcon.sprite = shopItemData.Icon;
        }
        
        // Update background color
        if (backgroundImage != null)
        {
            backgroundImage.color = shopItemData.BackgroundColor;
        }
        
        // Update price display
        UpdatePriceDisplay();
        
        // Update availability overlays
        UpdateAvailabilityOverlays();
    }
    
    private void UpdatePriceDisplay()
    {
        if (priceText == null || shopItemData?.Cost == null) return;

        var sb = new StringBuilder();
        bool canAfford = true;

        foreach (var resource in shopItemData.Cost.RequiredResources)
        {
            if (sb.Length > 0) sb.Append(" ");

            bool hasEnough = ResourceManager.IN?.HasResource(resource.Type, resource.Amount) ?? false;
            if (!hasEnough) canAfford = false;

            string color = hasEnough ? "white" : "red";
            sb.AppendFormat("<color={0}>{1} {2}</color>", color, resource.Amount, resource.DisplayName);
        }

        priceText.text = sb.ToString();
    }
    
    private void UpdateAvailabilityOverlays()
    {
        bool canPurchase = shopItemData != null && shopItemData.CanPurchase;
        bool canAfford = shopItemData?.Cost?.CanAfford(ResourceManager.IN) ?? false;
        
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
        
        // // Update button interactability
        // if (itemButton != null)
        // {
        //     itemButton.interactable = canPurchase && canAfford;
        // }
    }
    
    private void SelectItem()
    {
        OnItemSelected?.Invoke(shopItemData);
    }
    
    public void Refresh()
    {
        UpdateDisplay();
    }
}