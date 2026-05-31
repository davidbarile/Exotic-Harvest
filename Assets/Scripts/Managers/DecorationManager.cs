using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all decorations on the desktop
/// </summary>
public class DecorationManager : MonoBehaviour
{
    public static DecorationManager IN;

    [Header("UI Placement Settings")]
    [SerializeField] private RectTransform worldDecorationsContainer, screenDecorationsContainer; // Canvas for decorations
    
    private Dictionary<int,Transform> decorationParents = new(); // List of parent transforms for different decoration types

    public List<DecorationBase> PlacedDecorations = new();

    private List<DecorationBase> initDecorations = new();

    private void Awake()
    {
        InitDecorationParents();
    }

    /// <summary>
    /// New Game Initialization - can be used to set up any necessary state or spawn default decorations in the world
    /// </summary>
    public void InitDecorationsInWorld(bool isNewGame)
    {
        this.initDecorations = new List<DecorationBase>(this.worldDecorationsContainer.GetComponentsInChildren<DecorationBase>());
        var screenDecorations = new List<DecorationBase>(this.screenDecorationsContainer.GetComponentsInChildren<DecorationBase>());

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

        if (worldItem.ChildDragTarget != null && worldItem.ItemData.DecorationData.Guid == -1)
            worldItem.ItemData.DecorationData.Guid = UnityEngine.Random.Range(0, int.MaxValue);
            
        return worldItem;
    }
    
    private void InitDecorationParents()
    {
        var parentObjects = this.worldDecorationsContainer.GetComponentsInChildren<Transform>(true);
        foreach (var parent in parentObjects)
        {
            if (parent.TryGetComponent<DragTarget>(out var dragTarget))
            {
                this.decorationParents.Add(dragTarget.ItemContainer.GetInstanceID(), dragTarget.ItemContainer);
            }
        }

        var screenParentObjects = this.screenDecorationsContainer.GetComponentsInChildren<Transform>(true);
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

        var itemsWithDragTargets = new Dictionary<int, DragTarget>();
        var itemsWithoutParents = new List<DecorationBase>();
        var spawnedDecorations = new List<DecorationBase>();

        // Recreate decorations from save data
        foreach (var data in savedWorldItems)
        {
            var wData = data.DecorationData.WorldSaveData;
            var success = this.decorationParents.TryGetValue(wData.ParentGuid, out var foundParent);
            var parentTrans = success ? foundParent : this.worldDecorationsContainer.transform;

            var decoration = SpawnItemInWorld(data, wData.WorldPosition, parentTrans);
            decoration.transform.SetSiblingIndex(wData.SiblingIndex);

            if (success)
                this.PlacedDecorations.Add(decoration);
            else
                itemsWithoutParents.Add(decoration);

            spawnedDecorations.Add(decoration);

            if (decoration.ChildDragTarget != null)
                itemsWithDragTargets.Add(decoration.ItemData.DecorationData.Guid, decoration.ChildDragTarget);
        }

        //with remaining items, loop thru spawned decorations and see if any match as a parent, then spawn remaining items
        foreach (var item in itemsWithoutParents)
        {
            var wData = item.ItemData.DecorationData.WorldSaveData;
            Transform parentTrans = null;

            if (itemsWithDragTargets.TryGetValue(wData.ParentGuid, out var foundSubDragTarget))
                parentTrans = foundSubDragTarget.ItemContainer != null ? foundSubDragTarget.ItemContainer : foundSubDragTarget.transform;

            if (parentTrans == null)
            {
                Debug.Log($"<color=red>No parent found for {item.ItemData.DisplayName} with ParentGuid {wData.ParentGuid}. Spawning under worldDecorationCanvas.</color>");
                parentTrans = this.worldDecorationsContainer.transform;
            }

            item.transform.SetParent(parentTrans);
            item.transform.localPosition = wData.WorldPosition;
            item.transform.SetSiblingIndex(wData.SiblingIndex);
            this.PlacedDecorations.Add(item);
        }
    }
}