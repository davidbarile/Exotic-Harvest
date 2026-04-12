using System;
using UnityEngine;
using Sirenix.OdinInspector;
using static ShopItemConfig;
using static GlobalEnums;

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
    public int MaxPurchases = -1;
    public int CurrentPurchases = 0;

    [Header("Item Data")]
    [ShowIf("Category", EShopCategory.Resources)]
    public ResourceItemData[] ResourceItems;
    
    public int Quanity = 1;
    public int MaxStack = 1;

    [Header("Decoration Data")]
    public DecorationData DecorationData;

    [Header("Visual")]
    public Color IconColor = Color.white;
    public Color BgColor = Color.white;
    public bool ShowInShop = true;
    
    // Properties
    public bool CanPurchase => MaxPurchases < 0 || CurrentPurchases < MaxPurchases;
    public int RemainingPurchases => MaxPurchases < 0 ? 99999 : MaxPurchases - CurrentPurchases;
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
            Quantity = shopItemData.Quanity,
            MaxStack = shopItemData.IsResource ? 100 : shopItemData.MaxStack,
            IconSpriteName = shopItemData.Icon != null ? shopItemData.Icon.name : string.Empty,
            IconColor = shopItemData.IconColor,
            DecorationData = DecorationData.Copy(shopItemData.DecorationData)
        };

        return inventoryItemData;
    }
}