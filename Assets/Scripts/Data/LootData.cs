using System;
using UnityEngine;
using static GlobalEnums;

[Serializable]
public class LootData
{
    public string DisplayName;
    public string OverrideSpriteName;
    public EResourceType ResourceType;
    public int Quantity { get; set; }

    [Header("x/100 chance to drop")]
    public WeightedRandom ChanceToDrop;
    [Header("Amount under Rock, or # of spawns in Meadow")]
    public WeightedRandom QuantityToDrop;

    public static LootData Copy(LootData inLootData)
    {
        var lootData = new LootData()
        {
            DisplayName = inLootData.DisplayName,
            OverrideSpriteName = inLootData.OverrideSpriteName,
            ResourceType = inLootData.ResourceType,
            Quantity = inLootData.Quantity,
            ChanceToDrop = inLootData.ChanceToDrop,
            QuantityToDrop = inLootData.QuantityToDrop
        };

        return lootData;
    }
}