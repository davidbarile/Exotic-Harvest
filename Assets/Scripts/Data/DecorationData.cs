using System;
using UnityEngine;

[Serializable]
public class DecorationData
{
    public EDecorationType Type;
    public string PrefabName = "DefaultItemUI";
    public Vector3 WorldPosition;
    public int ParentGuid; // For decorations that are children of others;
    public int SiblingIndex;
    
    // For passive harvesters
    [Space] public int CurrentAmount;
    public float LastGenerationTime;
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
            LastGenerationTime = decorationData.LastGenerationTime,
            IsActive = decorationData.IsActive
        };
    }
}
