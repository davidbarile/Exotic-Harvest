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
    [SerializeField] private ShopDatabase shopDatabase;
    [SerializeField] private bool debugMode;
    
    private List<ShopItem> allShopItems = new();
    
    private Dictionary<string, ShopItem> shopItemsById;
    private Dictionary<EShopCategory, List<ShopItem>> itemsByCategory;
    
    public ShopDatabase Database => this.shopDatabase;
    
    // Events
    public static event Action<ShopItem> OnItemPurchased;
    public static event Action<ShopItem, string> OnPurchaseFailed; // Item, reason
    public static event Action OnShopRefreshed;
    
    private void Awake()
    {
        if (IN == null)
        {
            IN = this;
        }
        else if (IN != this)
        {
            Debug.LogWarning("Multiple ShopManager instances found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        InitializeShop();
    }
    
    private void Start()
    {
        SetupDefaultItems();
    }
    
    private void InitializeShop()
    {
        this.shopItemsById = new();
        this.itemsByCategory = new();
        
        // Initialize category lists
        foreach (EShopCategory category in System.Enum.GetValues(typeof(EShopCategory)))
        {
            this.itemsByCategory[category] = new();
        }
    }
    
    private void SetupDefaultItems()
    {
        // Create ShopItems from ScriptableObject definitions
        if (this.shopDatabase != null && this.shopDatabase.AllShopItems != null)
        {
            foreach (var definition in this.shopDatabase.AllShopItems)
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
    
    private ShopItem CreateShopItemFromDefinition(ShopItemConfig definition)
    {
        var shopItem = new ShopItem(definition.ID, definition.DisplayName, definition.Category, definition.ItemType)
        {
            Description = definition.Description,
            Cost = definition.Cost,
            IsUnlocked = definition.IsUnlockedByDefault,
            IsLimitedQuantity = definition.HasLimitedQuantity,
            MaxPurchases = definition.MaxPurchases,
            DecorationType = definition.DecorationType,
            ResourceType = definition.ResourceType,
            ResourceAmount = definition.ResourceAmount,
            Icon = definition.Icon
        };
        
        return shopItem;
    }
    
    public ShopItem CreateDecorationItem(string id, string name, string description, DecorationType decorationType, ResourceCost cost)
    {
        var item = new ShopItem(id, name, EShopCategory.Decorations, EItemType.Decoration)
        {
            Description = description,
            DecorationType = decorationType,
            Cost = cost
        };
        
        AddItem(item);
        return item;
    }
    
    public ShopItem CreateResourceItem(string id, string name, string description, ResourceType resourceType, int amount, ResourceCost cost)
    {
        var item = new ShopItem(id, name, EShopCategory.Resources, EItemType.Resource)
        {
            Description = description,
            ResourceType = resourceType,
            ResourceAmount = amount,
            Cost = cost
        };
        
        AddItem(item);
        return item;
    }
    
    public void AddItem(ShopItem item)
    {
        if (item == null || string.IsNullOrEmpty(item.Id))
        {
            Debug.LogError("Invalid shop item");
            return;
        }
        
        // Add to main collection
        if (!allShopItems.Contains(item))
            allShopItems.Add(item);
            
        // Update lookup dictionaries
        shopItemsById[item.Id] = item;
        itemsByCategory[item.Category].Add(item);
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
        if (!item.Cost.CanAfford(ResourceManager.IN))
        {
            OnPurchaseFailed?.Invoke(item, "Cannot afford this item");
            return false;
        }
        
        // Spend resources
        if (!ResourceManager.IN.SpendResources(item.Cost))
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
                Debug.Log($"Purchased {item.DisplayName}");
                
            return true;
        }
        else
        {
            // Refund resources if execution failed
            foreach (var resource in item.Cost.RequiredResources)
            {
                ResourceManager.IN.AddResource(resource.Type, resource.Amount);
            }
            OnPurchaseFailed?.Invoke(item, "Failed to execute purchase");
            return false;
        }
    }
    
    private bool ExecutePurchase(ShopItem item)
    {
        switch (item.ItemType)
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
            var decoration = DecorationManager.IN.PlaceDecoration(item.DecorationType);
            return decoration != null;
        }
        return false;
    }
    
    private bool PurchaseResource(ShopItem item)
    {
        if (ResourceManager.IN != null)
        {
            return ResourceManager.IN.AddResource(item.ResourceType, item.ResourceAmount);
        }
        return false;
    }
    
    public List<ShopItem> GetItemsByCategory(EShopCategory category)
    {
        if (this.itemsByCategory.TryGetValue(category, out List<ShopItem> items))
            return new(items);
        return new();
    }
    
    public List<ShopItem> GetAvailableItems(EShopCategory category)
    {
        var categoryItems = GetItemsByCategory(category);
        return categoryItems.FindAll(item => item.CanPurchase);
    }
    
    public ShopItem GetItemById(string id)
    {
        this.shopItemsById.TryGetValue(id, out ShopItem item);
        return item;
    }
    
    /// <summary>
    /// Get shop item config from database by ID
    /// </summary>
    public ShopItemConfig GetItemConfigById(string id)
    {
        if (this.shopDatabase != null)
        {
            return this.shopDatabase.GetShopItem(id);
        }
        return null;
    }
    
    /// <summary>
    /// Get shop item configs by category from database
    /// </summary>
    public ShopItemConfig[] GetItemConfigsByCategory(EShopCategory category)
    {
        if (this.shopDatabase != null)
        {
            return this.shopDatabase.GetItemsByCategory(category);
        }
        return new ShopItemConfig[0];
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
            item.IsUnlocked = true;
            RefreshShop();
        }
    }
    
    public void LockItem(string itemId)
    {
        if (shopItemsById.TryGetValue(itemId, out ShopItem item))
        {
            item.IsUnlocked = false;
            RefreshShop();
        }
    }
    
    // For save system
    public Dictionary<string, int> GetPurchaseData()
    {
        var purchaseData = new Dictionary<string, int>();
        foreach (var item in this.allShopItems)
        {
            if (item.CurrentPurchases > 0)
                purchaseData[item.Id] = item.CurrentPurchases;
        }
        return purchaseData;
    }
    
    public void LoadPurchaseData(Dictionary<string, int> purchaseData)
    {
        foreach (var kvp in purchaseData)
        {
            if (this.shopItemsById.TryGetValue(kvp.Key, out ShopItem item))
            {
                item.CurrentPurchases = kvp.Value;
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