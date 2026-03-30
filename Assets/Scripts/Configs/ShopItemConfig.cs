using UnityEngine;

/// <summary>
/// ScriptableObject definition for shop items
/// </summary>
[CreateAssetMenu(fileName = "New Shop Item", menuName = "Exotic Harvest/Shop Item Definition")]
public class ShopItemConfig : ScriptableObject
{
    public string ID; // Unique identifier, can be auto-generated from name or set manually
    [Header("Basic Info")]
     [TextArea(1, 4)] public string DisplayName;
    [TextArea(2, 4)] public string Description;
    public Sprite Icon;
    public float Scale = 1f;

    [Header("Decoration Data")]
    public bool CanDragToWorld;
    public DecorationData DecorationData;

    [Header("Inventory Item Data")]
    public int Quanity = 1;
    public int MaxStack = 1;

    [Header("Shop Properties")]
    public EShopCategory Category;
    public bool IsTool => this.Category == EShopCategory.Tools;
    public bool IsResource => this.Category == EShopCategory.Resources;
    public bool IsDecoration => this.Category == EShopCategory.Decorations;
    public ResourceCost Cost;
    
    [Header("Availability")]
    public bool IsUnlockedByDefault = true;
    public int PlayerLevelRequired = 1;
    public string[] PrerequisiteItems = new string[0]; // IDs of items that must be purchased first
    
    [Header("Purchase Limits")]
    public bool HasLimitedQuantity = false;
    public int MaxPurchases = 1;
    
    [Header("Item Effects")]
    public EDecorationType DecorationType; // For decoration items
    public EResourceType ResourceType;     // For resource items
    public int ResourceAmount = 0;        // Amount when purchasing resources
    public GameObject DecorationPrefab;   // Prefab to spawn for decorations
    
    [Header("Visual")]
    public Color BackgroundColor = Color.white;
    public bool ShowInShop = true;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(this.ID))
            return;

        this.ID = this.name;
        //this.ID = this.name.Substring(this.name.IndexOf("_") + 1, this.name.Length - this.name.IndexOf("_") - 1).Trim();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    public bool IsUnlocked(int playerLevel, string[] purchasedItemIds)
    {
        if (!this.IsUnlockedByDefault)
            return false;

        if (playerLevel < this.PlayerLevelRequired)
            return false;

        // Check prerequisites
        foreach (var prereq in this.PrerequisiteItems)
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

        return true;
    }
    
    public static ShopItemData CreateShopItemDataFromConfig(ShopItemConfig inConfig)
    {
        var shopItem = new ShopItemData(inConfig.ID, inConfig.DisplayName, inConfig.Category)
        {
            Description = inConfig.Description,
            Cost = inConfig.Cost,
            IsUnlocked = inConfig.IsUnlockedByDefault,
            IsLimitedQuantity = inConfig.HasLimitedQuantity,
            Quanity = inConfig.Quanity,
            MaxStack = inConfig.MaxStack,
            MaxPurchases = inConfig.MaxPurchases,
            DecorationType = inConfig.DecorationType,
            ResourceType = inConfig.ResourceType,
            ResourceAmount = inConfig.ResourceAmount,
            Icon = inConfig.Icon,
            DecorationData = DecorationData.Copy(inConfig.DecorationData)
        };
        
        return shopItem;
    }
}