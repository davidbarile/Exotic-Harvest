using UnityEngine;
using Sirenix.OdinInspector;
using System;

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
    [HideIf("Category", EShopCategory.Resources)]
    public bool CanDragToWorld;
    [HideIf("Category", EShopCategory.Resources)]
    public DecorationData DecorationData;

    [Header("Inventory Item Data")]
    public int Quanity = 1;
    public int MaxStack = 1;

    [Header("Shop Properties")]
    public EShopCategory Category;

    [ShowIf("Category", EShopCategory.Resources)]
    public ResourceItemData[] ResourceItems;

    public bool IsTool => this.Category == EShopCategory.Tools;
    public bool IsResource => this.Category == EShopCategory.Resources;
    public bool IsDecoration => this.Category == EShopCategory.Decorations;

    public ResourceCost Cost;
    
    [Header("Purchase Limits")]
    public int MaxPurchases = -1;

    [Header("Visual")]
    public Color IconColor = Color.white;
    public Color BgColor = Color.white;
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


    public static ShopItemData CreateShopItemDataFromConfig(ShopItemConfig inConfig)
    {
        var shopItem = new ShopItemData(inConfig.ID, inConfig.DisplayName, inConfig.Category)
        {
            Description = inConfig.Description,
            Cost = inConfig.Cost,
            Quanity = inConfig.Quanity,
            MaxStack = inConfig.MaxStack,
            MaxPurchases = inConfig.MaxPurchases,
            ResourceItems = inConfig.ResourceItems,
            Icon = inConfig.Icon,
            IconColor = inConfig.IconColor,
            BgColor = inConfig.BgColor,
            CanDragToWorld = inConfig.CanDragToWorld,
            DecorationData = DecorationData.Copy(inConfig.DecorationData)
        };

        return shopItem;
    }
    
    [Serializable]
    public class ResourceItemData
    {
        public EResourceType ResourceType;
        public int Amount;
    }
}