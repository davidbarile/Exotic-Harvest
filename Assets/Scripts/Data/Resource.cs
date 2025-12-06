using System;
using UnityEngine;

/// <summary>
/// Represents a resource with type and quantity (runtime data)
/// </summary>
[System.Serializable]
public class Resource
{
    public ResourceType type;
    public int amount;
    
    // Cache reference to definition (not serialized)
    [System.NonSerialized] private ResourceDefinition cachedDefinition;
    
    public Resource(ResourceType type, int amount = 0)
    {
        this.type = type;
        this.amount = amount;
    }
    
    public Resource(ResourceDefinition definition, int amount = 0)
    {
        this.type = definition.resourceType;
        this.amount = amount;
        this.cachedDefinition = definition;
    }
    
    public ResourceDefinition GetDefinition()
    {
        if (this.cachedDefinition == null && ResourceManager.IN?.Database != null)
        {
            this.cachedDefinition = ResourceManager.IN.Database.GetResource(this.type);
        }
        return this.cachedDefinition;
    }
    
    public void Add(int value)
    {
        var definition = GetDefinition();
        int maxAmount = definition?.maxStackSize ?? 999;
        this.amount = Mathf.Min(this.amount + value, maxAmount);
    }
    
    public bool CanSubtract(int value)
    {
        return this.amount >= value;
    }
    
    public bool Subtract(int value)
    {
        if (CanSubtract(value))
        {
            this.amount -= value;
            return true;
        }
        return false;
    }
    
    public Resource Copy()
    {
        var copy = new Resource(this.type, this.amount);
        copy.cachedDefinition = this.cachedDefinition;
        return copy;
    }
    
    // Convenience properties that use definition
    public string DisplayName => GetDefinition()?.displayName ?? type.ToString();
    public string Description => GetDefinition()?.description ?? "";
    public Sprite Icon => GetDefinition()?.icon;
    public Color UIColor => GetDefinition()?.uiColor ?? Color.white;
    public int BaseValue => GetDefinition()?.baseValue ?? 1;
}