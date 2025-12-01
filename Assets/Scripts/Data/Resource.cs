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
        if (cachedDefinition == null && ResourceManager.IN?.Database != null)
        {
            cachedDefinition = ResourceManager.IN.Database.GetResource(type);
        }
        return cachedDefinition;
    }
    
    public void Add(int value)
    {
        var definition = GetDefinition();
        int maxAmount = definition?.maxStackSize ?? 999;
        amount = Mathf.Min(amount + value, maxAmount);
    }
    
    public bool CanSubtract(int value)
    {
        return amount >= value;
    }
    
    public bool Subtract(int value)
    {
        if (CanSubtract(value))
        {
            amount -= value;
            return true;
        }
        return false;
    }
    
    public Resource Copy()
    {
        var copy = new Resource(type, amount);
        copy.cachedDefinition = cachedDefinition;
        return copy;
    }
    
    // Convenience properties that use definition
    public string DisplayName => GetDefinition()?.displayName ?? type.ToString();
    public string Description => GetDefinition()?.description ?? "";
    public Sprite Icon => GetDefinition()?.icon;
    public Color UIColor => GetDefinition()?.uiColor ?? Color.white;
    public int BaseValue => GetDefinition()?.baseValue ?? 1;
}