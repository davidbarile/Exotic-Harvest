using System;
using System.Collections.Generic;

/// <summary>
/// Serializable data structure for saving/loading resources
/// </summary>
[Serializable]
public class ResourceData
{
    public List<Resource> Resources = new();
    public int MaxInventorySize = 100;
}