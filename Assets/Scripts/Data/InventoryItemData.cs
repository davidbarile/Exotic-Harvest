using System;

[Serializable]
public class InventoryItemData
{
    public string DisplayName;
    public int Quantity;
    public int QuantityPerStack;
    public EShopCategory Category;

    public bool IsResource => this.Category == EShopCategory.Resources;
    public bool IsDecoration => this.Category == EShopCategory.Decorations;
    public bool IsItem => this.Category != EShopCategory.Resources;

    public DecorationData DecorationData;

    public bool IsUnlocked = true;
    public bool CanDragToWorld;
    public string IconSpriteName;
    public string WorldPrefabName = "DefaultItemUI";

    public static InventoryItemData Copy(InventoryItemData cloneTarget)
    {
        return new InventoryItemData
        {
            DisplayName = cloneTarget.DisplayName,
            Quantity = cloneTarget.Quantity,
            QuantityPerStack = cloneTarget.QuantityPerStack,
            Category = cloneTarget.Category,
            IsUnlocked = cloneTarget.IsUnlocked,
            CanDragToWorld = cloneTarget.CanDragToWorld,
            IconSpriteName = cloneTarget.IconSpriteName,
            WorldPrefabName = cloneTarget.WorldPrefabName
        };
    }

    public static InventoryItemData CreateFromShopConfig(ShopItemConfig shopConfig)
    {
        return new InventoryItemData
        {
            DisplayName = shopConfig.DisplayName,
            Quantity = shopConfig.ResourceAmount > 0 ? shopConfig.ResourceAmount : 1,
            QuantityPerStack = shopConfig.IsResource ? shopConfig.ResourceAmount : 1,
            Category = shopConfig.Category,
            IsUnlocked = shopConfig.IsUnlockedByDefault,
            CanDragToWorld = shopConfig.IsDecoration,
            IconSpriteName = shopConfig.Icon != null ? shopConfig.Icon.name : string.Empty,
            WorldPrefabName = shopConfig.IsDecoration && shopConfig.DecorationPrefab != null ? shopConfig.DecorationPrefab.name : "DefaultItemUI",
            DecorationData = shopConfig.IsDecoration ? new DecorationData { Type = shopConfig.DecorationType } : null
        };
    }
}