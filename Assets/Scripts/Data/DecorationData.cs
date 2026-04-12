using System;
using UnityEngine;
using static GlobalEnums;

[Serializable]
public class DecorationData
{
    public string PrefabName = "DefaultItemUI";
    public Vector3 WorldPosition;
    public int ParentGuid;
    public int SiblingIndex;

    [Space, Header("Drag Zone Flags")]
    public EDecorationType DecorationType;
    public bool HighlightValidTargetsWhenDragged;

    // For passive harvesters
    [Header("Resource Generation")]
    public EResourceType GeneratedResource;
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
            PrefabName = decorationData.PrefabName,
            WorldPosition = decorationData.WorldPosition,
            ParentGuid = decorationData.ParentGuid,
            SiblingIndex = decorationData.SiblingIndex,
            DecorationType = decorationData.DecorationType,
            HighlightValidTargetsWhenDragged = decorationData.HighlightValidTargetsWhenDragged,
            GeneratedResource = decorationData.GeneratedResource,
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
