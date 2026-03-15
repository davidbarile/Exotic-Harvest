using System;
using UnityEngine;

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

    public enum ELootType
    {
        None,
        RockPile,
        NightSky,
        //add more as needed
    }
}