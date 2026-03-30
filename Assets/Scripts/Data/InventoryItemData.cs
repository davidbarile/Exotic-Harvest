using System;

[Serializable]
public class InventoryItemData
{
    public string DisplayName;
    public int Quantity;
    public int MaxStack;
    public int SpaceAvailableInStack => MaxStack - Quantity;
    public EShopCategory Category;
    public DecorationData DecorationData;

    public bool IsItem => this.Category != EShopCategory.Resources;
    public bool IsDecoration => this.Category == EShopCategory.Decorations;
    public bool IsResource => this.Category == EShopCategory.Resources;

    public bool IsUnlocked = true;
    public bool CanDragToWorld;
    public float Scale = 1f;
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
            Scale = cloneTarget.Scale,
            IconSpriteName = cloneTarget.IconSpriteName,
            DecorationData = DecorationData.Copy(cloneTarget.DecorationData)
        };
    }

    public static InventoryItemData CreateFromShopConfig(ShopItemConfig shopConfig)
    {
        return new InventoryItemData
        {
            DisplayName = shopConfig.DisplayName,
            Quantity = shopConfig.IsResource ? shopConfig.ResourceAmount : 1,
            MaxStack = shopConfig.IsResource ? 100 : shopConfig.MaxStack,
            Category = shopConfig.Category,
            IsUnlocked = shopConfig.IsUnlockedByDefault,
            CanDragToWorld = shopConfig.CanDragToWorld,
            Scale = shopConfig.Scale,
            IconSpriteName = shopConfig.Icon != null ? shopConfig.Icon.name : string.Empty,
            DecorationData = DecorationData.Copy(shopConfig.DecorationData)
        };
    }
}