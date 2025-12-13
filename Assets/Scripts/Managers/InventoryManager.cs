using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager IN;

    public static int NumInventorySlots = 20;//TODO: link to InventoryManager and get value from Backpack, Chest, etc.

    public static event Action OnInventoryRefreshed;

    /// <summary>
    /// Categories for organizing shop items
    /// </summary>
    public enum EInventoryCategory
    {
        Decorations,
        Tools,
        Resources,
        Special,
        Items
    }

    public InventoryItemData[] StartingInventoryItemDatas;//TODO: change to config

    private Dictionary<EInventoryCategory, InventoryItemData[]> itemsByCategory = new();
    private InventoryItemData[] allInventoryItems = new InventoryItemData[NumInventorySlots];

    public void InitInventoryDict()
    {
        this.itemsByCategory.Clear();
        foreach (EInventoryCategory category in Enum.GetValues(typeof(EInventoryCategory)))
        {
            this.itemsByCategory[category] = new InventoryItemData[InventoryManager.NumInventorySlots];
        }
    }

    public void AddDefaultItemsToInventory()
    {
        foreach (EInventoryCategory category in Enum.GetValues(typeof(EInventoryCategory)))
        {
            var slotCounter = 0;
            for (int i = 0; i < this.StartingInventoryItemDatas.Length; i++)
            {
                var itemData = this.StartingInventoryItemDatas[i];
                if (itemData != null && itemData.Category == category && slotCounter < InventoryManager.NumInventorySlots)
                {
                    this.itemsByCategory[category][slotCounter] = itemData;
                    ++slotCounter;
                }
            }
        }

        for (int i = 0; i < this.StartingInventoryItemDatas.Length; i++)
        {
            var itemData = this.StartingInventoryItemDatas[i];
            if (i < InventoryManager.NumInventorySlots)
            {
                this.allInventoryItems[i] = itemData;
            }
        }

        OnInventoryRefreshed?.Invoke();
    }

    public void AddSavedItemsToInventory(Dictionary<string, InventoryItemData[]> savedInventoryData, InventoryItemData[] savedAllInventoryItems)
    {
        foreach (var kvp in savedInventoryData)
        {
            if (Enum.TryParse<EInventoryCategory>(kvp.Key, out var category))
            {
                this.itemsByCategory[category] = kvp.Value;
            }
        }

        this.allInventoryItems = new InventoryItemData[NumInventorySlots];
        Array.Copy(savedAllInventoryItems, this.allInventoryItems, NumInventorySlots);

        OnInventoryRefreshed?.Invoke();
    }

    public void AddItemToInventory(InventoryItemData itemData, EInventoryCategory category, int itemIndex, int quantity = 1)
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

        this.allInventoryItems[itemIndex] = itemData;

        OnInventoryRefreshed?.Invoke();
    }
    
    public void RemoveItemFromInventory(EInventoryCategory category, int itemIndex, int quantity = 1)
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

        var itemData2 = this.allInventoryItems[itemIndex];
        if (itemData2 != null)
        {
            itemData2.Quantity -= quantity;
            if (itemData2.Quantity <= 0)
            {
                this.allInventoryItems[itemIndex] = null;
            }
        }

        OnInventoryRefreshed?.Invoke();
    }

    public InventoryItemData[] GetItemsByCategory(EInventoryCategory category)
    {
        // Implementation for retrieving items by category
        if (this.itemsByCategory.ContainsKey(category))
        {
            return this.itemsByCategory[category];
        }
        return new InventoryItemData[0];
    }

    // For save system
    public void LoadSaveData(Dictionary<string, InventoryItemData[]> saveData)
    {
        InitInventoryDict();
        foreach (var kvp in saveData)
        {
            if (Enum.TryParse<EInventoryCategory>(kvp.Key, out var category))
            {
                this.itemsByCategory[category] = kvp.Value;
            }
        }

        print("Inventory loaded from save data.");

        OnInventoryRefreshed?.Invoke();
    }

    public void LoadAllInventory(InventoryItemData[] saveData)
    {
        this.allInventoryItems = new InventoryItemData[NumInventorySlots];
        Array.Copy(saveData, this.allInventoryItems, NumInventorySlots);

        print("Inventory loaded from save data.");

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
    
    public InventoryItemData[] GetAllInventoryItems()
    {
        return this.allInventoryItems;
    }
}