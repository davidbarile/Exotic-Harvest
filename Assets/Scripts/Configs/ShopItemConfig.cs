using UnityEngine;

/// <summary>
/// ScriptableObject definition for shop items
/// </summary>
[CreateAssetMenu(fileName = "New Shop Item", menuName = "Exotic Harvest/Shop Item Definition")]
public class ShopItemConfig : ScriptableObject
{
    [Header("Basic Info")]
    public string DisplayName;
    [TextArea(2, 4)] public string Description;
    public Sprite Icon;
    
    [Header("Shop Properties")]
    public EShopCategory Category;
    public EItemType ItemType;
    public ResourceCost Cost;
    
    [Header("Availability")]
    public bool IsUnlockedByDefault = true;
    public int PlayerLevelRequired = 1;
    public string[] PrerequisiteItems; // IDs of items that must be purchased first
    
    [Header("Purchase Limits")]
    public bool HasLimitedQuantity = false;
    public int MaxPurchases = 1;
    
    [Header("Item Effects")]
    public DecorationType DecorationType; // For decoration items
    public ResourceType ResourceType;     // For resource items
    public int ResourceAmount = 1;        // Amount when purchasing resources
    public GameObject DecorationPrefab;   // Prefab to spawn for decorations
    
    [Header("Visual")]
    public Color BackgroundColor = Color.white;
    public bool ShowInShop = true;
    
    // Runtime properties
    public string ID => name; // Use ScriptableObject name as ID
    
    public bool IsUnlocked(int playerLevel, string[] purchasedItemIds)
    {
        if (!IsUnlockedByDefault)
            return false;
            
        if (playerLevel < PlayerLevelRequired)
            return false;
            
        // Check prerequisites
        if (PrerequisiteItems != null && PrerequisiteItems.Length > 0)
        {
            foreach (var prereq in PrerequisiteItems)
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