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

    public DecorationBase SpawnItemInWorld(InventoryItemData inItemData, Vector3 inSpawnPosition, Transform inParent = null)
    {
        if (inItemData == null || string.IsNullOrEmpty(inItemData.DecorationData.PrefabName))
            return null;

        var worldItem = PrefabManager.IN.SpawnPrefab<DecorationBase>(inItemData.DecorationData.PrefabName, inParent ?? DragManager.IN.WorldDecorationsContainer);
       
        worldItem.transform.localPosition = inSpawnPosition;
        worldItem.transform.localRotation = Quaternion.identity;

        var prefabName = inItemData.DisplayName.Replace("\n", "");
        var decType = inItemData.DecorationData.DecorationType.ToString();
        if (decType.Contains("General"))
            worldItem.name = $"Decoration_{prefabName}*";
        else
            worldItem.name = $"{inItemData.DecorationData.PrefabName}*";

        worldItem.ConfigureFromDrag(inItemData);

        if (worldItem.transform.IsChildOf(UiManager.IN.WorldCanvas.transform))
            worldItem.transform.localScale = Vector3.one * inItemData.Scale / DragManager.UiCanvasScaleFactor;
        else
            worldItem.transform.localScale = Vector3.one * inItemData.Scale;

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
                this.decorationParents.Add(dragTarget.ItemContainer.GetInstanceID(), dragTarget.ItemContainer);
            }
        }

        var screenParentObjects = this.screenDecorationCanvas.GetComponentsInChildren<Transform>(true);
        foreach (var parent in screenParentObjects)
        {
            if (parent.TryGetComponent<DragTarget>(out var dragTarget))
            {
                this.decorationParents.Add(dragTarget.ItemContainer.GetInstanceID(), dragTarget.ItemContainer);
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
            var wData = data.DecorationData.WorldSaveData;
            if (this.decorationParents.TryGetValue(wData.ParentGuid, out var foundParent))
            {
                var decoration = SpawnItemInWorld(data, wData.WorldPosition, foundParent);
                decoration.transform.SetSiblingIndex(wData.SiblingIndex);
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
            var wData = data.DecorationData.WorldSaveData;
            Transform parentTrans = null;
            if (this.stoolsAndBenches.TryGetValue(wData.ParentGuid, out var foundParent))
            {
                var childDragTarget = foundParent.GetComponentInChildren<DragTarget>();
                if (childDragTarget != null)
                    parentTrans = childDragTarget.ItemContainer;
            }

            if (parentTrans == null)
            {
                Debug.Log($"<color=red>No parent found for {data.DisplayName} with ParentGuid {wData.ParentGuid}. Spawning under worldDecorationCanvas.</color>");
                parentTrans = this.worldDecorationCanvas.transform;
            }

            var decoration = SpawnItemInWorld(data, wData.WorldPosition, parentTrans);
            decoration.transform.SetSiblingIndex(wData.SiblingIndex);
            this.PlacedDecorations.Add(decoration);
        }
        
        //TODO: loop third time for planters that are children of drag targets on stools/benches that are themselves children of drag targets on stools/benches
    }
}