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
    [SerializeField] private UiInventoryItem inventoryItemPrefab;

    [Header("Item Detail Panel")]
    [SerializeField] private GameObject itemDetailPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Image itemIcon;

    [Header("Inventory Stats")]

    private EInventoryCategory currentCategory = EInventoryCategory.Decorations;
    private InventoryItemData selectedItemData;
    private List<UiInventoryCell> allInventoryCells = new();

    private bool isInitialized;
    
    protected override void RegisterEvents()
    {
        if (!this.isInitialized)
            SetupCategoryTabs();
            
        if (InventoryManager.IN != null)
        {
            InventoryManager.OnInventoryRefreshed += RefreshInventory;
        }

        RefreshInventory();
    }

    protected override void UnregisterEvents()
    {
        if (InventoryManager.IN != null)
        {
            InventoryManager.OnInventoryRefreshed -= RefreshInventory;
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
                tab.onValueChanged.AddListener(isOn => { if (isOn) SwitchCategory((EInventoryCategory)categoryIndex); });
                tab.GetComponentInChildren<TextMeshProUGUI>().text = ((EInventoryCategory)categoryIndex).ToString();
            }
        }

        var selectedTab = this.categoryTabs[(int)this.currentCategory];
        selectedTab.isOn = true;

        this.isInitialized = true;
    }

    public void SwitchCategory(EInventoryCategory category)
    {
        if (this.currentCategory == category)
            return;

        if (UiInventoryCell.SelectedCell != null)
            UiInventoryCell.SelectedCell.SetSelected(false);
            
        UiInventoryCell.SelectedCell = null;
        
        this.currentCategory = category;
        this.selectedItemData = null;
        
        RefreshItemGrid(false);
        HideItemDetail();
    }

      private void RefreshInventory()
    {
        RefreshItemGrid(false);
        RefreshItemDetail();
    }

    private void RefreshItemGrid(bool shouldRecreateCells)
    {
        if (!this.isInitialized)
            return;

        if (shouldRecreateCells || this.allInventoryCells.Count == 0)
            CreateItemGrid();

        // Clear existing items
        foreach (var item in this.allInventoryCells)
        {
            if (item != null)
                item.ClearItem();
        }

        // Get items for current category
        var itemsArray = InventoryManager.IN.GetItemsByCategory(currentCategory);

        // // Create UI elements for each itemData
        for (int i = 0; i < InventoryManager.NumInventorySlots; i++)
        {
            var itemData = itemsArray[i];
            if (itemData != null)
            {
                var cell = this.allInventoryCells[i];
                var prefab = Instantiate(this.inventoryItemPrefab, cell.Container);
                prefab.name = $"Item_{itemData.DisplayName}";
                this.allInventoryCells[i].AddItem(prefab, itemData);
            }
        }
    }

    private void CreateItemGrid()
    {
        ClearItemGrid();

        for (int i = 0; i < InventoryManager.NumInventorySlots; i++)
        {
            var cell = Instantiate(this.inventoryCellPrefab, this.itemsGridParent);
            cell.name = $"Cell_{i}";
            this.allInventoryCells.Add(cell);
        }
    }

    private void ClearItemGrid()
    {
        foreach (var item in this.allInventoryCells)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        this.allInventoryCells.Clear();
    }
    
    private void SelectItem(InventoryItemData itemData)
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
        if (this.selectedItemData == null)
            return;

        // Update itemData info
        if (this.itemNameText != null)
            this.itemNameText.text = this.selectedItemData.DisplayName;

        this.itemIcon.sprite = SpriteManager.GetSprite(this.selectedItemData.IconSpriteName);
    }
}