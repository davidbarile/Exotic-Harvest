using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiShopPanel : UIPanelBase
{
    [Header("Shop UI Elements")]
    [SerializeField] private Transform itemsGridParent;
    [SerializeField] private Toggle[] categoryTabs;
    [Header("Add Categories Here")]
    [SerializeField] private EShopCategory[] categoryTabMapping; // Maps toggle index to shop category
    
    [Header("Item Detail Panel")]
    [SerializeField] private GameObject itemDetailPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private GameObject[] itemIconDisplayObjects;
    [SerializeField] private ShopItemIconDisplayUI[] itemIconDisplays;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TextMeshProUGUI purchaseButtonText;
    [SerializeField] private ResourceDisplayUI[] buyButtonCostDisplays;
    
    private EShopCategory currentCategory = EShopCategory.Tools;
    private ShopItemData selectedItemData;
    private List<GameObject> currentItemDisplays = new();

    private bool isInitialized;
    
    public override void Show()
    {
        base.Show();
        if (!this.isInitialized)
        {
            SetupCategoryTabs();
            this.isInitialized = true;
            RefreshShop();
        }

        this.selectedItemData = null;
        HideItemDetail();
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
    }
    
    private void SetupCategoryTabs()
    {
        // Setup category tab buttons if they exist
        for (int i = 0; i < this.categoryTabs.Length; i++)
        {
            int categoryIndex = i;
            var tab = this.categoryTabs[i];
            var shouldShow = categoryIndex < this.categoryTabMapping.Length;
            tab.gameObject.SetActive(shouldShow);
            if (shouldShow)
            {
                tab.onValueChanged.AddListener(isOn => { if (isOn) SwitchCategory(this.categoryTabMapping[categoryIndex]); });
                tab.GetComponentInChildren<TextMeshProUGUI>().text = this.categoryTabMapping[categoryIndex].ToString();
            }
        }
    }
    
    public void SwitchCategory(EShopCategory category)
    {
        Debug.Log($"Switching to category: {category}");
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
        
        // Create UI elements for each itemData
        foreach (var item in items)
        {
            CreateItemDisplay(item);
        }
    }
    
    private void CreateItemDisplay(ShopItemData itemData)
    {            
        var shopItemUI = PrefabManager.IN.SpawnPrefab<ShopItemUI>("ShopItemUI", this.itemsGridParent);
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
        this.itemDetailPanel.SetActive(true);
            
        RefreshItemDetail();
    }
    
    private void HideItemDetail()
    {
        this.itemDetailPanel.SetActive(false);
    }
    
    private void RefreshItemDetail()
    {
        if (this.selectedItemData == null)
        {
            HideItemDetail();
            return;
        }

        // Update itemData name and description
        this.itemNameText.text = this.selectedItemData.DisplayName;
        this.itemDescriptionText.text = this.selectedItemData.Description;

        //hide all icon displays initially
        foreach (var iconDisplayObject in this.itemIconDisplayObjects)
        {
            iconDisplayObject.SetActive(false);
        }
        
        //show icons and quantity based on number of resources if it's a resource item, otherwise show single icon
        if(this.selectedItemData.IsResource)
        {
            this.itemIconDisplayObjects[this.selectedItemData.ResourceItems.Length - 1].SetActive(true);

            var indexOffset = this.selectedItemData.ResourceItems.Length - 1;

            if (this.selectedItemData.ResourceItems.Length == 3)
                indexOffset = 3;
            else if (this.selectedItemData.ResourceItems.Length == 4)
                indexOffset = 6;
                
            for(int i = 0; i < this.selectedItemData.ResourceItems.Length && i < this.itemIconDisplays.Length; i++)
            {
                var resourceItem = this.selectedItemData.ResourceItems[i];
                var iconDisplay = this.itemIconDisplays[i + indexOffset];
                var resourceData = ResourceManager.IN.Database.GetResource(resourceItem.ResourceType);
                iconDisplay.Configure(resourceData.Icon, resourceItem.Amount, resourceData.IconColor, $"{resourceItem.ResourceType}");
            }
        }
        else
        {
            this.itemIconDisplayObjects[0].SetActive(true);
            var iconDisplay = this.itemIconDisplays[0];
            iconDisplay.Configure(this.selectedItemData.Icon, 1, this.selectedItemData.IconColor);
        }
        
        // Update purchase button
        RefreshPurchaseButton();
        
        // Update cost display
        RefreshCostDisplay();
    }
    
    private void RefreshPurchaseButton()
    {
        if (this.selectedItemData == null)
            return;

        bool hasInventorySpace = InventoryManager.TryAddItemToInventory(ShopItemData.ToInventoryItemData(this.selectedItemData), true);
        
        bool canPurchase = hasInventorySpace && this.selectedItemData.CanPurchase && (this.selectedItemData.Cost?.CanAfford() ?? false);
       
        this.purchaseButton.interactable = canPurchase;
        
        if (this.purchaseButtonText != null)
        {
            if (!this.selectedItemData.CanPurchase)
                this.purchaseButtonText.text = "Max Purchased";
            else if (!hasInventorySpace)
                this.purchaseButtonText.text = "No Inventory Space";
            else if (!canPurchase)
                this.purchaseButtonText.text = "Can't Afford";
            else
                this.purchaseButtonText.text = "Purchase";
        }
    }
    
    private void RefreshCostDisplay()
    {
        foreach (var costDisplay in this.buyButtonCostDisplays)
        {
            costDisplay.gameObject.SetActive(false);
        }
        
        if (this.selectedItemData?.Cost == null)
            return;
            
        for(int i = 0; i < this.buyButtonCostDisplays.Length; i++)
        {
            var costDisplay = this.buyButtonCostDisplays[i];
            var shouldShow = i < this.selectedItemData.Cost.RequiredResources.Count;
            costDisplay.gameObject.SetActive(shouldShow);
            if (shouldShow)
            {
                var resourceCost = this.selectedItemData.Cost.RequiredResources[i];
                costDisplay.Configure(resourceCost.Type, resourceCost);
            }
        }
    }
    
    public void PurchaseSelectedItem()
    {
        if (this.selectedItemData == null)
            return;

        if (!this.purchaseButton.interactable)
            return;//blocks sub elements from triggering purchase if button is disabled

        ShopManager.IN.TryPurchaseItem(this.selectedItemData);
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
    
    private void OnResourceChanged(EResourceType type, int newAmount)
    {
        // Update purchase button state when resources change
        RefreshPurchaseButton();
        RefreshCostDisplay();
    }
}