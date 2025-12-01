using System.Collections.Generic;

/// <summary>
/// Serializable data structure for saving/loading resources
/// </summary>
[System.Serializable]
public class ResourceData
{
    public List<Resource> resources = new List<Resource>();
    public int maxInventorySize = 100;
}