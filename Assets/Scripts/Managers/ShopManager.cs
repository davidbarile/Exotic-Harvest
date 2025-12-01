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
    [SerializeField] private List<ShopItem> allShopItems = new List<ShopItem>();
    [SerializeField] private bool debugMode = false;
    
    private Dictionary<string, ShopItem> shopItemsById;
    private Dictionary<ShopCategory, List<ShopItem>> itemsByCategory;
    
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
        itemsByCategory = new Dictionary<ShopCategory, List<ShopItem>>();
        
        // Initialize category lists
        foreach (ShopCategory category in System.Enum.GetValues(typeof(ShopCategory)))
        {
            itemsByCategory[category] = new List<ShopItem>();
        }
    }
    
    private void SetupDefaultItems()
    {
        // Phase 1 MVP items
        CreateDecorationItem("bucket_basic", "Water Bucket", "Collects rainwater automatically", 
            DecorationType.Bucket, new ResourceCost(ResourceType.Water, 5));
            
        CreateDecorationItem("plant_basic", "Jungle Plant", "Decorative plant for your desktop", 
            DecorationType.Plant, new ResourceCost(ResourceType.Water, 3));
            
        CreateResourceItem("water_small", "Water Drop", "Small amount of water", 
            ResourceType.Water, 1, new ResourceCost(ResourceType.Gems, 1));
            
        CreateResourceItem("water_large", "Water Bottle", "Large amount of water", 
            ResourceType.Water, 10, new ResourceCost(ResourceType.Gems, 8));
            
        RefreshShop();
    }
    
    public ShopItem CreateDecorationItem(string id, string name, string description, DecorationType decorationType, ResourceCost cost)
    {
        var item = new ShopItem(id, name, ShopCategory.Decorations, ItemType.Decoration)
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
        var item = new ShopItem(id, name, ShopCategory.Resources, ItemType.Resource)
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
            foreach (var resource in item.cost.requiredResources)
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
            case ItemType.Decoration:
                return PurchaseDecoration(item);
            case ItemType.Resource:
                return PurchaseResource(item);
            case ItemType.ToolUpgrade:
            case ItemType.Capacity:
            case ItemType.Multiplier:
            case ItemType.Unlock:
            case ItemType.Consumable:
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
    
    public List<ShopItem> GetItemsByCategory(ShopCategory category)
    {
        if (itemsByCategory.TryGetValue(category, out List<ShopItem> items))
            return new List<ShopItem>(items);
        return new List<ShopItem>();
    }
    
    public List<ShopItem> GetAvailableItems(ShopCategory category)
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