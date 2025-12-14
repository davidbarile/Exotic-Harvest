using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all decorations on the desktop
/// </summary>
public class DecorationManager : MonoBehaviour
{
    public static DecorationManager IN;
    
    [Header("Decoration Prefabs")]
    [SerializeField] private GameObject bucketPrefab;
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] private GameObject[] allDecorationPrefabs; // Array for all decoration types
    
    [Header("UI Placement Settings")]
    [SerializeField] private RectTransform decorationCanvas; // Canvas for decorations
    [SerializeField] private RectTransform decorationParent; // UI container
    [SerializeField] private Vector2 placementPadding = new Vector2(100f, 100f); // Padding from edges
    [SerializeField] private float gridSpacing = 80f; // UI spacing
    [SerializeField] private bool useGridPlacement = true;
    
    private Dictionary<DecorationType, GameObject> decorationPrefabs;
    private List<DecorationBase> placedDecorations = new();
    
    // Events
    public static event Action<DecorationBase> OnDecorationAdded;
    public static event Action<DecorationBase> OnDecorationRemoved;
    public static event Action<int> OnDecorationCountChanged;
    
    private void Awake()
    {
        InitializePrefabs();
        
        if (this.decorationParent == null)
            this.decorationParent = GetComponent<RectTransform>();
            
        if (this.decorationCanvas == null)
            this.decorationCanvas = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
    }
    
    private void OnEnable()
    {
        DecorationBase.OnDecorationPlaced += OnDecorationPlaced;
        DecorationBase.OnDecorationRemoved += OnDecorationRemoved;
    }
    
    private void OnDisable()
    {
        DecorationBase.OnDecorationPlaced -= OnDecorationPlaced;
        DecorationBase.OnDecorationRemoved -= OnDecorationRemoved;
    }
    
    private void InitializePrefabs()
    {
        this.decorationPrefabs = new();
        
        if (this.bucketPrefab != null)
            this.decorationPrefabs[DecorationType.Bucket] = this.bucketPrefab;
        if (this.plantPrefab != null)
            this.decorationPrefabs[DecorationType.Plant] = this.plantPrefab;
    }
    
    public bool CanPlaceDecoration(DecorationType type)
    {
        // Check if prefab exists
        if (!this.decorationPrefabs.ContainsKey(type))
            return false;
            
        // Check if player can afford it (will be implemented with shop)
        // For now, allow unlimited placement
        return true;
    }
    
    public DecorationBase PlaceDecoration(DecorationType type, Vector2 uiPosition)
    {
        if (!CanPlaceDecoration(type))
            return null;
            
        if (!IsValidUIPlacementPosition(uiPosition))
            return null;
            
        GameObject prefab = this.decorationPrefabs[type];
        GameObject instance = Instantiate(prefab, this.decorationParent);
        
        // Set UI position
        RectTransform instanceRect = instance.GetComponent<RectTransform>();
        if (instanceRect != null)
        {
            instanceRect.anchoredPosition = uiPosition;
        }
        
        DecorationBase decoration = instance.GetComponent<DecorationBase>();
        if (decoration != null)
        {
            return decoration;
        }
        
        // Fallback if no DecorationBase component
        Destroy(instance);
        return null;
    }
    
    public DecorationBase PlaceDecoration(DecorationType type)
    {
        Vector2 randomPosition = GetRandomUIPlacementPosition();
        return PlaceDecoration(type, randomPosition);
    }
    
    private Vector2 GetRandomUIPlacementPosition()
    {
        if (this.decorationCanvas == null)
            return Vector2.zero;
            
        Rect canvasRect = this.decorationCanvas.rect;
        
        return new Vector2(
            UnityEngine.Random.Range(canvasRect.xMin + this.placementPadding.x, canvasRect.xMax - this.placementPadding.x),
            UnityEngine.Random.Range(canvasRect.yMin + this.placementPadding.y, canvasRect.yMax - this.placementPadding.y)
        );
    }
    
    private bool IsValidUIPlacementPosition(Vector2 uiPosition)
    {
        if (this.decorationCanvas == null)
            return false;
            
        Rect canvasRect = this.decorationCanvas.rect;
        
        // Check UI bounds with padding
        if (uiPosition.x < canvasRect.xMin + this.placementPadding.x || 
            uiPosition.x > canvasRect.xMax - this.placementPadding.x ||
            uiPosition.y < canvasRect.yMin + this.placementPadding.y || 
            uiPosition.y > canvasRect.yMax - this.placementPadding.y)
        {
            return false;
        }
        
        // Check for overlapping decorations if using grid placement
        if (this.useGridPlacement)
        {
            foreach (var decoration in this.placedDecorations)
            {
                if (decoration != null)
                {
                    RectTransform decorationRect = decoration.GetComponent<RectTransform>();
                    if (decorationRect != null)
                    {
                        float distance = Vector2.Distance(decorationRect.anchoredPosition, uiPosition);
                        if (distance < this.gridSpacing)
                            return false;
                    }
                }
            }
        }
        
        return true;
    }
    
    public void RemoveDecoration(DecorationBase decoration)
    {
        if (decoration != null)
            decoration.Remove();
    }
    
    public List<DecorationBase> GetAllDecorations()
    {
        return new(this.placedDecorations);
    }
    
    public List<T> GetDecorationsOfType<T>() where T : DecorationBase
    {
        List<T> result = new();
        foreach (var decoration in placedDecorations)
        {
            if (decoration is T typed)
                result.Add(typed);
        }
        return result;
    }
    
    public int GetDecorationCount(DecorationType type)
    {
        int count = 0;
        foreach (var decoration in this.placedDecorations)
        {
            if (decoration.Type == type)
                count++;
        }
        return count;
    }
    
    private void OnDecorationPlaced(DecorationBase decoration)
    {
        if (!this.placedDecorations.Contains(decoration))
        {
            this.placedDecorations.Add(decoration);
            OnDecorationAdded?.Invoke(decoration);
            OnDecorationCountChanged?.Invoke(this.placedDecorations.Count);
        }
    }
    
    private void OnDecorationRemovedHandler(DecorationBase decoration)
    {
        if (this.placedDecorations.Remove(decoration))
        {
            DecorationManager.OnDecorationRemoved?.Invoke(decoration);
            OnDecorationCountChanged?.Invoke(this.placedDecorations.Count);
        }
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
            DecorationBase decoration = PlaceDecoration(data.Type, data.Position);
            if (decoration != null)
                decoration.LoadSaveData(data);
        }
    }
    
    // Utility methods for Phase 1
    public void SpawnInitialDecorations()
    {
        // Place a few starting decorations
        PlaceDecoration(DecorationType.Bucket, new Vector3(-3f, -2f, 0f));
        PlaceDecoration(DecorationType.Plant, new Vector3(3f, 2f, 0f));
    }
}