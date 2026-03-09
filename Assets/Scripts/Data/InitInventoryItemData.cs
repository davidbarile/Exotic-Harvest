using System;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public class InitInventoryItemData : InventoryItemData
{
    [Header("Initialization Config - overrides above properties")]
    public ShopItemConfig ShopItemConfig;
}