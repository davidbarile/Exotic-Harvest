using System;
using System.Collections.Generic;

/// <summary>
/// Serializable data structure for saving/loading resources
/// </summary>
[Serializable]
public class ResourceSaveData
{
    public List<ResourceData> ResourceDatas = new();
}