using System;
using System.Collections.Generic;

/// <summary>
/// Serializable data structure for saving/loading resources
/// </summary>
[Serializable]
public class ResourceData
{
    public List<Resource> resources = new();
    public int maxInventorySize = 100;
}