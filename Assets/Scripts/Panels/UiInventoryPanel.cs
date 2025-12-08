using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static InventoryManager;

public class UiInventoryPanel : UIPanelBase
{
    [Header("Shop UI Elements")]
    [SerializeField] private Transform categoryTabsParent;
    [SerializeField] private Transform itemsGridParent;
    [SerializeField] private Toggle[] categoryTabs;
    [SerializeField] private UiInventoryCell inventoryCellPrefab;

    [Header("Item Detail Panel")]
    [SerializeField] private GameObject itemDetailPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Image itemIcon;

    [Header("Inventory Stats")]
    [SerializeField] private int numInventorySlots = 20;//TODO: link to InventoryManager and get value from Backpack, Chest, etc.

    private EItemCategory currentCategory = EItemCategory.Decorations;
    private ShopItem selectedItem;
    private List<GameObject> currentItemDisplays = new();

    private void Start()
    {
        SetupCategoryTabs();
        SetupEventListeners();
        RefreshInventory();
    }
    
    private void OnEnable()
    {
        if (InventoryManager.IN != null)
        {
            // InventoryManager.OnItemPurchased += OnItemPurchased;
            // InventoryManager.OnPurchaseFailed += OnPurchaseFailed;
            InventoryManager.OnInventoryRefreshed += RefreshInventory;
        }

        if (ResourceManager.IN != null)
        {
            ResourceManager.OnResourceChanged += OnResourceChanged;
        }

        RefreshInventory();
    }

    private void OnDisable()
    {
        if (InventoryManager.IN != null)
        {
            // InventoryManager.OnItemPurchased -= OnItemPurchased;
            // InventoryManager.OnPurchaseFailed -= OnPurchaseFailed;
            InventoryManager.OnInventoryRefreshed -= RefreshInventory;
        }

        if (ResourceManager.IN != null)
        {
            ResourceManager.OnResourceChanged -= OnResourceChanged;
        }
    }
    
    private void SetupCategoryTabs()
    {
        // Setup category tab buttons if they exist
        for (int i = 0; i < categoryTabs.Length; i++)
        {
            int categoryIndex = i;
            var tab = categoryTabs[i];
            if (tab != null)
            {
                tab.onValueChanged.AddListener(isOn => { if (isOn) SwitchCategory((EItemCategory)categoryIndex); });
                tab.GetComponentInChildren<TextMeshProUGUI>().text = ((EItemCategory)categoryIndex).ToString();
            }
        }

        var selectedTab = this.categoryTabs[(int)this.currentCategory];
        selectedTab.isOn = true;
    }
    
    private void SetupEventListeners()
    {
        
    }

    public void SwitchCategory(EItemCategory category)
    {
        this.currentCategory = category;
        this.selectedItem = null;
        RefreshItemGrid();
        HideItemDetail();
    }

      private void RefreshInventory()
    {
        RefreshItemGrid();
        RefreshItemDetail();
    }
    
    private void RefreshItemGrid()
    {
        if (InventoryManager.IN == null) return;
        
        // Clear existing items
        foreach (var item in this.currentItemDisplays)
        {
            if (item != null)
                Destroy(item);
        }
        this.currentItemDisplays.Clear();
            
        // Get items for current category
        var items = InventoryManager.IN.GetItemsByCategory(currentCategory);
        
        // // Create UI elements for each item
        foreach (var item in items)
        {
            if (item.IsUnlocked)
                CreateItemDisplay(item);
        }
    }
    
    private void CreateItemDisplay(ShopItem item)
    {            
        var invCellItem = Instantiate(this.inventoryCellPrefab, this.itemsGridParent);
        this.currentItemDisplays.Add(invCellItem.gameObject);
        
        // Setup item display (this would be expanded with actual UI components)
        Button itemButton = invCellItem.GetComponent<Button>();
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(() => SelectItem(item));
        }

        // invCellItem.Initialize(item);
    }
    
    private void SelectItem(ShopItem item)
    {
        this.selectedItem = item;
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
        if (this.selectedItem == null)
            return;

        // Update item info
        if (this.itemNameText != null)
            this.itemNameText.text = this.selectedItem.DisplayName;

        if (this.itemDescriptionText != null)
            this.itemDescriptionText.text = this.selectedItem.Description;

        if (this.itemIcon != null && this.selectedItem.Icon != null)
            this.itemIcon.sprite = this.selectedItem.Icon;
    }
    
    private void OnResourceChanged(ResourceType type, int newAmount)
    {
        
    }
}