using System;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager IN;

    public static int NumInventorySlots = 10;//TODO: link to InventoryManager and get value from Backpack, Chest, etc.

    public static Action OnInventoryRefreshed;

    [SerializeField] private InitInventoryItemData[] initInventoryItemDatas;

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

    public void AddDefaultItemsToInventory()
    {
        var sortedInitItemsList = this.initInventoryItemDatas.ToList();
        sortedInitItemsList.Sort((a, b) => a.ShopItemConfig.Category.CompareTo(b.ShopItemConfig.Category));

        foreach(var itemData in sortedInitItemsList)
        {
            itemData.DisplayName = itemData.ShopItemConfig.DisplayName;
            itemData.Category = itemData.ShopItemConfig.Category;
            itemData.IconSpriteName = itemData.ShopItemConfig.Icon != null ? itemData.ShopItemConfig.Icon.name : string.Empty;
            itemData.CanDragToWorld = itemData.ShopItemConfig.CanDragToWorld;
            itemData.DecorationData = DecorationData.Copy(itemData.ShopItemConfig.DecorationData);
        }

        for (int i = 0; i < sortedInitItemsList.Count; i++)
        {
            var itemData = sortedInitItemsList[i];
            if (i < NumInventorySlots)
            {
                SaveManager.Data.InventoryItems[i] = itemData as InventoryItemData;
            }
        }

        OnInventoryRefreshed?.Invoke();
    }

    public void AddSavedItemsToInventory(InventoryItemData[] savedAllInventoryItems)
    {
        SaveManager.Data.InventoryItems = new InventoryItemData[NumInventorySlots];
        Array.Copy(savedAllInventoryItems, SaveManager.Data.InventoryItems, NumInventorySlots);

        OnInventoryRefreshed?.Invoke();
    }

    public static bool TryAddItemToInventory(InventoryItemData itemData, bool isCheckOnly = false)
    {
        //create backup in case we can't distribute all elements and need to revert
        var backupAllItems = new InventoryItemData[SaveManager.Data.InventoryItems.Length];
        Array.Copy(SaveManager.Data.InventoryItems, backupAllItems, SaveManager.Data.InventoryItems.Length);
            
        // Implementation for adding item to inventory
        var itemIndex = 0;
        var foundEmptySlotIndex = -1;
        var initialItemQuantity = itemData.Quantity;

        for(int i = 0; i < SaveManager.Data.InventoryItems.Length; i++)
        {
            var searchItem = SaveManager.Data.InventoryItems[i];
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
        }
        else
        {
            //look for existing stack to add to
            for (int i = 0; i < SaveManager.Data.InventoryItems.Length; i++)
            {
                var searchItem = SaveManager.Data.InventoryItems[i];

                if (searchItem.DisplayName == itemData.DisplayName)
                {
                    if (searchItem.SpaceAvailableInStack <= 0)
                        continue;//stack is full, look for another stack or empty slot

                    if (itemData.Quantity <= searchItem.SpaceAvailableInStack)
                    {
                        if (isCheckOnly)
                        {
                            SaveManager.Data.InventoryItems = backupAllItems;
                            return true;
                        }
        
                        //found stack with enough space, add and exit
                        SaveManager.Data.InventoryItems[i].Quantity += itemData.Quantity;
                        OnInventoryRefreshed?.Invoke();
                        return true;
                    }
                    else
                    {
                        //found stack but not enough space, fill stack and keep looking for more stacks or empty slot
                        var quantityToAdd = searchItem.SpaceAvailableInStack;
                        SaveManager.Data.InventoryItems[i].Quantity += quantityToAdd;

                        itemData.Quantity -= quantityToAdd;
                    }
                }
            }

            //after trying to distribute across stacks, still have quantity left
            if (itemData.Quantity < initialItemQuantity)
            {
                if (isCheckOnly)
                {
                    SaveManager.Data.InventoryItems = backupAllItems;
                    return false;
                }
        
                //partial success
                OnInventoryRefreshed?.Invoke();
                return false;
            }
            else
            {
                //total failure
                SaveManager.Data.InventoryItems = backupAllItems;
                return false;
            }
        }
        
        if(isCheckOnly)
        {
            SaveManager.Data.InventoryItems = backupAllItems;
            return true;
        }

        SaveManager.Data.InventoryItems[itemIndex] = itemData;

        OnInventoryRefreshed?.Invoke();
        return true;
    }
    
    public void RemoveItemFromInventory(int itemIndex, int quantity = 1)
    {
        var itemData2 = SaveManager.Data.InventoryItems[itemIndex];
        if (itemData2 != null)
        {
            itemData2.Quantity -= quantity;
            if (itemData2.Quantity <= 0)
            {
                SaveManager.Data.InventoryItems[itemIndex] = null;
            }
        }

        OnInventoryRefreshed?.Invoke();
    }

    public InventoryItemData[] GetItemsByCategory(EShopCategory category)
    {
        var itemsOfCategory = new InventoryItemData[NumInventorySlots];
        if (category == EShopCategory.All)
        {
            return SaveManager.Data.InventoryItems;
        }
        else
        {
            int index = 0;
            for (int i = 0; i < SaveManager.Data.InventoryItems.Length; i++)
            {
                var itemData = SaveManager.Data.InventoryItems[i];
                if (itemData != null && itemData.Category == category)
                {
                    itemsOfCategory[index] = itemData;
                    index++;
                }
            }
        }
        return itemsOfCategory;
    }

    public void LoadAllInventory(InventoryItemData[] saveData)
    {
        OnInventoryRefreshed?.Invoke();
    }

    private void OnShopItemPurchased(ShopItemData itemData)
    {
        // Handle adding purchased item to inventory    
        var inventoryItemData = ShopItemData.ToInventoryItemData(itemData);

        TryAddItemToInventory(inventoryItemData);
    }
}