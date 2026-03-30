using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all decorations on the desktop
/// </summary>
public class DecorationManager : MonoBehaviour
{
    public static DecorationManager IN;

    [Header("UI Placement Settings")]
    [SerializeField] private RectTransform worldDecorationCanvas, screenDecorationCanvas; // Canvas for decorations
    
    private Dictionary<int,Transform> decorationParents = new(); // List of parent transforms for different decoration types

    public List<UiDecorationBase> PlacedDecorations = new();
    
    private List<UiDecorationBase> initDecorations = new();

    private void Awake()
    {
        InitDecorationParents();
    }

    /// <summary>
    /// New Game Initialization - can be used to set up any necessary state or spawn default decorations in the world
    /// </summary>
    public void InitDecorationsInWorld(bool isNewGame)
    {
        this.initDecorations = new List<UiDecorationBase>(this.worldDecorationCanvas.GetComponentsInChildren<UiDecorationBase>());
        var screenDecorations = new List<UiDecorationBase>(this.screenDecorationCanvas.GetComponentsInChildren<UiDecorationBase>());

        this.initDecorations.AddRange(screenDecorations);

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
            this.initDecorations = new List<UiDecorationBase>();
    }

    public UiDecorationBase SpawnItemInWorld(InventoryItemData itemData, Vector3 spawnPosition, Transform parent = null)
    {
        if (itemData == null || string.IsNullOrEmpty(itemData.DecorationData.PrefabName))
            return null;

        var worldItem = PrefabManager.IN.SpawnPrefab<UiDecorationBase>(itemData.DecorationData.PrefabName, parent ?? DragManager.IN.DefaultParent);
        worldItem.transform.localPosition = spawnPosition;
        worldItem.transform.localScale = Vector3.one;
        worldItem.name = $"Decoration_{itemData.DisplayName}";
        worldItem.ConfigureFromDrag(itemData, Vector2.zero);
        return worldItem;
    }
    
    private void InitDecorationParents()
    {
        var parentObjects = this.worldDecorationCanvas.GetComponentsInChildren<Transform>(true);
        foreach (var parent in parentObjects)
        {
            if (parent.TryGetComponent<UiDragTarget>(out var dragTarget))
            {
                this.decorationParents.Add(parent.GetInstanceID(), parent);
            }
        }
    }
    
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