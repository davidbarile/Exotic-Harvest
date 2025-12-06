using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager IN;

    public static event Action OnInventoryRefreshed;

    /// <summary>
    /// Categories for organizing shop items
    /// </summary>
    public enum EItemCategory
    {
        Decorations,
        Resources,
        Tools,
        Special
    }

    public ShopItemConfig[] StartingShopItemConfigs;

    public void AddDefaultItemsToInventory()
    {
        // Implementation for adding default items to inventory
        foreach (var itemConfig in StartingShopItemConfigs)
        {
            //UiManager.IN.InventoryPanel.AddItemToInventory(itemConfig);
        }

        OnInventoryRefreshed?.Invoke();
    }

    public ShopItem[] GetItemsByCategory(EItemCategory category)
    {
        // Implementation for retrieving items by category
        // This is a placeholder implementation
        return Array.Empty<ShopItem>();
    }
}