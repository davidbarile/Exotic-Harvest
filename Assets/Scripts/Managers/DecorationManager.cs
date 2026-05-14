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

    public List<DecorationBase> PlacedDecorations = new();

    private List<DecorationBase> initDecorations = new();
    private Dictionary<int, Transform> stoolsAndBenches = new();
    private List<InventoryItemData> itemsWithoutParents = new();

    private void Awake()
    {
        InitDecorationParents();
    }

    /// <summary>
    /// New Game Initialization - can be used to set up any necessary state or spawn default decorations in the world
    /// </summary>
    public void InitDecorationsInWorld(bool isNewGame)
    {
        this.initDecorations = new List<DecorationBase>(this.worldDecorationCanvas.GetComponentsInChildren<DecorationBase>());
        var screenDecorations = new List<DecorationBase>(this.screenDecorationCanvas.GetComponentsInChildren<DecorationBase>());

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
            this.initDecorations = new List<DecorationBase>();
    }

    public DecorationBase SpawnItemInWorld(InventoryItemData itemData, Vector3 spawnPosition, Transform parent = null)
    {
        if (itemData == null || string.IsNullOrEmpty(itemData.DecorationData.PrefabName))
            return null;

        var worldItem = PrefabManager.IN.SpawnPrefab<DecorationBase>(itemData.DecorationData.PrefabName, parent ?? DragManager.IN.WorldDecorationsContainer);
        Debug.Log($"Spawning item in world: {itemData.DisplayName} at position {spawnPosition} with parent {(parent != null ? parent.name : "null")}", worldItem.gameObject);

        worldItem.transform.localPosition = spawnPosition;
        worldItem.transform.localScale = Vector3.one;
        worldItem.name = $"Decoration_{itemData.DisplayName}";
        worldItem.ConfigureFromDrag(itemData, Vector2.zero);

        if(worldItem.ItemData.DecorationData.IsDragZone && worldItem.ItemData.DecorationData.Guid == -1)
        {
            worldItem.ItemData.DecorationData.Guid = UnityEngine.Random.Range(0, int.MaxValue);
        }
        return worldItem;
    }
    
    private void InitDecorationParents()
    {
        var parentObjects = this.worldDecorationCanvas.GetComponentsInChildren<Transform>(true);
        foreach (var parent in parentObjects)
        {
            if (parent.TryGetComponent<DragTarget>(out var dragTarget))
            {
                this.decorationParents.Add(parent.GetInstanceID(), parent);
            }
        }

        var screenParentObjects = this.screenDecorationCanvas.GetComponentsInChildren<Transform>(true);
        foreach (var parent in screenParentObjects)
        {
            if (parent.TryGetComponent<DragTarget>(out var dragTarget))
            {
                this.decorationParents.Add(parent.GetInstanceID(), parent);
            }
        }
    }
    
    public void ApplyFromSaveData(List<InventoryItemData> savedWorldItems)
    {
        // Clear existing decorations
        for (int i = this.PlacedDecorations.Count - 1; i >= 0; i--)
        {
            var decoration = this.PlacedDecorations[i];
            if (decoration != null)
                Destroy(decoration.gameObject);
        }
        this.PlacedDecorations.Clear();
        this.stoolsAndBenches.Clear();
        this.itemsWithoutParents.Clear();

        // Recreate decorations from save data
        foreach (var data in savedWorldItems)
        {
            if (this.decorationParents.TryGetValue(data.DecorationData.WorldSaveData.ParentGuid, out var foundParent))
            {
                var decoration = SpawnItemInWorld(data, data.DecorationData.WorldSaveData.WorldPosition, foundParent);
                decoration.transform.SetSiblingIndex(data.DecorationData.WorldSaveData.SiblingIndex);
                this.PlacedDecorations.Add(decoration);

                if (decoration.ItemData.DecorationData.IsDragZone)
                {
                    var childDragTarget = decoration.GetComponentInChildren<DragTarget>();
                    if (childDragTarget != null)
                        this.stoolsAndBenches.Add(decoration.ItemData.DecorationData.Guid, childDragTarget.transform);
                }
            }
            else
                this.itemsWithoutParents.Add(data);
        }

        //with remaining items, loop thru spawned decorations and see if any match as a parent, then spawn remaining items
        foreach (var data in this.itemsWithoutParents)
        {
            Transform parentTrans = null;
            if (this.stoolsAndBenches.TryGetValue(data.DecorationData.WorldSaveData.ParentGuid, out var foundParent))
            {
                var childDragTarget = foundParent.GetComponentInChildren<DragTarget>();
                if (childDragTarget != null)
                    parentTrans = childDragTarget.transform;
            }

            if (parentTrans == null)
            {
                Debug.Log($"<color=red>No parent found for {data.DisplayName} with ParentGuid {data.DecorationData.WorldSaveData.ParentGuid}. Spawning under worldDecorationCanvas.</color>");
                parentTrans = this.worldDecorationCanvas.transform;
            }
                
            var decoration = SpawnItemInWorld(data, data.DecorationData.WorldSaveData.WorldPosition, parentTrans);
            decoration.transform.SetSiblingIndex(data.DecorationData.WorldSaveData.SiblingIndex);
            this.PlacedDecorations.Add(decoration);
        }
    }
}