using System;
using UnityEngine;
using static GlobalEnums;

[Serializable]
public class InventoryItemData
{
    [TextArea(1, 4)] public string DisplayName;
    public int Quantity;
    public int MaxStack;
    public int SpaceAvailableInStack => MaxStack - Quantity;
    public EShopCategory Category;
    public DecorationData DecorationData = new();

    public bool IsItem => this.Category != EShopCategory.Resources;
    public bool IsDecoration => this.Category == EShopCategory.Decorations;
    public bool IsResource => this.Category == EShopCategory.Resources;

    public float Scale = 1f;
    public string IconSpriteName;
    public string WorldIconSpriteName;
    public Color IconColor = Color.white;

    public static InventoryItemData Copy(InventoryItemData cloneTarget)
    {
        return new InventoryItemData
        {
            DisplayName = cloneTarget.DisplayName,
            Quantity = cloneTarget.Quantity,
            MaxStack = cloneTarget.MaxStack,
            Category = cloneTarget.Category,
            Scale = cloneTarget.Scale,
            IconSpriteName = cloneTarget.IconSpriteName,
            WorldIconSpriteName = cloneTarget.WorldIconSpriteName,
            IconColor = cloneTarget.IconColor,
            DecorationData = DecorationData.Copy(cloneTarget.DecorationData),
        };
    }

    public static InventoryItemData CreateFromShopConfig(ShopItemConfig shopConfig)
    {
        return new InventoryItemData
        {
            DisplayName = shopConfig.DisplayName,
            Quantity = shopConfig.Quanity,
            MaxStack = shopConfig.IsResource ? 100 : shopConfig.MaxStack,
            Category = shopConfig.Category,
            Scale = shopConfig.Scale,
            IconSpriteName = shopConfig.Icon != null ? shopConfig.Icon.name : string.Empty,
            WorldIconSpriteName = shopConfig.WorldSprite != null ? shopConfig.WorldSprite.name : string.Empty,
            IconColor = shopConfig.IconColor,
            DecorationData = DecorationData.Copy(shopConfig.DecorationData)
        };
    }
}