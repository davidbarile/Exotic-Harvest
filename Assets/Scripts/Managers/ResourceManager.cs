using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all player resources and inventory
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager IN;
    
    [Header("ResourceData Database")]
    [SerializeField] private ResourceDatabase resourceDatabase;
    
    [Header("Inventory Settings")]
    [SerializeField] private int maxInventorySize = 1000000; // Total item limit across all resources
    
    private Dictionary<EResourceType, ResourceData> inventory = new();
    
    public ResourceDatabase Database => this.resourceDatabase;
    
    // Events for UI updates
    public static event Action<EResourceType, int> OnResourceChanged;
    public static event Action<EResourceType, int> OnResourceGained;
    public static event Action OnInventoryFull;

    public int DebugAddAmount = 10; // Amount to add when testing resource gain
    
    private void Awake()
    {
        InitializeInventory();
    }
    
    private void InitializeInventory()
    {
        // Initialize with 0 of each resource type
        foreach (EResourceType type in Enum.GetValues(typeof(EResourceType)))
        {
            this.inventory[type] = new ResourceData(type, 0);
        }
    }
    
    public bool HasResource(EResourceType type, int amount)
    {
        return this.inventory.ContainsKey(type) && this.inventory[type].Amount >= amount;
    }
    
    public int GetResourceAmount(EResourceType type)
    {
        return this.inventory.ContainsKey(type) ? this.inventory[type].Amount : 0;
    }
    
    public bool AddResource(EResourceType type, int amount)
    {
        if (GetTotalItemCount() + amount > this.maxInventorySize)
        {
            OnInventoryFull?.Invoke();
            return false;
        }
        
        if (!this.inventory.ContainsKey(type))
            this.inventory[type] = new ResourceData(type, 0);
        
        this.inventory[type].Add(amount);
        OnResourceChanged?.Invoke(type, this.inventory[type].Amount);
        OnResourceGained?.Invoke(type, amount);
        return true;
    }
    
    public bool SpendResources(ResourceCost cost)
    {
        if (!cost.CanAfford(this))
            return false;
            
        foreach (var resource in cost.RequiredResources)
        {
            this.inventory[resource.Type].Subtract(resource.Amount);
            OnResourceChanged?.Invoke(resource.Type, this.inventory[resource.Type].Amount);
        }
        return true;
    }
    
    public int GetTotalItemCount()
    {
        int total = 0;
        foreach (var resource in this.inventory.Values)
        {
            total += resource.Amount;
        }
        return total;
    }
    
    public Dictionary<EResourceType, ResourceData> GetAllResources()
    {
        return new(this.inventory);
    }
    
    // For save system
    public ResourceSaveData GetSaveData()
    {
        var saveData = new ResourceSaveData();
        foreach (var kvp in this.inventory)
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
            if (this.inventory.ContainsKey(resource.Type))
                this.inventory[resource.Type] = resource.Copy();
        }
        
        // Notify UI of all changes
        foreach (var kvp in this.inventory)
        {
            OnResourceChanged?.Invoke(kvp.Key, kvp.Value.Amount);
        }
    }
}