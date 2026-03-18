using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager IN;

    public static int NumInventorySlots = 20;//TODO: link to InventoryManager and get value from Backpack, Chest, etc.

    public static Action OnInventoryRefreshed;

    [SerializeField] private InitInventoryItemData[] initInventoryItemDatas;

    private Dictionary<EShopCategory, InventoryItemData[]> itemsByCategory = new();

    private int numShopCategories = Enum.GetValues(typeof(EShopCategory)).Length;

    private void OnValidate()
    {
        foreach (var itemData in this.initInventoryItemDatas)
        {
            if (itemData != null && itemData.ShopItemConfig != null)
            {
                itemData.DisplayName = itemData.ShopItemConfig.DisplayName;
                itemData.Category = itemData.ShopItemConfig.Category;
                itemData.IconSpriteName = itemData.ShopItemConfig.Icon != null ? itemData.ShopItemConfig.Icon.name : string.Empty;
                itemData.CanDragToWorld = itemData.ShopItemConfig.CanDragToWorld;
                itemData.DecorationData = DecorationData.Copy(itemData.ShopItemConfig.DecorationData);
            }
        }
    }

    private void Start()
    {
        ShopManager.OnItemPurchased += OnShopItemPurchased;
    }

    private void OnDestroy()
    {
        ShopManager.OnItemPurchased -= OnShopItemPurchased;
    }

    public void InitInventoryDict()
    {
        this.itemsByCategory.Clear();
        foreach (EShopCategory category in Enum.GetValues(typeof(EShopCategory)))
        {
            this.itemsByCategory[category] = new InventoryItemData[NumInventorySlots];
        }
    }

    public void AddDefaultItemsToInventory()
    {
        foreach (EShopCategory category in Enum.GetValues(typeof(EShopCategory)))
        {
            var slotCounter = 0;
            for (int i = 0; i < this.initInventoryItemDatas.Length; i++)
            {
                var itemData = this.initInventoryItemDatas[i];
                if (itemData != null && itemData.Category == category && slotCounter < NumInventorySlots)
                {
                    this.itemsByCategory[category][slotCounter] = InventoryItemData.Copy(itemData as InventoryItemData);
                    ++slotCounter;
                }
            }
        }

        for (int i = 0; i < this.initInventoryItemDatas.Length; i++)
        {
            var itemData = this.initInventoryItemDatas[i];
            if (i < NumInventorySlots)
            {
                SaveManager.Data.AllInventoryItems[i] = itemData as InventoryItemData;
            }
        }

        OnInventoryRefreshed?.Invoke();
    }

    public void AddSavedItemsToInventory(Dictionary<string, InventoryItemData[]> savedInventoryData, InventoryItemData[] savedAllInventoryItems)
    {
        foreach (var kvp in savedInventoryData)
        {
            if (Enum.TryParse<EShopCategory>(kvp.Key, out var category))
            {
                this.itemsByCategory[category] = kvp.Value;
            }
        }

        SaveManager.Data.AllInventoryItems = new InventoryItemData[NumInventorySlots];
        Array.Copy(savedAllInventoryItems, SaveManager.Data.AllInventoryItems, NumInventorySlots);

        OnInventoryRefreshed?.Invoke();
    }

    public bool AddItemToInventory(InventoryItemData itemData)
    {
        // Implementation for adding item to inventory
        var itemIndex = 0;
        var foundEmptySlotIndex = -1;
        var initialItemQuantity = itemData.Quantity;

        var itemsOfCategory = new InventoryItemData[0];

        for(int i = 0; i < SaveManager.Data.AllInventoryItems.Length; i++)
        {
            var searchItem = SaveManager.Data.AllInventoryItems[i];
            if (searchItem == null)
            {
                //found an empty slot
                foundEmptySlotIndex = i;
                break;
            }
        }

        if (foundEmptySlotIndex > -1)
        {
            //found an empty slot so skip to bottom
            itemIndex = foundEmptySlotIndex;
            itemsOfCategory = this.itemsByCategory[itemData.Category];
        }
        else
        {
            //all slots full, check for stackable item
            if (itemData.MaxStack.Equals(1))
            {
                //not stackable and no empty slots, can't add
                return false;
            }

            //look for existing stack to add to
            itemsOfCategory = this.itemsByCategory[itemData.Category];

            //create backup in case we can't distribute all elements and need to revert
            var backupItemsOfCategory = new InventoryItemData[itemsOfCategory.Length];
            Array.Copy(itemsOfCategory, backupItemsOfCategory, itemsOfCategory.Length);

            for (int i = 0; i < itemsOfCategory.Length; i++)
            {
                var searchItem = itemsOfCategory[i];
                if (searchItem.DisplayName == itemData.DisplayName)
                {
                    if (itemData.Quantity <= searchItem.SpaceAvailableInStack)
                    {
                        //found stack with enough space, add and exit
                        searchItem.Quantity += itemData.Quantity;
                        SaveManager.Data.AllInventoryItems[i] = InventoryItemData.Copy(searchItem);
                        itemsOfCategory[i] = InventoryItemData.Copy(searchItem);
                        OnInventoryRefreshed?.Invoke();
                        return true;
                    }
                    else
                    {
                        //found stack but not enough space, fill stack and keep looking for more stacks or empty slot
                        var quantityToAdd = searchItem.SpaceAvailableInStack;
                        searchItem.Quantity += quantityToAdd;
                        itemData.Quantity -= quantityToAdd;

                        SaveManager.Data.AllInventoryItems[i] = InventoryItemData.Copy(searchItem);
                        itemsOfCategory[i] = InventoryItemData.Copy(searchItem);
                    }
                }
            }

            //after trying to distribute across stacks, still have quantity left
            if (itemData.Quantity < initialItemQuantity)
            {
                //partial success
                OnInventoryRefreshed?.Invoke();
                return true;
            }
            else
            {
                //total failure
                this.itemsByCategory[itemData.Category] = backupItemsOfCategory;
                return false;
            }
        }

        SaveManager.Data.AllInventoryItems[itemIndex] = itemData;
        itemsOfCategory[itemIndex] = itemData;

        OnInventoryRefreshed?.Invoke();
        return true;
    }
    
    public void RemoveItemFromInventory(EShopCategory category, int itemIndex, int quantity = 1)
    {
        // Implementation for removing item from inventory
        if (this.itemsByCategory.TryGetValue(category, out var itemsOfCategory))
        {
            if (itemsOfCategory != null && itemIndex >= 0 && itemIndex < itemsOfCategory.Length)
            {
                var itemData = itemsOfCategory[itemIndex];
                if (itemData != null)
                {
                    itemData.Quantity -= quantity;
                    if (itemData.Quantity <= 0)
                    {
                        itemsOfCategory[itemIndex] = null;
                    }
                }
            }
        }

        var itemData2 = SaveManager.Data.AllInventoryItems[itemIndex];
        if (itemData2 != null)
        {
            itemData2.Quantity -= quantity;
            if (itemData2.Quantity <= 0)
            {
                SaveManager.Data.AllInventoryItems[itemIndex] = null;
            }
        }

        OnInventoryRefreshed?.Invoke();
    }

    public InventoryItemData[] GetItemsByCategory(EShopCategory category)
    {
        // Implementation for retrieving items by category
        if (this.itemsByCategory.ContainsKey(category))
        {
            return this.itemsByCategory[category];
        }
        return new InventoryItemData[0];
    }

    public InventoryItemData[] GetNonResourceItems()
    {
        var nonResourceItems = new List<InventoryItemData>();
        foreach (EShopCategory category in Enum.GetValues(typeof(EShopCategory)))
        {
            if (category != EShopCategory.Resources && this.itemsByCategory.ContainsKey(category))
            {
                nonResourceItems.AddRange(this.itemsByCategory[category]);
            }
        }
        return nonResourceItems.ToArray();
    }

    // For save system
    public void CreateDictFromSaveData(Dictionary<string, InventoryItemData[]> saveData)
    {
        InitInventoryDict();
        foreach (var kvp in saveData)
        {
            if (Enum.TryParse<EShopCategory>(kvp.Key, out var category))
            {
                this.itemsByCategory[category] = kvp.Value;
            }
        }
    }

    public void LoadAllInventory(InventoryItemData[] saveData)
    {
        OnInventoryRefreshed?.Invoke();
    }

    public Dictionary<string, InventoryItemData[]> GetSaveData()
    {
        var saveData = new Dictionary<string, InventoryItemData[]>();
        foreach (var kvp in this.itemsByCategory)
        {
            saveData[kvp.Key.ToString()] = kvp.Value;
        }
        return saveData;
    }

    private void OnShopItemPurchased(ShopItemData itemData)
    {
        // Handle adding purchased item to inventory    
        var inventoryItemData = ShopItemData.ToInventoryItemData(itemData);

        AddItemToInventory(inventoryItemData);
    }
}