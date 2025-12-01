using UnityEngine;

/// <summary>
/// ScriptableObject definition for shop items
/// </summary>
[CreateAssetMenu(fileName = "New Shop Item", menuName = "Exotic Harvest/Shop Item Definition")]
public class ShopItemDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string displayName;
    [TextArea(2, 4)] public string description;
    public Sprite icon;
    
    [Header("Shop Properties")]
    public EShopCategory category;
    public EItemType itemType;
    public ResourceCost cost;
    
    [Header("Availability")]
    public bool isUnlockedByDefault = true;
    public int playerLevelRequired = 1;
    public string[] prerequisiteItems; // IDs of items that must be purchased first
    
    [Header("Purchase Limits")]
    public bool hasLimitedQuantity = false;
    public int maxPurchases = 1;
    
    [Header("Item Effects")]
    public DecorationType decorationType; // For decoration items
    public ResourceType resourceType;     // For resource items
    public int resourceAmount = 1;        // Amount when purchasing resources
    public GameObject decorationPrefab;   // Prefab to spawn for decorations
    
    [Header("Visual")]
    public Color backgroundColor = Color.white;
    public bool showInShop = true;
    
    // Runtime properties
    public string ID => name; // Use ScriptableObject name as ID
    
    public bool IsUnlocked(int playerLevel, string[] purchasedItemIds)
    {
        if (!isUnlockedByDefault)
            return false;
            
        if (playerLevel < playerLevelRequired)
            return false;
            
        // Check prerequisites
        if (prerequisiteItems != null && prerequisiteItems.Length > 0)
        {
            foreach (var prereq in prerequisiteItems)
            {
                bool found = false;
                if (purchasedItemIds != null)
                {
                    foreach (var purchased in purchasedItemIds)
                    {
                        if (purchased == prereq)
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                    return false;
            }
        }
        
        return true;
    }
}