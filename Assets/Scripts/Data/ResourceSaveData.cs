using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

/// <summary>
/// Serializable data structure for saving/loading resources
/// </summary>
[Serializable]
public class ResourceSaveData
{
    [FormerlySerializedAs("Resources")] public List<ResourceData> ResourceDatas = new();
    public int MaxInventorySize = 100;
}