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
    public ResourceCost Cost;
    
    [Header("Purchase Rules")]
    public bool IsUnlocked = true;
    public bool IsLimitedQuantity = false;
    public int MaxPurchases = 1;
    public int CurrentPurchases = 0;
    
    [Header("Item Data")]
    public EDecorationType DecorationType; // For decoration items
    public EResourceType ResourceType;     // For resource items
    public int ResourceAmount = 1;        // Amount when purchasing resources
    public int Quanity = 1;
    public int MaxStack = 1;

    [Header("Decoration Data")]
    public DecorationData DecorationData;
    public bool CanDragToWorld;

    [Header("Visual")]
    public Color BackgroundColor = Color.white;
    public bool ShowInShop = true;
    
    // Properties
    public bool CanPurchase => IsUnlocked && (!IsLimitedQuantity || CurrentPurchases < MaxPurchases);
    public bool IsMaxedOut => IsLimitedQuantity && CurrentPurchases >= MaxPurchases;
    public int RemainingPurchases => IsLimitedQuantity ? MaxPurchases - CurrentPurchases : -1;
    public bool IsTool => this.Category == EShopCategory.Tools;
    public bool IsResource => this.Category == EShopCategory.Resources;
    public bool IsDecoration => this.Category == EShopCategory.Decorations;
    
    public ShopItemData(string id, string name, EShopCategory category)
    {
        this.Id = id;
        this.DisplayName = name;
        this.Category = category;
        this.Cost = new ResourceCost();
    }
    
    public bool TryPurchase()
    {
        if (!this.CanPurchase)
            return false;
            
        if (this.IsLimitedQuantity)
            this.CurrentPurchases++;
            
        return true;
    }

    public void ResetPurchases()
    {
        this.CurrentPurchases = 0;
    }

    public static InventoryItemData ToInventoryItemData(ShopItemData shopItemData)
    {
        if (shopItemData == null)
            return null;

        var inventoryItemData = new InventoryItemData
        {
            DisplayName = shopItemData.DisplayName,
            Category = shopItemData.Category,
            Quantity = shopItemData.IsResource ? shopItemData.ResourceAmount : shopItemData.Quanity,
            MaxStack = shopItemData.IsResource ? 100 : shopItemData.MaxStack,
            IconSpriteName = shopItemData.Icon != null ? shopItemData.Icon.name : string.Empty,
            CanDragToWorld = shopItemData.CanDragToWorld,
            DecorationData = DecorationData.Copy(shopItemData.DecorationData)
        };

        return inventoryItemData;
    }
}