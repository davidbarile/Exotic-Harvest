using System;
using UnityEngine;

/// <summary>
/// Represents a resource with type and quantity (runtime data)
/// </summary>
[Serializable]
public class ResourceData
{
    public ResourceType Type;
    public int Amount;
    
    // Cache reference to config (not serialized)
    [System.NonSerialized] private ResourceConfig cachedConfig;
    
    public ResourceData(ResourceType type, int amount = 0)
    {
        this.Type = type;
        this.Amount = amount;
    }
    
    public ResourceData(ResourceConfig config, int amount = 0)
    {
        this.Type = config.ResourceType;
        this.Amount = amount;
        this.cachedConfig = config;
    }
    
    public ResourceConfig GetDefinition()
    {
        if (this.cachedConfig == null && ResourceManager.IN?.Database != null)
        {
            this.cachedConfig = ResourceManager.IN.Database.GetResource(this.Type);
        }
        return this.cachedConfig;
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
    
    public ResourceData Copy()
    {
        var copy = new ResourceData(this.Type, this.Amount);
        copy.cachedConfig = this.cachedConfig;
        return copy;
    }
    
    // Convenience properties that use config
    public string DisplayName => GetDefinition()?.DisplayName ?? Type.ToString();
    public string Description => GetDefinition()?.Description ?? "";
    public Sprite Icon => GetDefinition()?.Icon;
    public Color UIColor => GetDefinition()?.UiColor ?? Color.white;
    public int BaseValue => GetDefinition()?.BaseValue ?? 1;
}