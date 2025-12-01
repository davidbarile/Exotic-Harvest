using UnityEngine;

/// <summary>
/// Represents an item that can be purchased in the shop
/// </summary>
[System.Serializable]
public class ShopItem
{
    [Header("Item Identity")]
    public string id;
    public string displayName;
    [TextArea(2, 4)] public string description;
    public Sprite icon;
    
    [Header("Item Properties")]
    public ShopCategory category;
    public ItemType itemType;
    public ResourceCost cost;
    
    [Header("Purchase Rules")]
    public bool isUnlocked = true;
    public bool isLimitedQuantity = false;
    public int maxPurchases = 1;
    public int currentPurchases = 0;
    
    [Header("Item Data")]
    public DecorationType decorationType; // For decoration items
    public ResourceType resourceType;     // For resource items
    public int resourceAmount = 1;        // Amount when purchasing resources
    
    // Properties
    public bool CanPurchase => isUnlocked && (!isLimitedQuantity || currentPurchases < maxPurchases);
    public bool IsMaxedOut => isLimitedQuantity && currentPurchases >= maxPurchases;
    public int RemainingPurchases => isLimitedQuantity ? maxPurchases - currentPurchases : -1;
    
    public ShopItem(string id, string name, ShopCategory category, ItemType type)
    {
        this.id = id;
        this.displayName = name;
        this.category = category;
        this.itemType = type;
        this.cost = new ResourceCost();
    }
    
    public bool TryPurchase()
    {
        if (!CanPurchase)
            return false;
            
        if (isLimitedQuantity)
            currentPurchases++;
            
        return true;
    }
    
    public void ResetPurchases()
    {
        currentPurchases = 0;
    }
}