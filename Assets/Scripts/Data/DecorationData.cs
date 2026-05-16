using System;
using UnityEngine;
using static GlobalEnums;

[Serializable]
public class DecorationData
{
    public string PrefabName = "DefaultItemUI";

    public WorldSaveData WorldSaveData;

    [Space, Header("Drag Zone Flags")]
    public EDecorationType DecorationType;
    public bool HighlightValidTargetsWhenDragged;

    [Space, Header("Decoration Holder Settings")]
    public bool IsDragZone;
    public int Guid { get; set; } = -1;

    // For passive harvesters
    [Header("Resource Generation")]
    [HideInInspector] public EResourceType ActiveResourceType;
    public EResourceType GeneratedResource;
    public int CurrentAmount;
    public int MaxAmount;
    public float GenerationInterval;// Seconds between generation
    public bool RequiresSpecificConditions;
    public float LastGenerationTime {get; set;}
    public bool IsActive = true;

    public static DecorationData Copy(DecorationData decorationData)
    {        
        return new DecorationData
        {
            PrefabName = decorationData.PrefabName,
            WorldSaveData = new WorldSaveData
            {
                WorldPosition = decorationData.WorldSaveData.WorldPosition,
                Scale = decorationData.WorldSaveData.Scale,
                Rotation = decorationData.WorldSaveData.Rotation,
                ParentGuid = decorationData.WorldSaveData.ParentGuid,
                SiblingIndex = decorationData.WorldSaveData.SiblingIndex
            },
            DecorationType = decorationData.DecorationType,
            HighlightValidTargetsWhenDragged = decorationData.HighlightValidTargetsWhenDragged,
            IsDragZone = decorationData.IsDragZone,
            Guid = decorationData.Guid, // Only generate new guid if original is -1, otherwise copy existing guid (used for saving/loading)
            ActiveResourceType = decorationData.ActiveResourceType,
            GeneratedResource = decorationData.GeneratedResource,
            CurrentAmount = decorationData.CurrentAmount,
            MaxAmount = decorationData.MaxAmount,
            GenerationInterval = decorationData.GenerationInterval,
            RequiresSpecificConditions = decorationData.RequiresSpecificConditions,
            LastGenerationTime = decorationData.LastGenerationTime,
            IsActive = decorationData.IsActive
        };
    }
}

[Serializable]
public class WorldSaveData
{
    public Vector3 WorldPosition;
    public float Scale;
    public float Rotation;
    public int ParentGuid;
    public int SiblingIndex;
}