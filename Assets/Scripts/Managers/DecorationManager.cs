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
    private List<DecorationBase> placedDecorations = new List<DecorationBase>();
    
    // Events
    public static event Action<DecorationBase> OnDecorationAdded;
    public static event Action<DecorationBase> OnDecorationRemoved;
    public static event Action<int> OnDecorationCountChanged;
    
    private void Awake()
    {
        InitializePrefabs();
        
        if (decorationParent == null)
            decorationParent = GetComponent<RectTransform>();
            
        if (decorationCanvas == null)
            decorationCanvas = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
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
        decorationPrefabs = new Dictionary<DecorationType, GameObject>();
        
        if (bucketPrefab != null)
            decorationPrefabs[DecorationType.Bucket] = bucketPrefab;
        if (plantPrefab != null)
            decorationPrefabs[DecorationType.Plant] = plantPrefab;
    }
    
    public bool CanPlaceDecoration(DecorationType type)
    {
        // Check if prefab exists
        if (!decorationPrefabs.ContainsKey(type))
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
            
        GameObject prefab = decorationPrefabs[type];
        GameObject instance = Instantiate(prefab, decorationParent);
        
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
        if (decorationCanvas == null)
            return Vector2.zero;
            
        Rect canvasRect = decorationCanvas.rect;
        
        return new Vector2(
            UnityEngine.Random.Range(canvasRect.xMin + placementPadding.x, canvasRect.xMax - placementPadding.x),
            UnityEngine.Random.Range(canvasRect.yMin + placementPadding.y, canvasRect.yMax - placementPadding.y)
        );
    }
    
    private bool IsValidUIPlacementPosition(Vector2 uiPosition)
    {
        if (decorationCanvas == null)
            return false;
            
        Rect canvasRect = decorationCanvas.rect;
        
        // Check UI bounds with padding
        if (uiPosition.x < canvasRect.xMin + placementPadding.x || 
            uiPosition.x > canvasRect.xMax - placementPadding.x ||
            uiPosition.y < canvasRect.yMin + placementPadding.y || 
            uiPosition.y > canvasRect.yMax - placementPadding.y)
        {
            return false;
        }
        
        // Check for overlapping decorations if using grid placement
        if (useGridPlacement)
        {
            foreach (var decoration in placedDecorations)
            {
                if (decoration != null)
                {
                    RectTransform decorationRect = decoration.GetComponent<RectTransform>();
                    if (decorationRect != null)
                    {
                        float distance = Vector2.Distance(decorationRect.anchoredPosition, uiPosition);
                        if (distance < gridSpacing)
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
        return new List<DecorationBase>(placedDecorations);
    }
    
    public List<T> GetDecorationsOfType<T>() where T : DecorationBase
    {
        List<T> result = new List<T>();
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
        foreach (var decoration in placedDecorations)
        {
            if (decoration.Type == type)
                count++;
        }
        return count;
    }
    
    private void OnDecorationPlaced(DecorationBase decoration)
    {
        if (!placedDecorations.Contains(decoration))
        {
            placedDecorations.Add(decoration);
            OnDecorationAdded?.Invoke(decoration);
            OnDecorationCountChanged?.Invoke(placedDecorations.Count);
        }
    }
    
    private void OnDecorationRemovedHandler(DecorationBase decoration)
    {
        if (placedDecorations.Remove(decoration))
        {
            DecorationManager.OnDecorationRemoved?.Invoke(decoration);
            OnDecorationCountChanged?.Invoke(placedDecorations.Count);
        }
    }
    
    // For save system
    public List<DecorationData> GetSaveData()
    {
        List<DecorationData> saveData = new List<DecorationData>();
        foreach (var decoration in placedDecorations)
        {
            if (decoration != null)
                saveData.Add(decoration.GetSaveData());
        }
        return saveData;
    }
    
    public void LoadSaveData(List<DecorationData> saveData)
    {
        // Clear existing decorations
        for (int i = placedDecorations.Count - 1; i >= 0; i--)
        {
            if (placedDecorations[i] != null)
                Destroy(placedDecorations[i].gameObject);
        }
        placedDecorations.Clear();
        
        // Recreate decorations from save data
        foreach (var data in saveData)
        {
            DecorationBase decoration = PlaceDecoration(data.type, data.position);
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