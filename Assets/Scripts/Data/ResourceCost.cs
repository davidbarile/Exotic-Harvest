using System.Collections.Generic;

/// <summary>
/// Represents the cost of an item in multiple resources (Catan-style)
/// </summary>
[System.Serializable]
public class ResourceCost
{
    public List<Resource> requiredResources = new List<Resource>();
    
    public ResourceCost()
    {
        requiredResources = new List<Resource>();
    }
    
    public ResourceCost(ResourceType type, int amount)
    {
        requiredResources = new List<Resource> { new Resource(type, amount) };
    }
    
    public ResourceCost(params Resource[] resources)
    {
        requiredResources = new List<Resource>(resources);
    }
    
    public void AddCost(ResourceType type, int amount)
    {
        requiredResources.Add(new Resource(type, amount));
    }
    
    public bool CanAfford(ResourceManager resourceManager)
    {
        foreach (var resource in requiredResources)
        {
            if (!resourceManager.HasResource(resource.type, resource.amount))
                return false;
        }
        return true;
    }
}