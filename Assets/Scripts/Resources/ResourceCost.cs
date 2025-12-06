using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the cost of an item in multiple resources (Catan-style)
/// </summary>
[Serializable]
public class ResourceCost
{
    [SerializeField] private List<Resource> requiredResources = new();
    
    public List<Resource> RequiredResources => requiredResources;
    
    public ResourceCost()
    {
        requiredResources = new();
    }
    
    public ResourceCost(ResourceType type, int amount)
    {
        requiredResources = new() { new Resource(type, amount) };
    }
    
    public ResourceCost(ResourceDefinition definition, int amount)
    {
        requiredResources = new() { new Resource(definition, amount) };
    }
    
    public ResourceCost(params Resource[] resources)
    {
        requiredResources = new(resources);
    }
    
    public void AddCost(ResourceType type, int amount)
    {
        requiredResources.Add(new Resource(type, amount));
    }
    
    public void AddCost(ResourceDefinition definition, int amount)
    {
        requiredResources.Add(new Resource(definition, amount));
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
    
    public int GetTotalValue()
    {
        int totalValue = 0;
        foreach (var resource in requiredResources)
        {
            totalValue += resource.BaseValue * resource.amount;
        }
        return totalValue;
    }
}