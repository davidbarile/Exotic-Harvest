using System;

/// <summary>
/// Represents a resource with type and quantity
/// </summary>
[System.Serializable]
public class Resource
{
    public ResourceType type;
    public int amount;
    
    public Resource(ResourceType type, int amount = 0)
    {
        this.type = type;
        this.amount = amount;
    }
    
    public void Add(int value)
    {
        amount += value;
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
        return new Resource(type, amount);
    }
}