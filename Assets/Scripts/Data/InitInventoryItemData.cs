using System;
using UnityEngine;

[Serializable]
public class InitInventoryItemData : InventoryItemData
{
    [Header("Initialization Config - overrides above properties")]
    public ShopItemConfig ShopItemConfig;
}