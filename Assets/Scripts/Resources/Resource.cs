using System;
using UnityEngine;

/// <summary>
/// Represents a resource with type and quantity (runtime data)
/// </summary>
[Serializable]
public class Resource
{
    public ResourceType Type;
    public int Amount;
    
    // Cache reference to definition (not serialized)
    [System.NonSerialized] private ResourceDefinition cachedDefinition;
    
    public Resource(ResourceType type, int amount = 0)
    {
        this.Type = type;
        this.Amount = amount;
    }
    
    public Resource(ResourceDefinition definition, int amount = 0)
    {
        this.Type = definition.ResourceType;
        this.Amount = amount;
        this.cachedDefinition = definition;
    }
    
    public ResourceDefinition GetDefinition()
    {
        if (this.cachedDefinition == null && ResourceManager.IN?.Database != null)
        {
            this.cachedDefinition = ResourceManager.IN.Database.GetResource(this.Type);
        }
        return this.cachedDefinition;
    }
    
    public void Add(int value)
    {
        var definition = GetDefinition();
        int maxAmount = definition?.MaxStackSize ?? 999;
        this.Amount = Mathf.Min(this.Amount + value, maxAmount);
    }
    
    public bool CanSubtract(int value)
    {
        return this.Amount >= value;
    }
    
    public bool Subtract(int value)
    {
        if (CanSubtract(value))
        {
            this.Amount -= value;
            return true;
        }
        return false;
    }
    
    public Resource Copy()
    {
        var copy = new Resource(this.Type, this.Amount);
        copy.cachedDefinition = this.cachedDefinition;
        return copy;
    }
    
    // Convenience properties that use definition
    public string DisplayName => GetDefinition()?.DisplayName ?? Type.ToString();
    public string Description => GetDefinition()?.Description ?? "";
    public Sprite Icon => GetDefinition()?.Icon;
    public Color UIColor => GetDefinition()?.UiColor ?? Color.white;
    public int BaseValue => GetDefinition()?.BaseValue ?? 1;
}