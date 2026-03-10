using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all decorations on the desktop
/// </summary>
public class DecorationManager : MonoBehaviour
{
    public static DecorationManager IN;
    
    // [Header("Decoration Prefabs")]
    // [SerializeField] private GameObject bucketPrefab;
    // [SerializeField] private GameObject plantPrefab;
    // [SerializeField] private GameObject[] allDecorationPrefabs; // Array for all decoration types
    
    [Header("UI Placement Settings")]
    [SerializeField] private RectTransform decorationCanvas; // Canvas for decorations
    [SerializeField] private Vector2 placementPadding = new Vector2(100f, 100f); // Padding from edges
    [SerializeField] private float gridSpacing = 80f; // UI spacing
    [SerializeField] private bool useGridPlacement = true;
    
    private Dictionary<EDecorationType, GameObject> decorationPrefabs;
    private List<UiDecorationBase> placedDecorations = new();

    // Events
    // public static Action<DecorationBase> OnDecorationPlaced;
    // public static Action<DecorationBase> OnDecorationAdded;
    // public static Action<DecorationBase> OnDecorationRemoved;
    // public static Action<int> OnDecorationCountChanged;
    
    private void Awake()
    {
        InitializePrefabs();
    }
    
    // private void OnEnable()
    // {
    //     DecorationBase.OnDecorationPlaced += OnDecorationPlaced;
    //     DecorationBase.OnDecorationRemoved += OnDecorationRemoved;
    // }

    // private void OnDisable()
    // {
    //     DecorationBase.OnDecorationPlaced -= OnDecorationPlaced;
    //     DecorationBase.OnDecorationRemoved -= OnDecorationRemoved;
    // }
    
    public UiDecorationBase SpawnItemInWorld(InventoryItemData itemData, Vector3 position)
    {
        if (itemData == null || string.IsNullOrEmpty(itemData.WorldPrefabName))
            return null;

        var prefab = Resources.Load<UiDecorationBase>($"Prefabs/WorldItems/{itemData.WorldPrefabName}");
        if (prefab != null)
        {
            var worldItem = Instantiate(prefab, position, Quaternion.identity, DragManager.IN.DefaultParent);
            worldItem.transform.localScale = Vector3.one;
            worldItem.name = $"WorldItem_{itemData.DisplayName}";
            worldItem.InitializeFromDrag(itemData, Vector2.zero);
            return worldItem;
        }

        Debug.LogError($"Failed to load world prefab for item: {itemData.DisplayName} at path: Prefabs/WorldItems/{itemData.WorldPrefabName}");

        return null;
    }
    
    private void InitializePrefabs()
    {
        this.decorationPrefabs = new();
        
        // if (this.bucketPrefab != null)
        //     this.decorationPrefabs[EDecorationType.Bucket] = this.bucketPrefab;
        // if (this.plantPrefab != null)
        //     this.decorationPrefabs[EDecorationType.Plant] = this.plantPrefab;
    }
    
    public List<UiDecorationBase> GetAllDecorations()
    {
        return new(this.placedDecorations);
    }
    
    // For save system
    public List<DecorationData> GetSaveData()
    {
        List<DecorationData> saveData = new();
        foreach (var decoration in this.placedDecorations)
        {
            if (decoration != null)
                saveData.Add(decoration.GetSaveData());
        }
        return saveData;
    }
    
    public void LoadSaveData(List<DecorationData> saveData)
    {
        // Clear existing decorations
        for (int i = this.placedDecorations.Count - 1; i >= 0; i--)
        {
            if (this.placedDecorations[i] != null)
                Destroy(this.placedDecorations[i].gameObject);
        }
        this.placedDecorations.Clear();
        
        // Recreate decorations from save data
        foreach (var data in saveData)
        {
            // DecorationBase decoration = PlaceDecoration(data.Type, data.WorldPosition);
            // if (decoration != null)
            //     decoration.LoadSaveData(data);
        }
    }
}