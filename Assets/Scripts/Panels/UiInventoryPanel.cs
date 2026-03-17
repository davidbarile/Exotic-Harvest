using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static InventoryManager;

public class UiInventoryPanel : UIPanelBase
{
    public static Action<bool> OnDragOutOfInventoryZoneActiveChanged;

    [Header("Shop UI Elements")]
    [SerializeField] private Transform categoryTabsParent;
    [SerializeField] private Transform itemsGridParent;
    [SerializeField] private Toggle[] categoryTabs;

    [Header("Item Detail Panel")]
    [SerializeField] private GameObject itemDetailPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Image itemIcon;

    [Header("Inventory Stats")]
    [SerializeField] private EShopCategory[] categoriesToShow; // Which categories to display in inventory
    private EShopCategory currentCategory = EShopCategory.All;
    private InventoryItemData selectedItemData;
    private List<UiInventoryCell> allInventoryCells = new();

    [Header("Misc")]
    [SerializeField] private GameObject dragOutOfInventoryZone;

    private bool isInitialized;

    public override void Show()
    {
        base.Show();
        SetDragOutOfInventoryZoneActive(false);
    }
    
    protected override void RegisterEvents()
    {
        if (!this.isInitialized)
            SetupCategoryTabs();
            
        if (InventoryManager.IN != null)
        {
            InventoryManager.OnInventoryRefreshed += RefreshInventory;
        }

        RefreshInventory();

        OnDragOutOfInventoryZoneActiveChanged += SetDragOutOfInventoryZoneActive;
    }

    protected override void UnregisterEvents()
    {
        if (InventoryManager.IN != null)
        {
            InventoryManager.OnInventoryRefreshed -= RefreshInventory;
        }

        OnDragOutOfInventoryZoneActiveChanged -= SetDragOutOfInventoryZoneActive;
    }
    
    private void SetupCategoryTabs()
    {
        // Setup category tab buttons if they exist
        for (int i = 0; i < this.categoryTabs.Length; i++)
        {
            var tab = this.categoryTabs[i];
            var shouldShow = i < this.categoriesToShow.Length;
            tab.gameObject.SetActive(shouldShow);

            if(shouldShow)
            {
                var category = this.categoriesToShow[i];
                tab.onValueChanged.AddListener(isOn => { if (isOn) SwitchCategory(category); });
                tab.GetComponentInChildren<TextMeshProUGUI>().text = category.ToString();
            }
        }

        var selectedTab = this.categoryTabs[0];//this.categoryTab_Items;
        selectedTab.isOn = true;

        this.isInitialized = true;
    }

    public void SwitchCategory(EShopCategory category)
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
                item.ClearItem(true);
        }

        var itemsArray = new InventoryItemData[NumInventorySlots];

        if (this.currentCategory == EShopCategory.All)
        {
            itemsArray = SaveManager.Data.AllInventoryItems;
        }
        else
        {
            itemsArray = InventoryManager.IN.GetItemsByCategory(this.currentCategory);
        }

        // // Create UI elements for each itemData
        for (int i = 0; i < InventoryManager.NumInventorySlots; i++)
        {
            var itemData = itemsArray[i];
            if (itemData != null)
            {
                SpawnInventoryItemInCell(itemData, i);
            }
        }
    }

    public void SpawnInventoryItemInCell(InventoryItemData itemData, int cellIndex)
    {
        if (cellIndex < 0 || cellIndex >= this.allInventoryCells.Count)
            return;

        var cell = this.allInventoryCells[cellIndex];
        var prefab = PrefabManager.IN.SpawnPrefab<UiInventoryItem>($"InventoryItemUI", cell.Container);
        prefab.name = $"Item_{itemData.DisplayName}";
        cell.AddItem(prefab, itemData);
    }

    public UiInventoryCell GetFirstEmptyCell()
    {
        foreach (var cell in this.allInventoryCells)
        {
            if (cell.Item == null)
                return cell;
        }
        return null;
    }
    
    public UiInventoryCell GetFirstCellWithSpace(InventoryItemData itemData)
    {
        foreach (var cell in this.allInventoryCells)
        {
            if (cell.Item != null && cell.Item.ItemData != null && cell.Item.ItemData.DisplayName == itemData.DisplayName && cell.Item.ItemData.Quantity < cell.Item.ItemData.MaxStack)
                return cell;
        }
        return null;
    }

    private void CreateItemGrid()
    {
        ClearItemGrid();

        for (int i = 0; i < InventoryManager.NumInventorySlots; i++)
        {
            var cell = PrefabManager.IN.SpawnPrefab<UiInventoryCell>($"InventoryCellUI", this.itemsGridParent);
            cell.name = $"Cell_{i}";
            cell.CellIndex = i;
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

    private void SetDragOutOfInventoryZoneActive(bool isActive)
    {
        this.dragOutOfInventoryZone.SetActive(isActive);
    }
}