using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all player resources and inventory
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager IN;
    
    [Header("Resource Database")]
    [SerializeField] private ResourceDatabase resourceDatabase;
    
    [Header("Inventory Settings")]
    [SerializeField] private int maxInventorySize = 100; // Total item limit across all resources
    
    private Dictionary<ResourceType, Resource> inventory = new();
    
    public ResourceDatabase Database => this.resourceDatabase;
    
    // Events for UI updates
    public static event Action<ResourceType, int> OnResourceChanged;
    public static event Action<ResourceType, int> OnResourceGained;
    public static event Action OnInventoryFull;
    
    private void Awake()
    {
        InitializeInventory();
    }
    
    private void InitializeInventory()
    {
        // Initialize with 0 of each resource type
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            this.inventory[type] = new Resource(type, 0);
        }
    }
    
    public bool HasResource(ResourceType type, int amount)
    {
        return this.inventory.ContainsKey(type) && this.inventory[type].amount >= amount;
    }
    
    public int GetResourceAmount(ResourceType type)
    {
        return this.inventory.ContainsKey(type) ? this.inventory[type].amount : 0;
    }
    
    public bool AddResource(ResourceType type, int amount)
    {
        if (GetTotalItemCount() + amount > this.maxInventorySize)
        {
            OnInventoryFull?.Invoke();
            return false;
        }
        
        if (!this.inventory.ContainsKey(type))
            this.inventory[type] = new Resource(type, 0);
        
        this.inventory[type].Add(amount);
        OnResourceChanged?.Invoke(type, this.inventory[type].amount);
        OnResourceGained?.Invoke(type, amount);
        return true;
    }
    
    public bool SpendResources(ResourceCost cost)
    {
        if (!cost.CanAfford(this))
            return false;
            
        foreach (var resource in cost.RequiredResources)
        {
            this.inventory[resource.type].Subtract(resource.amount);
            OnResourceChanged?.Invoke(resource.type, this.inventory[resource.type].amount);
        }
        return true;
    }
    
    public int GetTotalItemCount()
    {
        int total = 0;
        foreach (var resource in this.inventory.Values)
        {
            total += resource.amount;
        }
        return total;
    }
    
    public Dictionary<ResourceType, Resource> GetAllResources()
    {
        return new(this.inventory);
    }
    
    // For save system
    public ResourceData GetSaveData()
    {
        var saveData = new ResourceData();
        foreach (var kvp in this.inventory)
        {
            if (kvp.Value.amount > 0)
                saveData.resources.Add(kvp.Value.Copy());
        }
        saveData.maxInventorySize = this.maxInventorySize;
        return saveData;
    }
    
    public void LoadSaveData(ResourceData saveData)
    {
        InitializeInventory(); // Reset to 0
        
        foreach (var resource in saveData.resources)
        {
            if (this.inventory.ContainsKey(resource.type))
                this.inventory[resource.type] = resource.Copy();
        }
        
        this.maxInventorySize = saveData.maxInventorySize;
        
        // Notify UI of all changes
        foreach (var kvp in this.inventory)
        {
            OnResourceChanged?.Invoke(kvp.Key, kvp.Value.amount);
        }
    }
}