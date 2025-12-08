using System;
using UnityEngine;

/// <summary>
/// Represents an item that can be purchased in the shop
/// </summary>
[Serializable]
public class ShopItemData
{
    [Header("Item Identity")]
    public string Id;
    public string DisplayName;
    [TextArea(2, 4)] public string Description;
    public Sprite Icon;
    
    [Header("Item Properties")]
    public EShopCategory Category;
    public EItemType ItemType;
    public ResourceCost Cost;
    
    [Header("Purchase Rules")]
    public bool IsUnlocked = true;
    public bool IsLimitedQuantity = false;
    public int MaxPurchases = 1;
    public int CurrentPurchases = 0;
    
    [Header("Item Data")]
    public DecorationType DecorationType; // For decoration items
    public ResourceType ResourceType;     // For resource items
    public int ResourceAmount = 1;        // Amount when purchasing resources

    [Header("Visual")]
    public Color BackgroundColor = Color.white;
    public bool ShowInShop = true;
    
    // Properties
    public bool CanPurchase => IsUnlocked && (!IsLimitedQuantity || CurrentPurchases < MaxPurchases);
    public bool IsMaxedOut => IsLimitedQuantity && CurrentPurchases >= MaxPurchases;
    public int RemainingPurchases => IsLimitedQuantity ? MaxPurchases - CurrentPurchases : -1;
    
    public ShopItemData(string id, string name, EShopCategory category, EItemType type)
    {
        this.Id = id;
        this.DisplayName = name;
        this.Category = category;
        this.ItemType = type;
        this.Cost = new ResourceCost();
    }
    
    public bool TryPurchase()
    {
        if (!CanPurchase)
            return false;
            
        if (IsLimitedQuantity)
            CurrentPurchases++;
            
        return true;
    }
    
    public void ResetPurchases()
    {
        CurrentPurchases = 0;
    }
}