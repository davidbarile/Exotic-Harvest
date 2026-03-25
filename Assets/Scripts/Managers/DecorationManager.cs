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
    
    private Dictionary<int,Transform> decorationParents = new(); // List of parent transforms for different decoration types

    // private Dictionary<EDecorationType, GameObject> decorationPrefabs;
    public List<UiDecorationBase> PlacedDecorations = new();
    
    private UiDecorationBase[] initDecorations = new UiDecorationBase[0];

    // Events
    // public static Action<DecorationBase> OnDecorationPlaced;
    // public static Action<DecorationBase> OnDecorationAdded;
    // public static Action<DecorationBase> OnDecorationRemoved;
    // public static Action<int> OnDecorationCountChanged;

    private void Awake()
    {
        InitDecorationParents();
    }

    /// <summary>
    /// New Game Initialization - can be used to set up any necessary state or spawn default decorations in the world
    /// </summary>
    public void InitDecorationsInWorld(bool isNewGame)
    {
        this.initDecorations = this.decorationCanvas.GetComponentsInChildren<UiDecorationBase>();
        foreach (var decoration in this.initDecorations)
        {
            if (decoration != null)
            {
                if (isNewGame)
                {
                    decoration.InitWorldPositionAndParent();
                    this.PlacedDecorations.Add(decoration);
                    SaveManager.Data.WorldItems.Add(decoration.ItemData);
                }
                else
                {
                    Destroy(decoration.gameObject);
                }
            }
        }
        
        if(!isNewGame)
            this.initDecorations = new UiDecorationBase[0];
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

    public UiDecorationBase SpawnItemInWorld(InventoryItemData itemData, Vector3 spawnPosition, Transform parent = null)
    {
        if (itemData == null || string.IsNullOrEmpty(itemData.DecorationData.PrefabName))
            return null;

        var worldItem = PrefabManager.IN.SpawnPrefab<UiDecorationBase>(itemData.DecorationData.PrefabName, parent ?? DragManager.IN.DefaultParent);
        worldItem.transform.localPosition = spawnPosition;
        worldItem.transform.localScale = Vector3.one;
        worldItem.name = $"Decoration_{itemData.DisplayName}";
        worldItem.InitializeFromDrag(itemData, Vector2.zero);
        return worldItem;
    }
    
    private void InitDecorationParents()
    {
        var parentObjects = this.decorationCanvas.GetComponentsInChildren<Transform>(true);
        foreach (var parent in parentObjects)
        {
            if (parent.TryGetComponent<UiDragTarget>(out var dragTarget))
            {
                this.decorationParents.Add(parent.GetInstanceID(), parent);
            }
        }
    }

    // For save system
    // public List<InventoryItemData> GetSaveData()
    // {
    //     var saveData = new List<InventoryItemData>();

    //     foreach (var decoration in this.PlacedDecorations)
    //     {
    //         if (decoration != null)
    //             saveData.Add(decoration.ItemData);
    //     }

    //     return saveData;
    // }
    
    public void LoadFromSaveData(List<InventoryItemData> savedWorldItems)
    {
        // Clear existing decorations
        for (int i = this.PlacedDecorations.Count - 1; i >= 0; i--)
        {
            var decoration = this.PlacedDecorations[i];
            if (decoration != null)
                Destroy(decoration.gameObject);
        }
        this.PlacedDecorations.Clear();
        
        // Recreate decorations from save data
        foreach (var data in savedWorldItems)
        {
            Transform parentTrans = null;
            if (this.decorationParents.TryGetValue(data.DecorationData.ParentGuid, out var foundParent))
            {
                parentTrans = foundParent;
            }

            var decoration = SpawnItemInWorld(data, data.DecorationData.WorldPosition, parentTrans);
            if (decoration != null)
            {
                decoration.transform.SetSiblingIndex(data.DecorationData.SiblingIndex);
                this.PlacedDecorations.Add(decoration);
            }
        }
    }
}