using System;
using System.Collections.Generic;
using UnityEngine;
using static GlobalEnums;

/// <summary>
/// Manages all player resources and inventory
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager IN;

     // Events for UI updates
    public static Action<EResourceType, int> OnResourceChanged;
    public static Action<EResourceType, int> OnResourceGained;
    public static Action OnInventoryFull;
    
    [Header("ResourceData Database")]
    [SerializeField] private ResourceDatabase resourceDatabase;
    
    private Dictionary<EResourceType, ResourceData> resourcesDict = new();
    
    public ResourceDatabase Database => this.resourceDatabase;

    public int DebugAddAmount = 10; // Amount to add when testing resource gain
    public bool DebugGrantAllResources; // Toggle for testing resource gain in Update()
    
    private void Awake()
    {
        InitializeInventory();
    }
    
    private void InitializeInventory()
    {
        // Initialize with 0 of each resource type
        foreach (EResourceType type in Enum.GetValues(typeof(EResourceType)))
        {
            this.resourcesDict[type] = new ResourceData(type, 0);
        }
    }
    
    public bool HasResource(EResourceType type, int amount)
    {
        return this.resourcesDict.ContainsKey(type) && this.resourcesDict[type].Amount >= amount;
    }
    
    public int GetResourceAmount(EResourceType type)
    {
        return this.resourcesDict.ContainsKey(type) ? this.resourcesDict[type].Amount : 0;
    }
    
    public void AddResource(EResourceType type, int amount)
    {
        if (!this.resourcesDict.ContainsKey(type))
            this.resourcesDict[type] = new ResourceData(type, 0);
        
        this.resourcesDict[type].Add(amount);
        OnResourceChanged?.Invoke(type, this.resourcesDict[type].Amount);
        OnResourceGained?.Invoke(type, amount);
    }
    
    public bool SpendResources(ResourceCost cost)
    {
        if (!cost.CanAfford())
            return false;
            
        foreach (var resource in cost.RequiredResources)
        {
            this.resourcesDict[resource.Type].Subtract(resource.Amount);
            OnResourceChanged?.Invoke(resource.Type, this.resourcesDict[resource.Type].Amount);
        }
        return true;
    }
    
    public int GetTotalItemCount()
    {
        int total = 0;
        foreach (var resource in this.resourcesDict.Values)
        {
            total += resource.Amount;
        }
        return total;
    }
    
    // For save system
    public ResourceSaveData GetSaveData()
    {
        var saveData = new ResourceSaveData();
        foreach (var kvp in this.resourcesDict)
        {
            if (kvp.Value.Amount > 0)
                saveData.ResourceDatas.Add(kvp.Value.Copy());
        }
        return saveData;
    }
    
    public void LoadFromSaveData(ResourceSaveData saveSaveData)
    {
        InitializeInventory(); // Reset to 0
        
        foreach (var resource in saveSaveData.ResourceDatas)
        {
            if (this.resourcesDict.ContainsKey(resource.Type))
                this.resourcesDict[resource.Type] = resource.Copy();
        }
        
        // Notify UI of all changes
        foreach (var kvp in this.resourcesDict)
        {
            OnResourceChanged?.Invoke(kvp.Key, kvp.Value.Amount);
        }
    }
}