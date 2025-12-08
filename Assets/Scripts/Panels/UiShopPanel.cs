using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiShopPanel : UIPanelBase
{
    [Header("Shop UI Elements")]
    [SerializeField] private Transform categoryTabsParent;
    [SerializeField] private Transform itemsGridParent;
    [SerializeField] private Toggle[] categoryTabs;
    [SerializeField] private ShopItemUI shopItemPrefab;
    
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
    private ShopItemData selectedItemData;
    private List<GameObject> currentItemDisplays = new();
    private List<GameObject> currentCostDisplays = new();
    
    private void Start()
    {
        SetupCategoryTabs();
        RefreshShop();
    }
    
    protected override void RegisterEvents()
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
        
        if (this.purchaseButton != null)
            this.purchaseButton.onClick.AddListener(PurchaseSelectedItem);

        RefreshShop();
        
        var selectedTab = this.categoryTabs[(int)this.currentCategory];
        selectedTab.isOn = true;
    }
    
    protected override void UnregisterEvents()
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
        
        if (this.purchaseButton != null)
            this.purchaseButton.onClick.RemoveAllListeners();
    }
    
    private void SetupCategoryTabs()
    {
        // Setup category tab buttons if they exist
        for (int i = 0; i < this.categoryTabs.Length; i++)
        {
            int categoryIndex = i;
            var tab = this.categoryTabs[i];
            if (tab != null)
            {
                tab.onValueChanged.AddListener(isOn => { if (isOn) SwitchCategory((EShopCategory)categoryIndex); });
                tab.GetComponentInChildren<TextMeshProUGUI>().text = ((EShopCategory)categoryIndex).ToString();
            }
        }
    }
    
    public void SwitchCategory(EShopCategory category)
    {
        this.currentCategory = category;
        this.selectedItemData = null;
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
        print($"Refresh Items Grid");
        if (ShopManager.IN == null) return;
        
        // Clear existing items
        foreach (var item in currentItemDisplays)
        {
            if (item != null)
                Destroy(item);
        }
        currentItemDisplays.Clear();
            
        // Get items for current category
        var items = ShopManager.IN.GetItemsByCategory(this.currentCategory);
        
        print($"Refresh Items {items.Count}");
        
        // Create UI elements for each itemData
        foreach (var item in items)
        {
            if (item.IsUnlocked)
                CreateItemDisplay(item);
        }
    }
    
    private void CreateItemDisplay(ShopItemData itemData)
    {            
        var shopItemUI = Instantiate(shopItemPrefab, itemsGridParent);
        this.currentItemDisplays.Add(shopItemUI.gameObject);
        
        // Setup itemData display (this would be expanded with actual UI components)
        Button itemButton = shopItemUI.GetComponent<Button>();
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(() => SelectItem(itemData));
        }

        shopItemUI.Initialize(itemData);
    }
    
    private void SelectItem(ShopItemData itemData)
    {
        this.selectedItemData = itemData;
        ShowItemDetail();
    }
    
    private void ShowItemDetail()
    {
        if (this.itemDetailPanel != null)
            this.itemDetailPanel.SetActive(true);
            
        RefreshItemDetail();
    }
    
    private void HideItemDetail()
    {
        if (this.itemDetailPanel != null)
            this.itemDetailPanel.SetActive(false);
    }
    
    private void RefreshItemDetail()
    {
        if (selectedItemData == null)
            return;
            
        // Update itemData info
        if (this.itemNameText != null)
            this.itemNameText.text = this.selectedItemData.DisplayName;

        if (this.itemDescriptionText != null)
            this.itemDescriptionText.text = this.selectedItemData.Description;        if (this.itemIcon != null && this.selectedItemData.Icon != null)
            this.itemIcon.sprite = this.selectedItemData.Icon;
        
        // Update purchase button
        RefreshPurchaseButton();
        
        // Update cost display
        RefreshCostDisplay();
    }
    
    private void RefreshPurchaseButton()
    {
        if (this.purchaseButton == null || this.selectedItemData == null)
            return;
            
        bool canPurchase = this.selectedItemData.CanPurchase && 
                          (this.selectedItemData.Cost?.CanAfford(ResourceManager.IN) ?? false);
        
        this.purchaseButton.interactable = canPurchase;
        
        if (this.purchaseButtonText != null)
        {
            if (!this.selectedItemData.CanPurchase)
                this.purchaseButtonText.text = this.selectedItemData.IsMaxedOut ? "Max Purchased" : "Locked";
            else if (!canPurchase)
                this.purchaseButtonText.text = "Can't Afford";
            else
                this.purchaseButtonText.text = "Purchase";
        }
    }
    
    private void RefreshCostDisplay()
    {
        // Clear existing cost displays
        foreach (var costDisplay in this.currentCostDisplays)
        {
            if (costDisplay != null)
                Destroy(costDisplay);
        }
        this.currentCostDisplays.Clear();
        
        if (this.selectedItemData?.Cost == null || costDisplayParent == null || costItemPrefab == null)
            return;
            
        // Create cost displays
        foreach (var resource in this.selectedItemData.Cost.RequiredResources)
        {
            GameObject costObj = Instantiate(costItemPrefab, costDisplayParent);
            currentCostDisplays.Add(costObj);
            
            // Setup cost display (would need actual prefab components)
            var costText = costObj.GetComponentInChildren<TextMeshProUGUI>();
            if (costText != null)
            {
                bool hasEnough = ResourceManager.IN.HasResource(resource.Type, resource.Amount);
                string color = hasEnough ? "white" : "red";
                costText.text = $"<color={color}>{resource.Amount}\n{resource.Type}</color>";
                //TODO: add icon and make class for this
            }
        }
    }
    
    private void PurchaseSelectedItem()
    {
        if (this.selectedItemData != null)
        {
            ShopManager.IN.TryPurchaseItem(this.selectedItemData);
        }
    }
    
    private void OnItemPurchased(ShopItemData itemData)
    {
        // Refresh UI after purchase
        RefreshItemDetail();
        RefreshItemGrid();
        
        // TODO: Show purchase success feedback
    }
    
    private void OnPurchaseFailed(ShopItemData itemData, string reason)
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