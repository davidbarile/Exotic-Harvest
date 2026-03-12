using System;

[Serializable]
public class InventoryItemData
{
    public string DisplayName;
    public int Quantity;
    public int MaxStack;
    public EShopCategory Category;
    public DecorationData DecorationData;

    public bool IsItem => this.Category != EShopCategory.Resources;
    public bool IsDecoration => this.Category == EShopCategory.Decorations;
    public bool IsResource => this.Category == EShopCategory.Resources;

    public bool IsUnlocked = true;
    public bool CanDragToWorld;
    public string IconSpriteName;

    public static InventoryItemData Copy(InventoryItemData cloneTarget)
    {
        return new InventoryItemData
        {
            DisplayName = cloneTarget.DisplayName,
            Quantity = cloneTarget.Quantity,
            MaxStack = cloneTarget.MaxStack,
            Category = cloneTarget.Category,
            IsUnlocked = cloneTarget.IsUnlocked,
            CanDragToWorld = cloneTarget.CanDragToWorld,
            IconSpriteName = cloneTarget.IconSpriteName,
            DecorationData = DecorationData.Copy(cloneTarget.DecorationData)
        };
    }

    public static InventoryItemData CreateFromShopConfig(ShopItemConfig shopConfig)
    {
        return new InventoryItemData
        {
            DisplayName = shopConfig.DisplayName,
            Quantity = shopConfig.ResourceAmount > 0 ? shopConfig.ResourceAmount : 1,
            MaxStack = shopConfig.IsResource ? shopConfig.ResourceAmount : 1,
            Category = shopConfig.Category,
            IsUnlocked = shopConfig.IsUnlockedByDefault,
            CanDragToWorld = shopConfig.IsDecoration,
            IconSpriteName = shopConfig.Icon != null ? shopConfig.Icon.name : string.Empty,
            DecorationData = DecorationData.Copy(shopConfig.DecorationData)
        };
    }
}