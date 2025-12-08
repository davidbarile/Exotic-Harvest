using System;
using UnityEngine;
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
}