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
    [SerializeField] private ShopItemConfig[] allShopItems = new ShopItemConfig[0];
    
    private Dictionary<string, ShopItemConfig> itemLookupDict = new();
    private Dictionary<EShopCategory, List<ShopItemConfig>> itemsByCategoryDict = new();
    
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
        this.itemLookupDict = new();
        this.itemsByCategoryDict = new();
        
        // Initialize category lists
        foreach (EShopCategory category in System.Enum.GetValues(typeof(EShopCategory)))
        {
            this.itemsByCategoryDict[category] = new();
        }
        
        foreach (var item in this.allShopItems)
        {
             this.itemLookupDict[item.ID] = item;
            this.itemsByCategoryDict[item.Category].Add(item);
        }
    }
    
    public ShopItemConfig GetShopItem(string id)
    {
        if (this.itemLookupDict == null) BuildLookupTables();

        this.itemLookupDict.TryGetValue(id, out ShopItemConfig item);
        return item;
    }
    
    public ShopItemConfig[] GetItemsByCategory(EShopCategory category)
    {
        if (this.itemsByCategoryDict == null) BuildLookupTables();
        
        if (this.itemsByCategoryDict.TryGetValue(category, out List<ShopItemConfig> items))
            return items.ToArray();
        return new ShopItemConfig[0];
    }
    
    public ShopItemConfig[] GetDecorationItems()
    {
        return GetItemsByCategory(EShopCategory.Decorations);
    }
    
    public ShopItemConfig[] GetResourceItems()
    {
        return GetItemsByCategory(EShopCategory.Resources);
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