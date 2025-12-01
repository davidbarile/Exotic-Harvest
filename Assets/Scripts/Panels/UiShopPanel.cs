using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiShopPanel : UIPanelBase
{
    [Header("Shop UI Elements")]
    [SerializeField] private Transform categoryTabsParent;
    [SerializeField] private Transform itemsGridParent;
    [SerializeField] private GameObject shopItemPrefab; // Assign ShopItemUI prefab here
    [SerializeField] private Button[] categoryTabs;
    
    [Header("Item Detail Panel")]
    [SerializeField] private GameObject itemDetailPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TextMeshProUGUI purchaseButtonText;
    [SerializeField] private Transform costDisplayParent;
    [SerializeField] private GameObject costItemPrefab;
    
    private EShopCategory currentCategory = EShopCategory.Decorations;
    private ShopItem selectedItem;
    private List<GameObject> currentItemDisplays = new List<GameObject>();
    private List<GameObject> currentCostDisplays = new List<GameObject>();
    
    private void Start()
    {
        SetupCategoryTabs();
        SetupEventListeners();
        RefreshShop();
    }
    
    private void OnEnable()
    {
        if (ShopManager.IN != null)
        {
            ShopManager.OnItemPurchased += OnItemPurchased;
            ShopManager.OnPurchaseFailed += OnPurchaseFailed;
            ShopManager.OnShopRefreshed += RefreshShop;
        }
        
        if (ResourceManager.IN != null)
        {
            ResourceManager.OnResourceChanged += OnResourceChanged;
        }
    }
    
    private void OnDisable()
    {
        if (ShopManager.IN != null)
        {
            ShopManager.OnItemPurchased -= OnItemPurchased;
            ShopManager.OnPurchaseFailed -= OnPurchaseFailed;
            ShopManager.OnShopRefreshed -= RefreshShop;
        }
        
        if (ResourceManager.IN != null)
        {
            ResourceManager.OnResourceChanged -= OnResourceChanged;
        }
    }
    
    private void SetupCategoryTabs()
    {
        // Setup category tab buttons if they exist
        if (categoryTabs != null)
        {
            for (int i = 0; i < categoryTabs.Length; i++)
            {
                int categoryIndex = i;
                var tab = categoryTabs[i];
                if (tab != null)
                {
                    tab.onClick.AddListener(() => SwitchCategory((EShopCategory)categoryIndex));
                    tab.GetComponentInChildren<TextMeshProUGUI>().text = ((EShopCategory)categoryIndex).ToString();
                }
            }
        }
    }
    
    private void SetupEventListeners()
    {
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(PurchaseSelectedItem);
        }
    }
    
    public void SwitchCategory(EShopCategory category)
    {
        currentCategory = category;
        selectedItem = null;
        RefreshItemGrid();
        HideItemDetail();
    }
    
    private void RefreshShop()
    {
        RefreshItemGrid();
        RefreshItemDetail();
    }
    
    private void RefreshItemGrid()
    {
        // Clear existing items
        foreach (var item in currentItemDisplays)
        {
            if (item != null)
                Destroy(item);
        }
        currentItemDisplays.Clear();
        
        if (ShopManager.IN == null || itemsGridParent == null)
            return;
            
        // Get items for current category
        var items = ShopManager.IN.GetItemsByCategory(currentCategory);
        
        // Create UI elements for each item
        foreach (var item in items)
        {
            if (item.isUnlocked)
                CreateItemDisplay(item);
        }
    }
    
    private void CreateItemDisplay(ShopItem item)
    {
        if (shopItemPrefab == null)
            return;
            
        GameObject itemObj = Instantiate(shopItemPrefab, itemsGridParent);
        currentItemDisplays.Add(itemObj);
        
        // Setup item display (this would be expanded with actual UI components)
        Button itemButton = itemObj.GetComponent<Button>();
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(() => SelectItem(item));
        }
        
        // Setup visual elements (name, icon, etc.) - requires actual prefab setup
        var nameText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = item.displayName;
    }
    
    private void SelectItem(ShopItem item)
    {
        selectedItem = item;
        ShowItemDetail();
    }
    
    private void ShowItemDetail()
    {
        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(true);
            
        RefreshItemDetail();
    }
    
    private void HideItemDetail()
    {
        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(false);
    }
    
    private void RefreshItemDetail()
    {
        if (selectedItem == null)
            return;
            
        // Update item info
        if (itemNameText != null)
            itemNameText.text = selectedItem.displayName;
            
        if (itemDescriptionText != null)
            itemDescriptionText.text = selectedItem.description;
            
        if (itemIcon != null && selectedItem.icon != null)
            itemIcon.sprite = selectedItem.icon;
        
        // Update purchase button
        RefreshPurchaseButton();
        
        // Update cost display
        RefreshCostDisplay();
    }
    
    private void RefreshPurchaseButton()
    {
        if (purchaseButton == null || selectedItem == null)
            return;
            
        bool canPurchase = selectedItem.CanPurchase && 
                          (selectedItem.cost?.CanAfford(ResourceManager.IN) ?? false);
        
        purchaseButton.interactable = canPurchase;
        
        if (purchaseButtonText != null)
        {
            if (!selectedItem.CanPurchase)
                purchaseButtonText.text = selectedItem.IsMaxedOut ? "Max Purchased" : "Locked";
            else if (!canPurchase)
                purchaseButtonText.text = "Can't Afford";
            else
                purchaseButtonText.text = "Purchase";
        }
    }
    
    private void RefreshCostDisplay()
    {
        // Clear existing cost displays
        foreach (var costDisplay in currentCostDisplays)
        {
            if (costDisplay != null)
                Destroy(costDisplay);
        }
        currentCostDisplays.Clear();
        
        if (selectedItem?.cost == null || costDisplayParent == null || costItemPrefab == null)
            return;
            
        // Create cost displays
        foreach (var resource in selectedItem.cost.RequiredResources)
        {
            GameObject costObj = Instantiate(costItemPrefab, costDisplayParent);
            currentCostDisplays.Add(costObj);
            
            // Setup cost display (would need actual prefab components)
            var costText = costObj.GetComponentInChildren<TextMeshProUGUI>();
            if (costText != null)
            {
                bool hasEnough = ResourceManager.IN.HasResource(resource.type, resource.amount);
                string color = hasEnough ? "white" : "red";
                costText.text = $"<color={color}>{resource.amount}\n{resource.type}</color>";
                //TODO: add icon and make class for this
            }
        }
    }
    
    private void PurchaseSelectedItem()
    {
        if (selectedItem != null && ShopManager.IN != null)
        {
            ShopManager.IN.TryPurchaseItem(selectedItem);
        }
    }
    
    private void OnItemPurchased(ShopItem item)
    {
        // Refresh UI after purchase
        RefreshItemDetail();
        RefreshItemGrid();
        
        // TODO: Show purchase success feedback
    }
    
    private void OnPurchaseFailed(ShopItem item, string reason)
    {
        // TODO: Show purchase failure message
        Debug.Log($"Purchase failed: {reason}");
    }
    
    private void OnResourceChanged(ResourceType type, int newAmount)
    {
        // Update purchase button state when resources change
        RefreshPurchaseButton();
        RefreshCostDisplay();
    }
}