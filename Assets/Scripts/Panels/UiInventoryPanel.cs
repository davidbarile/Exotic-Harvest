using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private EShopCategory currentCategory = EShopCategory.Decorations;
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
                tab.onValueChanged.AddListener(isOn => { if (isOn) SwitchCategory((EShopCategory)categoryIndex); });
                tab.GetComponentInChildren<TextMeshProUGUI>().text = ((EShopCategory)categoryIndex).ToString();
            }
        }

        var selectedTab = categoryTabs[(int)currentCategory];
        selectedTab.isOn = true;
    }
    
    private void SetupEventListeners()
    {
        
    }

    public void SwitchCategory(EShopCategory category)
    {
        currentCategory = category;
        selectedItem = null;
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
        // Clear existing items
        foreach (var item in currentItemDisplays)
        {
            if (item != null)
                Destroy(item);
        }
        currentItemDisplays.Clear();
            
        // Get items for current category
        // var items = InventoryManager.IN.GetItemsByCategory(currentCategory);
        
        // // Create UI elements for each item
        // foreach (var item in items)
        // {
        //     if (item.isUnlocked)
        //         CreateItemDisplay(item);
        // }
    }
    
    private void CreateItemDisplay(ShopItem item)
    {            
        var invCellItem = Instantiate(inventoryCellPrefab, itemsGridParent);
        currentItemDisplays.Add(invCellItem.gameObject);
        
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
    }
    
    private void OnResourceChanged(ResourceType type, int newAmount)
    {
        
    }
}