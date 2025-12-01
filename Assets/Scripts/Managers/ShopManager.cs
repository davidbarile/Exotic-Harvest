using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the shop system, items, and purchasing
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager IN;
    
    [Header("Shop Configuration")]
    [SerializeField] private ShopItemDefinition[] allShopItemDefinitions;
    [SerializeField] private bool debugMode;
    
    private List<ShopItem> allShopItems = new();
    
    private Dictionary<string, ShopItem> shopItemsById;
    private Dictionary<EShopCategory, List<ShopItem>> itemsByCategory;
    
    // Events
    public static event Action<ShopItem> OnItemPurchased;
    public static event Action<ShopItem, string> OnPurchaseFailed; // Item, reason
    public static event Action OnShopRefreshed;
    
    private void Awake()
    {
        InitializeShop();
    }
    
    private void Start()
    {
        SetupDefaultItems();
    }
    
    private void InitializeShop()
    {
        shopItemsById = new Dictionary<string, ShopItem>();
        itemsByCategory = new Dictionary<EShopCategory, List<ShopItem>>();
        
        // Initialize category lists
        foreach (EShopCategory category in System.Enum.GetValues(typeof(EShopCategory)))
        {
            itemsByCategory[category] = new List<ShopItem>();
        }
    }
    
    private void SetupDefaultItems()
    {
        // Create ShopItems from ScriptableObject definitions
        if (allShopItemDefinitions != null)
        {
            foreach (var definition in allShopItemDefinitions)
            {
                if (definition != null)
                {
                    var shopItem = CreateShopItemFromDefinition(definition);
                    if (shopItem != null)
                    {
                        AddItem(shopItem);
                    }
                }
            }
        }
        
        RefreshShop();
    }
    
    private ShopItem CreateShopItemFromDefinition(ShopItemDefinition definition)
    {
        var shopItem = new ShopItem(definition.ID, definition.displayName, definition.category, definition.itemType)
        {
            description = definition.description,
            cost = definition.cost,
            isUnlocked = definition.isUnlockedByDefault,
            isLimitedQuantity = definition.hasLimitedQuantity,
            maxPurchases = definition.maxPurchases,
            decorationType = definition.decorationType,
            resourceType = definition.resourceType,
            resourceAmount = definition.resourceAmount,
            icon = definition.icon
        };
        
        return shopItem;
    }
    
    public ShopItem CreateDecorationItem(string id, string name, string description, DecorationType decorationType, ResourceCost cost)
    {
        var item = new ShopItem(id, name, EShopCategory.Decorations, EItemType.Decoration)
        {
            description = description,
            decorationType = decorationType,
            cost = cost
        };
        
        AddItem(item);
        return item;
    }
    
    public ShopItem CreateResourceItem(string id, string name, string description, ResourceType resourceType, int amount, ResourceCost cost)
    {
        var item = new ShopItem(id, name, EShopCategory.Resources, EItemType.Resource)
        {
            description = description,
            resourceType = resourceType,
            resourceAmount = amount,
            cost = cost
        };
        
        AddItem(item);
        return item;
    }
    
    public void AddItem(ShopItem item)
    {
        if (item == null || string.IsNullOrEmpty(item.id))
        {
            Debug.LogError("Invalid shop item");
            return;
        }
        
        // Add to main collection
        if (!allShopItems.Contains(item))
            allShopItems.Add(item);
            
        // Update lookup dictionaries
        shopItemsById[item.id] = item;
        itemsByCategory[item.category].Add(item);
    }
    
    public bool TryPurchaseItem(string itemId)
    {
        if (!shopItemsById.TryGetValue(itemId, out ShopItem item))
        {
            OnPurchaseFailed?.Invoke(null, "Item not found");
            return false;
        }
        
        return TryPurchaseItem(item);
    }
    
    public bool TryPurchaseItem(ShopItem item)
    {
        if (item == null)
        {
            OnPurchaseFailed?.Invoke(null, "Item is null");
            return false;
        }
        
        // Check if item can be purchased
        if (!item.CanPurchase)
        {
            OnPurchaseFailed?.Invoke(item, "Item cannot be purchased");
            return false;
        }
        
        // Check if player can afford it
        if (!item.cost.CanAfford(ResourceManager.IN))
        {
            OnPurchaseFailed?.Invoke(item, "Cannot afford this item");
            return false;
        }
        
        // Spend resources
        if (!ResourceManager.IN.SpendResources(item.cost))
        {
            OnPurchaseFailed?.Invoke(item, "Failed to spend resources");
            return false;
        }
        
        // Execute purchase
        if (ExecutePurchase(item))
        {
            item.TryPurchase(); // Update purchase count
            OnItemPurchased?.Invoke(item);
            
            if (debugMode)
                Debug.Log($"Purchased {item.displayName}");
                
            return true;
        }
        else
        {
            // Refund resources if execution failed
            foreach (var resource in item.cost.RequiredResources)
            {
                ResourceManager.IN.AddResource(resource.type, resource.amount);
            }
            OnPurchaseFailed?.Invoke(item, "Failed to execute purchase");
            return false;
        }
    }
    
    private bool ExecutePurchase(ShopItem item)
    {
        switch (item.itemType)
        {
            case EItemType.Decoration:
                return PurchaseDecoration(item);
            case EItemType.Resource:
                return PurchaseResource(item);
            case EItemType.ToolUpgrade:
            case EItemType.Capacity:
            case EItemType.Multiplier:
            case EItemType.Unlock:
            case EItemType.Consumable:
                // TODO: Implement in future phases
                return true;
            default:
                return false;
        }
    }
    
    private bool PurchaseDecoration(ShopItem item)
    {
        if (DecorationManager.IN != null)
        {
            var decoration = DecorationManager.IN.PlaceDecoration(item.decorationType);
            return decoration != null;
        }
        return false;
    }
    
    private bool PurchaseResource(ShopItem item)
    {
        if (ResourceManager.IN != null)
        {
            return ResourceManager.IN.AddResource(item.resourceType, item.resourceAmount);
        }
        return false;
    }
    
    public List<ShopItem> GetItemsByCategory(EShopCategory category)
    {
        if (itemsByCategory.TryGetValue(category, out List<ShopItem> items))
            return new List<ShopItem>(items);
        return new List<ShopItem>();
    }
    
    public List<ShopItem> GetAvailableItems(EShopCategory category)
    {
        var categoryItems = GetItemsByCategory(category);
        return categoryItems.FindAll(item => item.CanPurchase);
    }
    
    public ShopItem GetItemById(string id)
    {
        shopItemsById.TryGetValue(id, out ShopItem item);
        return item;
    }
    
    public void RefreshShop()
    {
        // Update item availability, prices, etc.
        OnShopRefreshed?.Invoke();
    }
    
    public void UnlockItem(string itemId)
    {
        if (shopItemsById.TryGetValue(itemId, out ShopItem item))
        {
            item.isUnlocked = true;
            RefreshShop();
        }
    }
    
    public void LockItem(string itemId)
    {
        if (shopItemsById.TryGetValue(itemId, out ShopItem item))
        {
            item.isUnlocked = false;
            RefreshShop();
        }
    }
    
    // For save system
    public Dictionary<string, int> GetPurchaseData()
    {
        var purchaseData = new Dictionary<string, int>();
        foreach (var item in allShopItems)
        {
            if (item.currentPurchases > 0)
                purchaseData[item.id] = item.currentPurchases;
        }
        return purchaseData;
    }
    
    public void LoadPurchaseData(Dictionary<string, int> purchaseData)
    {
        foreach (var kvp in purchaseData)
        {
            if (shopItemsById.TryGetValue(kvp.Key, out ShopItem item))
            {
                item.currentPurchases = kvp.Value;
            }
        }
        RefreshShop();
    }
    
    // Debug helpers
    [ContextMenu("Give Test Resources")]
    private void GiveTestResources()
    {
        if (debugMode && ResourceManager.IN != null)
        {
            ResourceManager.IN.AddResource(ResourceType.Water, 100);
            ResourceManager.IN.AddResource(ResourceType.Gems, 50);
            Debug.Log("Added test resources");
        }
    }
}