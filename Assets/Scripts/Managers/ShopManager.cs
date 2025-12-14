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
    
    private List<ShopItemData> allShopItems = new();
    
    private Dictionary<string, ShopItemData> shopItemsById;
    private Dictionary<EShopCategory, List<ShopItemData>> itemsByCategory;
    
    public ShopDatabase Database => this.shopDatabase;
    
    // Events
    public static event Action<ShopItemData> OnItemPurchased;
    public static event Action<ShopItemData, string> OnPurchaseFailed; // Item, reason
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
        foreach (EShopCategory category in Enum.GetValues(typeof(EShopCategory)))
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
    
    private ShopItemData CreateShopItemFromDefinition(ShopItemConfig definition)
    {
        var shopItem = new ShopItemData(definition.ID, definition.DisplayName, definition.Category, definition.ItemType)
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
    
    public ShopItemData CreateDecorationItem(string id, string name, string description, DecorationType decorationType, ResourceCost cost)
    {
        var item = new ShopItemData(id, name, EShopCategory.Decorations, EItemType.Decoration)
        {
            Description = description,
            DecorationType = decorationType,
            Cost = cost
        };
        
        AddItem(item);
        return item;
    }
    
    public ShopItemData CreateResourceItem(string id, string name, string description, ResourceType resourceType, int amount, ResourceCost cost)
    {
        var item = new ShopItemData(id, name, EShopCategory.Resources, EItemType.Resource)
        {
            Description = description,
            ResourceType = resourceType,
            ResourceAmount = amount,
            Cost = cost
        };
        
        AddItem(item);
        return item;
    }
    
    public void AddItem(ShopItemData itemData)
    {
        if (itemData == null || string.IsNullOrEmpty(itemData.Id))
        {
            Debug.LogError("Invalid shop itemData");
            return;
        }
        
        // Add to main collection
        if (!this.allShopItems.Contains(itemData))
            this.allShopItems.Add(itemData);
            
        // Update lookup dictionaries
        this.shopItemsById[itemData.Id] = itemData;
        this.itemsByCategory[itemData.Category].Add(itemData);
    }
    
    public bool TryPurchaseItem(string itemId)
    {
        if (!this.shopItemsById.TryGetValue(itemId, out ShopItemData item))
        {
            OnPurchaseFailed?.Invoke(null, "Item not found");
            return false;
        }
        
        return TryPurchaseItem(item);
    }
    
    public bool TryPurchaseItem(ShopItemData itemData)
    {
        if (itemData == null)
        {
            OnPurchaseFailed?.Invoke(null, "Item is null");
            return false;
        }
        
        // Check if itemData can be purchased
        if (!itemData.CanPurchase)
        {
            OnPurchaseFailed?.Invoke(itemData, "Item cannot be purchased");
            return false;
        }
        
        // Check if player can afford it
        if (!itemData.Cost.CanAfford(ResourceManager.IN))
        {
            OnPurchaseFailed?.Invoke(itemData, "Cannot afford this itemData");
            return false;
        }
        
        // Spend resources
        if (!ResourceManager.IN.SpendResources(itemData.Cost))
        {
            OnPurchaseFailed?.Invoke(itemData, "Failed to spend resources");
            return false;
        }
        
        // Execute purchase
        if (ExecutePurchase(itemData))
        {
            itemData.TryPurchase(); // Update purchase count
            OnItemPurchased?.Invoke(itemData);
            
            if (this.debugMode)
                Debug.Log($"Purchased {itemData.DisplayName}");
                
            return true;
        }
        else
        {
            // Refund resources if execution failed
            foreach (var resource in itemData.Cost.RequiredResources)
            {
                ResourceManager.IN.AddResource(resource.Type, resource.Amount);
            }
            OnPurchaseFailed?.Invoke(itemData, "Failed to execute purchase");
            return false;
        }
    }
    
    private bool ExecutePurchase(ShopItemData itemData)
    {
        switch (itemData.ItemType)
        {
            case EItemType.Decoration:
                return PurchaseDecoration(itemData);
            case EItemType.Resource:
                return PurchaseResource(itemData);
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
    
    private bool PurchaseDecoration(ShopItemData itemData)
    {
        if (DecorationManager.IN != null)
        {
            var decoration = DecorationManager.IN.PlaceDecoration(itemData.DecorationType);
            return decoration != null;
        }
        return false;
    }
    
    private bool PurchaseResource(ShopItemData itemData)
    {
        if (ResourceManager.IN != null)
        {
            return ResourceManager.IN.AddResource(itemData.ResourceType, itemData.ResourceAmount);
        }
        return false;
    }
    
    public List<ShopItemData> GetItemsByCategory(EShopCategory category)
    {
        if (this.itemsByCategory.TryGetValue(category, out List<ShopItemData> items))
            return new(items);
        return new();
    }
    
    public List<ShopItemData> GetAvailableItems(EShopCategory category)
    {
        var categoryItems = GetItemsByCategory(category);
        return categoryItems.FindAll(item => item.CanPurchase);
    }
    
    public ShopItemData GetItemById(string id)
    {
        this.shopItemsById.TryGetValue(id, out ShopItemData item);
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
        if (shopItemsById.TryGetValue(itemId, out ShopItemData item))
        {
            item.IsUnlocked = true;
            RefreshShop();
        }
    }
    
    public void LockItem(string itemId)
    {
        if (shopItemsById.TryGetValue(itemId, out ShopItemData item))
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
            if (this.shopItemsById.TryGetValue(kvp.Key, out ShopItemData item))
            {
                item.CurrentPurchases = kvp.Value;
            }
        }
        RefreshShop();
    }
    
    // Debug helpers
    [ContextMenu("Give Test resourcesSave")]
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