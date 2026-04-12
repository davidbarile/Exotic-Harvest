using System;
using System.Collections.Generic;
using UnityEngine;
using static GlobalEnums;

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
    
    public ResourceCost(EResourceType type, int amount)
    {
        this.requiredResources = new() { new ResourceData(type, amount) };
    }
    
    public ResourceCost(ResourceConfig config, int amount)
    {
        this.requiredResources = new() { new ResourceData(config, amount) };
    }
    
    public ResourceCost(params ResourceData[] resources)
    {
        this.requiredResources = new(resources);
    }
    
    public void AddCost(EResourceType type, int amount)
    {
        this.requiredResources.Add(new ResourceData(type, amount));
    }
    
    public void AddCost(ResourceConfig config, int amount)
    {
        this.requiredResources.Add(new ResourceData(config, amount));
    }
    
    public bool CanAfford()
    {
        foreach (var resource in this.requiredResources)
        {
            if (!ResourceManager.IN.HasResource(resource.Type, resource.Amount))
                return false;
        }
        return true;
    }
    
    public int GetTotalValue()
    {
        int totalValue = 0;
        foreach (var resource in this.requiredResources)
        {
            totalValue += resource.BaseValue * resource.Amount;
        }
        return totalValue;
    }
}