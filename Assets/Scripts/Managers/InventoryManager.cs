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

    public void InitInventoryDict()
    {
        this.itemsByCategory.Clear();
        foreach (EShopCategory category in Enum.GetValues(typeof(EShopCategory)))
        {
            this.itemsByCategory[category] = new InventoryItemData[InventoryManager.NumInventorySlots];
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
                if (itemData != null && itemData.Category == category && slotCounter < InventoryManager.NumInventorySlots)
                {
                    this.itemsByCategory[category][slotCounter] = InventoryItemData.Copy(itemData as InventoryItemData);
                    ++slotCounter;
                }
            }
        }

        for (int i = 0; i < this.initInventoryItemDatas.Length; i++)
        {
            var itemData = this.initInventoryItemDatas[i];
            if (i < InventoryManager.NumInventorySlots)
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

    public void AddItemToInventory(InventoryItemData itemData, EShopCategory category, int itemIndex, int quantity = 1)
    {
        // Implementation for adding item to inventory
        if (this.itemsByCategory.TryGetValue(category, out var itemsOfCategory))
        {
            if (itemsOfCategory != null && itemIndex >= 0 && itemIndex < itemsOfCategory.Length)
            {
                if (itemsOfCategory[itemIndex] != null)
                {
                    itemsOfCategory[itemIndex].Quantity += quantity;
                }
                else
                {
                    itemsOfCategory[itemIndex] = itemData;
                }
            }
        }

        SaveManager.Data.AllInventoryItems[itemIndex] = itemData;

        OnInventoryRefreshed?.Invoke();
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
}