using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Manages the display of resource UI elements
/// </summary>
public class ResourceDisplayManager : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject resourceDisplayPrefab; // Assign ResourceDisplayUI prefab here
    [SerializeField] private Transform resourceDisplayParent;
    [SerializeField] private ResourceCategory[] categoriesToShow; // Which categories to display
    [SerializeField] private bool showOnlyOwnedResources = true;
    
    private Dictionary<ResourceType, ResourceDisplayUI> activeDisplays = new Dictionary<ResourceType, ResourceDisplayUI>();
    
    private void Start()
    {
        if (ResourceManager.IN?.Database != null)
        {
            CreateResourceDisplays();
        }
    }
    
    private void OnEnable()
    {
        if (ResourceManager.IN != null)
        {
            ResourceManager.OnResourceChanged += OnResourceChanged;
        }
    }
    
    private void OnDisable()
    {
        if (ResourceManager.IN != null)
        {
            ResourceManager.OnResourceChanged -= OnResourceChanged;
        }
    }
    
    private void CreateResourceDisplays()
    {
        if (ResourceManager.IN?.Database == null || resourceDisplayPrefab == null || resourceDisplayParent == null)
            return;
            
        // Get resources to display
        var resourcesToShow = GetResourcesToDisplay();
        
        foreach (var resourceDef in resourcesToShow)
        {
            CreateResourceDisplay(resourceDef);
        }
    }
    
    private ResourceDefinition[] GetResourcesToDisplay()
    {
        var allResources = ResourceManager.IN.Database.AllResources;
        
        if (allResources == null) return new ResourceDefinition[0];
        
        return allResources.Where(r => 
        {
            if (r == null) return false;
            
            // Filter by category if specified
            if (categoriesToShow != null && categoriesToShow.Length > 0)
            {
                bool inCategory = categoriesToShow.Contains(r.category);
                if (!inCategory) return false;
            }
            
            // Filter by ownership if specified
            if (showOnlyOwnedResources)
            {
                int amount = ResourceManager.IN.GetResourceAmount(r.resourceType);
                if (amount <= 0) return false;
            }
            
            return true;
        }).ToArray();
    }
    
    private void CreateResourceDisplay(ResourceDefinition resourceDef)
    {
        GameObject displayObj = Instantiate(resourceDisplayPrefab, resourceDisplayParent);
        ResourceDisplayUI displayUI = displayObj.GetComponent<ResourceDisplayUI>();
        
        if (displayUI != null)
        {
            displayUI.Initialize(resourceDef.resourceType, resourceDef);
            activeDisplays[resourceDef.resourceType] = displayUI;
        }
        else
        {
            Debug.LogError("ResourceDisplayUI component not found on prefab");
            Destroy(displayObj);
        }
    }
    
    private void OnResourceChanged(ResourceType type, int newAmount)
    {
        // If showing only owned resources, create/destroy displays as needed
        if (showOnlyOwnedResources)
        {
            bool hasDisplay = activeDisplays.ContainsKey(type);
            bool shouldHaveDisplay = newAmount > 0;
            
            if (!hasDisplay && shouldHaveDisplay)
            {
                // Create new display
                var resourceDef = ResourceManager.IN.Database?.GetResource(type);
                if (resourceDef != null)
                {
                    CreateResourceDisplay(resourceDef);
                }
            }
            else if (hasDisplay && !shouldHaveDisplay)
            {
                // Remove display
                if (activeDisplays.TryGetValue(type, out ResourceDisplayUI display))
                {
                    activeDisplays.Remove(type);
                    if (display != null)
                        Destroy(display.gameObject);
                }
            }
        }
    }
    
    public void RefreshAllDisplays()
    {
        // Clear existing displays
        foreach (var display in activeDisplays.Values)
        {
            if (display != null)
                Destroy(display.gameObject);
        }
        activeDisplays.Clear();
        
        // Recreate displays
        CreateResourceDisplays();
    }
    
    [ContextMenu("Refresh Displays")]
    private void RefreshDisplaysContextMenu()
    {
        RefreshAllDisplays();
    }
}