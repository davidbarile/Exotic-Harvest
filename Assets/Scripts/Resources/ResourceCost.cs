using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the cost of an item in multiple resources (Catan-style)
/// </summary>
[Serializable]
public class ResourceCost
{
    [SerializeField] private List<ResourceData> requiredResources = new();
    
    public List<ResourceData> RequiredResources => requiredResources;
    
    public ResourceCost()
    {
        requiredResources = new();
    }
    
    public ResourceCost(ResourceType type, int amount)
    {
        requiredResources = new() { new ResourceData(type, amount) };
    }
    
    public ResourceCost(ResourceConfig config, int amount)
    {
        requiredResources = new() { new ResourceData(config, amount) };
    }
    
    public ResourceCost(params ResourceData[] resources)
    {
        requiredResources = new(resources);
    }
    
    public void AddCost(ResourceType type, int amount)
    {
        requiredResources.Add(new ResourceData(type, amount));
    }
    
    public void AddCost(ResourceConfig config, int amount)
    {
        requiredResources.Add(new ResourceData(config, amount));
    }
    
    public bool CanAfford(ResourceManager resourceManager)
    {
        foreach (var resource in requiredResources)
        {
            if (!resourceManager.HasResource(resource.Type, resource.Amount))
                return false;
        }
        return true;
    }
    
    public int GetTotalValue()
    {
        int totalValue = 0;
        foreach (var resource in requiredResources)
        {
            totalValue += resource.BaseValue * resource.Amount;
        }
        return totalValue;
    }
}