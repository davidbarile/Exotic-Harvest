using System;
using static InventoryManager;

[Serializable]
public class InventoryItemData
{
    public string DisplayName;
    public int Quantity;
    public int QuantityPerStack;
    public EInventoryCategory Category;
    public EItemType ItemType;

    public bool IsUnlocked = true;

    public bool CanDragToWorld;
    public string IconSpriteName;

    public static InventoryItemData Clone(InventoryItemData cloneTarget)
    {
        return new InventoryItemData
        {
            DisplayName = cloneTarget.DisplayName,
            Quantity = cloneTarget.Quantity,
            QuantityPerStack = cloneTarget.QuantityPerStack,
            Category = cloneTarget.Category,
            ItemType = cloneTarget.ItemType,
            IsUnlocked = cloneTarget.IsUnlocked,
            CanDragToWorld = cloneTarget.CanDragToWorld,
            IconSpriteName = cloneTarget.IconSpriteName
        };
    }
}