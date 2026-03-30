using System;
using UnityEngine;

[Serializable]
public class DecorationData
{
    public EDecorationType Type;
    public string PrefabName = "DefaultItemUI";
    public Vector3 WorldPosition;
    public int ParentGuid;
    public int SiblingIndex;

    // For passive harvesters
    [Header("Resource Generation")] 
    public int CurrentAmount;
    public int MaxAmount;
    public float ConversionRatio = 1f;
    public float GenerationInterval;// Seconds between generation
    public bool RequiresSpecificConditions;
    public float LastGenerationTime {get; set;}
    public bool IsActive = true;

    public static DecorationData Copy(DecorationData decorationData)
    {
        return new DecorationData
        {
            Type = decorationData.Type,
            PrefabName = decorationData.PrefabName,
            WorldPosition = decorationData.WorldPosition,
            ParentGuid = decorationData.ParentGuid,
            SiblingIndex = decorationData.SiblingIndex,
            CurrentAmount = decorationData.CurrentAmount,
            MaxAmount = decorationData.MaxAmount,
            ConversionRatio = decorationData.ConversionRatio,
            GenerationInterval = decorationData.GenerationInterval,
            RequiresSpecificConditions = decorationData.RequiresSpecificConditions,
            LastGenerationTime = decorationData.LastGenerationTime,
            IsActive = decorationData.IsActive
        };
    }
}
