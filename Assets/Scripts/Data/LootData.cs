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
    public WeightedRandom QuantityToDrop;
}