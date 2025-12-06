using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Database of all shop item definitions
/// </summary>
[CreateAssetMenu(fileName = "ShopDatabase", menuName = "Exotic Harvest/Shop Database")]
public class ShopDatabase : ScriptableObject
{
    [Header("All Shop Items")]
    [SerializeField] private ShopItemConfig[] allShopItems;
    
    private Dictionary<string, ShopItemConfig> itemLookup;
    private Dictionary<EShopCategory, List<ShopItemConfig>> itemsByCategory;
    private Dictionary<EItemType, List<ShopItemConfig>> itemsByType;
    
    public ShopItemConfig[] AllShopItems => this.allShopItems;
    
    private void OnEnable()
    {
        BuildLookupTables();
    }
    
    private void OnValidate()
    {
        BuildLookupTables();
    }
    
    private void BuildLookupTables()
    {
        if (this.allShopItems == null) return;
        
        this.itemLookup = new Dictionary<string, ShopItemConfig>();
        this.itemsByCategory = new Dictionary<EShopCategory, List<ShopItemConfig>>();
        this.itemsByType = new Dictionary<EItemType, List<ShopItemConfig>>();
        
        // Initialize category lists
        foreach (EShopCategory category in System.Enum.GetValues(typeof(EShopCategory)))
        {
            this.itemsByCategory[category] = new List<ShopItemConfig>();
        }
        
        // Initialize type lists  
        foreach (EItemType type in System.Enum.GetValues(typeof(EItemType)))
        {
            this.itemsByType[type] = new List<ShopItemConfig>();
        }
        
        foreach (var item in this.allShopItems)
        {
            if (item != null)
            {
                this.itemLookup[item.ID] = item;
                this.itemsByCategory[item.category].Add(item);
                this.itemsByType[item.itemType].Add(item);
            }
        }
    }
    
    public ShopItemConfig GetShopItem(string id)
    {
        if (this.itemLookup == null) BuildLookupTables();
        this.itemLookup.TryGetValue(id, out ShopItemConfig item);
        return item;
    }
    
    public ShopItemConfig[] GetItemsByCategory(EShopCategory category)
    {
        if (this.allShopItems == null) return new ShopItemConfig[0];
        if (this.itemsByCategory == null) BuildLookupTables();
        
        if (this.itemsByCategory.TryGetValue(category, out List<ShopItemConfig> items))
            return items.ToArray();
        return new ShopItemConfig[0];
    }
    
    public ShopItemConfig[] GetItemsByType(EItemType type)
    {
        if (this.allShopItems == null) return new ShopItemConfig[0];
        if (this.itemsByType == null) BuildLookupTables();
        
        if (this.itemsByType.TryGetValue(type, out List<ShopItemConfig> items))
            return items.ToArray();
        return new ShopItemConfig[0];
    }
    
    public ShopItemConfig[] GetAvailableItems(EShopCategory category, int playerLevel = 1, string[] purchasedItemIds = null)
    {
        var categoryItems = GetItemsByCategory(category);
        return categoryItems.Where(item => item != null && item.IsUnlocked(playerLevel, purchasedItemIds ?? new string[0])).ToArray();
    }
    
    public ShopItemConfig[] GetDecorationItems()
    {
        return GetItemsByType(EItemType.Decoration);
    }
    
    public ShopItemConfig[] GetResourceItems()
    {
        return GetItemsByType(EItemType.Resource);
    }
    
    public ShopItemConfig[] GetUnlockedItems(int playerLevel = 1, string[] purchasedItemIds = null)
    {
        if (this.allShopItems == null) return new ShopItemConfig[0];
        return this.allShopItems.Where(item => item != null && item.IsUnlocked(playerLevel, purchasedItemIds ?? new string[0])).ToArray();
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-Populate Shop Items")]
    private void AutoPopulateShopItems()
    {
        // This would be called in editor to automatically find all ShopItemConfig assets
        var shopItemGuids = UnityEditor.AssetDatabase.FindAssets("t:ShopItemConfig");
        var foundItems = new List<ShopItemConfig>();

        foreach (var guid in shopItemGuids)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var item = UnityEditor.AssetDatabase.LoadAssetAtPath<ShopItemConfig>(path);
            if (item != null)
                foundItems.Add(item);
        }

        this.allShopItems = foundItems.ToArray();
        BuildLookupTables();

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Auto-populated {this.allShopItems.Length} shop items");
    }
#endif
}