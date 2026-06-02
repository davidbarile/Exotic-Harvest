using System;
using UnityEngine;
using static GlobalEnums;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;

[Serializable]
public class DecorationData
{
    public string PrefabName = "DefaultItemUI";

    public WorldSaveData WorldSaveData;

    [Space, Header("Drag Zone Flags")]
    public EDecorationType DecorationType;
    public bool HighlightValidTargetsWhenDragged;

    //Decoration Holder Setting
    [ReadOnly] public int Guid = -1;

    // For passive harvesters
    [Header("Resource Generation")]
    [HideInInspector] public EResourceType ActiveResourceType;

    [ShowIf("@DecorationType.HasFlag(GlobalEnums.EDecorationType.Tool)")]
    public bool IsAttractor;
    [ShowIf("IsAttractor")]
    public AttractorData AttractorData;

    [Space]
    public EResourceType GeneratedResource;
    public int CurrentAmount;
    public int MaxAmount;
    public float GenerationInterval;// Seconds between generation
    public bool RequiresSpecificConditions;
    [ReadOnly] public float LastGenerationTime;
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
            Guid = decorationData.Guid, // Only generate new guid if original is -1, otherwise copy existing guid (used for saving/loading)
            ActiveResourceType = decorationData.ActiveResourceType,
            IsAttractor = decorationData.IsAttractor,
            AttractorData = decorationData.IsAttractor ? AttractorData.Copy(decorationData.AttractorData) : null,
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
    [ReadOnly] public Vector3 WorldPosition;
    [ReadOnly] public float Scale = 1f;
    [ReadOnly] public float Rotation;
    [ReadOnly] public int ParentGuid;
    [ReadOnly] public int SiblingIndex;
}